﻿using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Messages.Interfaces;
using ChatterBox.Common.Communication.Messages.Peers;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Messages.Standard;
using Common.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ChatterBox.Server
{
    public class RegisteredClient : IClientChannel, IServerChannel
    {
        public RegisteredClient()
        {
            ClientReadProxy = new ChannelInvoker(this);
        }

        private TcpClient ActiveConnection { get; set; }
        public int Avatar { get; set; }
        private ChannelWriteHelper ChannelWriteHelper { get; } = new ChannelWriteHelper(typeof(IServerChannel));
        private ChannelInvoker ClientReadProxy { get; }
        private Guid ConnectionId { get; set; }
        public string Domain { get; set; }
        public bool IsOnline { get; private set; }
        private ILog Logger => LogManager.GetLogger(ToString());

        private PushNotificationSender _pushNotificationSender;
        private ConcurrentQueue<RegisteredClientMessageQueueItem> MessageQueue { get; set; } =
            new ConcurrentQueue<RegisteredClientMessageQueueItem>();

        public string Name { get; set; }

        public string UserId { get; set; }
        private ConcurrentQueue<string> WriteQueue { get; set; } = new ConcurrentQueue<string>();

        #region IClientChannel Members

        public void ClientConfirmation(Confirmation confirmation)
        {
            var message = MessageQueue.SingleOrDefault(s => s.Message.Id == confirmation.ConfirmationFor);
            if (message == null) return;
            message.IsDelivered = true;
        }

        public void ClientHeartBeat()
        {
        }

        public void GetPeerList(Message message)
        {
            OnGetPeerList?.Invoke(this, message);
        }

        public void Register(Registration message)
        {
        }

        public void Relay(RelayMessage message)
        {
            ServerConfirmation(Confirmation.For(message));
            OnRelayMessage?.Invoke(this, message);
        }

        #endregion

        #region IServerChannel Members

        public void OnPeerList(PeerList peerList)
        {
            EnqueueMessage(peerList);
        }

        public void OnPeerPresence(PeerUpdate peer)
        {
            EnqueueMessage(peer);
        }

        public void OnRegistrationConfirmation(RegisteredReply reply)
        {
            EnqueueMessage(reply);
        }

        public void ServerConfirmation(Confirmation confirmation)
        {
            EnqueueOutput(confirmation);
        }

        public void ServerError(ErrorReply reply)
        {
            EnqueueMessage(reply);
        }

        public void ServerHeartBeat()
        {
            if (ActiveConnection == null) return;
            EnqueueOutput();
        }

        public void ServerReceivedInvalidMessage(InvalidMessage reply)
        {
            EnqueueOutput(reply);
        }

        public void ServerRelay(RelayMessage message)
        {
            EnqueueMessage(message);
        }

        public void ServerConnectionError()
        {
        }

        #endregion

        private void EnqueueMessage(IMessage message, [CallerMemberName] string method = null)
        {
            var serializedString = ChannelWriteHelper.FormatOutput(message, method);
            var queueItem = new RegisteredClientMessageQueueItem
            {
                SerializedMessage = serializedString,
                Message = message,
                Method = method
            };

            if (ActiveConnection == null)
                _pushNotificationSender?.SendNotification(queueItem.SerializedMessage);

            MessageQueue.Enqueue(queueItem);
        }

        private void EnqueueOutput(object message = null, [CallerMemberName] string method = null)
        {
            WriteQueue.Enqueue(ChannelWriteHelper.FormatOutput(message, method));
        }

        public event Action<RegisteredClient> OnConnected;
        public event Action<RegisteredClient> OnDisconnected;
        public event Action<RegisteredClient, IMessage> OnGetPeerList;
        public event Action<RegisteredClient, RelayMessage> OnRelayMessage;

        private void OnTcpClientDisconnected(Guid oldConnectionID)
        {
            if (oldConnectionID == ConnectionId)
            {
                var itemsToSend = MessageQueue.ToList();

                ActiveConnection = null;
                if (!IsOnline) return;
                IsOnline = false;
                OnDisconnected?.Invoke(this);

                if (_pushNotificationSender != null)
                {
                    foreach (var item in itemsToSend)
                    {
                        _pushNotificationSender.SendNotification(item.SerializedMessage);
                    }
                }
            }
        }

        private void ResetQueues()
        {
            WriteQueue = new ConcurrentQueue<string>();
            var queuedMessages = MessageQueue.OrderBy(s => s.Message.SentDateTimeUtc).ToList();
            MessageQueue = new ConcurrentQueue<RegisteredClientMessageQueueItem>();

            var confirmationMessage = queuedMessages.Last(s => s.Method == nameof(OnRegistrationConfirmation));
            queuedMessages.RemoveAll(s => s.Method == nameof(OnRegistrationConfirmation));
            queuedMessages.Insert(0, confirmationMessage);

            foreach (var queuedMessage in queuedMessages)
            {
                queuedMessage.IsSent = false;
                queuedMessage.IsDelivered = false;
                MessageQueue.Enqueue(queuedMessage);
            }
        }

        public void SetActiveConnection(UnregisteredConnection connection, Registration message)
        {
            Logger.Debug("Handling new TCP connection.");

            ConnectionId = Guid.NewGuid();
            ActiveConnection = connection.TcpClient;

            RegisterClientForPushNotifications(message.PushNotificationChannelURI);

            OnRegistrationConfirmation(new RegisteredReply
            {
                Avatar = Avatar,
                ReplyFor = message.Id
            });
            ResetQueues();
            StartReading();
            StartWriting();
            IsOnline = true;
            StartMessageQueueProcessing();
            OnConnected?.Invoke(this);
        }

        private void StartMessageQueueProcessing()
        {
            Task.Run(async () =>
            {
                while (IsOnline)
                {
                    while (!MessageQueue.IsEmpty)
                    {
                        RegisteredClientMessageQueueItem item;
                        if (!MessageQueue.TryPeek(out item)) break;

                        if (!item.IsSent)
                        {
                            item.IsSent = true;
                            WriteQueue.Enqueue(item.SerializedMessage);
                        }
                        else
                        {
                            if (!item.IsDelivered) break;
                            if (!MessageQueue.TryDequeue(out item)) break;
                        }
                    }

                    await Task.Delay(10);
                }
            });
        }

        private void StartReading()
        {
            Task.Run(async () =>
            {
                var connectionId = ConnectionId;
                try
                {
                    var reader = new StreamReader(ActiveConnection.GetStream());
                    while (IsOnline && connectionId == ConnectionId)
                    {
                        var message = await reader.ReadLineAsync();
                        if (message == null) break;
                        Logger.Trace($">> {message}");
                        if (!ClientReadProxy.ProcessRequest(message).Invoked)
                        {
                            ServerReceivedInvalidMessage(InvalidMessage.For(message));
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.Warn($"[READ] Disconnected. Reason: {exception.Message}");
                    OnTcpClientDisconnected(connectionId);
                }
            });
        }

        private void StartWriting()
        {
            Task.Run(async () =>
            {
                var connectionId = ConnectionId;
                try
                {
                    var writer = new StreamWriter(ActiveConnection.GetStream())
                    {
                        AutoFlush = true
                    };
                    while (IsOnline && connectionId == ConnectionId)
                    {
                        while (!WriteQueue.IsEmpty)
                        {
                            string message;
                            if (!WriteQueue.TryDequeue(out message)) continue;

                            Logger.Debug($"<< {message}");
                            await writer.WriteLineAsync(message);
                        }
                        await Task.Delay(10);
                    }
                }
                catch (Exception exception)
                {
                    Logger.Warn($"[WRITE] Disconnected. Reason: {exception.Message}");
                    OnTcpClientDisconnected(connectionId);
                }
            });
        }

        public override string ToString()
        {
            return $"[{Domain}/{Name}]";
        }

        private void OnUserChannelURIExpired()
        {
            _pushNotificationSender.ChannelURI = null;
        }

        public bool RegisterClientForPushNotifications(string channelURI)
        {
            bool ret = false;
            if (!String.IsNullOrEmpty(channelURI))
            {
                _pushNotificationSender = new PushNotificationSender();
                _pushNotificationSender.OnChannelURIExpired += OnUserChannelURIExpired;
                _pushNotificationSender.ChannelURI = channelURI;
                ret = true;
            }

            return ret;
        }
    }
}