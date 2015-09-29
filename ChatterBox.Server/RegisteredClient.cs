using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Messages.Interfaces;
using ChatterBox.Common.Communication.Messages.Peers;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Standard;
using Common.Logging;

namespace ChatterBox.Server
{
    public class RegisteredClient : IClientChannel, IServerChannel
    {
        private ILog Logger => LogManager.GetLogger(ToString());
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        public string PushToken { get; set; }

        private ChannelInvoker ClientReadProxy { get; }
        private ChannelWriteHelper ChannelWriteHelper { get; } = new ChannelWriteHelper(typeof(IServerChannel));

        private TcpClient ActiveConnection { get; set; }
        private ConcurrentQueue<RegisteredClientMessageQueueItem> MessageQueue { get; set; } = new ConcurrentQueue<RegisteredClientMessageQueueItem>();
        private ConcurrentQueue<string> WriteQueue { get; set; } = new ConcurrentQueue<string>();
        public bool IsOnline { get; private set; }


        public RegisteredClient()
        {
            ClientReadProxy = new ChannelInvoker(this);
        }




        public void SetActiveConnection(UnregisteredConnection connection, Registration message)
        {
            Logger.Debug("Handling new TCP connection.");
            ActiveConnection = connection.TcpClient;
            OnRegistrationConfirmation(OkReply.For(message));
            ResetQueues();
            StartReading();
            StartWriting();
            StartMessageQueueProcessing();
            IsOnline = true;
            OnConnected?.Invoke(this);
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

        private void StartReading()
        {
            Task.Run(async () =>
            {
                try
                {
                    var reader = new StreamReader(ActiveConnection.GetStream());
                    while (true)
                    {
                        var message = await reader.ReadLineAsync();
                        if (message == null) break;
                        Logger.Trace($"Received: {message}");
                        if (!ClientReadProxy.ProcessRequest(message))
                        {
                            ServerReceivedInvalidMessage(InvalidMessage.For(message));
                        }
                    }

                }
                catch (Exception exception)
                {
                    Logger.Warn($"[READ] Disconnected. Reason: {exception.Message}");
                    OnTcpClientDisconnected();
                }
            });
        }
        private void StartWriting()
        {
            Task.Run(async () =>
            {
                try
                {
                    var writer = new StreamWriter(ActiveConnection.GetStream())
                    {
                        AutoFlush = true
                    };
                    while (IsOnline)
                    {
                        while (!WriteQueue.IsEmpty)
                        {
                            string message;
                            if (!WriteQueue.TryDequeue(out message)) continue;

                            Logger.Debug($"Sent: {message}");
                            await writer.WriteLineAsync(message);
                        }
                        await Task.Delay(100);
                    }

                }
                catch (Exception exception)
                {
                    Logger.Warn($"[WRITE] Disconnected. Reason: {exception.Message}");
                    OnTcpClientDisconnected();
                }
            });
        }
        private void StartMessageQueueProcessing()
        {
            Task.Run(async () =>
            {
                while (true)
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

                    await Task.Delay(100);
                }
            });
        }




        public void Register(Registration message)
        {
        }

        public void ClientConfirmation(Confirmation confirmation)
        {
            var message = MessageQueue.SingleOrDefault(s => s.Message.Id == confirmation.ConfirmationFor);
            if (message == null) return;
            message.IsDelivered = true;
        }

        public void ClientHeartBeat()
        {
        }




        public void ServerConfirmation(Confirmation confirmation)
        {
            EnqueueOutput(confirmation);
        }
        public void ServerReceivedInvalidMessage(InvalidMessage reply)
        {
            EnqueueOutput(reply);
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

        public void OnPeerPresence(PeerInformation peer)
        {
            EnqueueMessage(peer);
        }

        public void OnPeerList(PeerList peerList)
        {
            EnqueueMessage(peerList);
        }

        public void OnRegistrationConfirmation(OkReply reply)
        {
            EnqueueMessage(reply);
        }




        private void EnqueueMessage(IMessage message, [CallerMemberName] string method = null)
        {
            var serializedString = ChannelWriteHelper.FormatOutput(message, method);
            var queueItem = new RegisteredClientMessageQueueItem
            {
                SerializedMessage = serializedString,
                Message = message,
                Method = method
            };
            MessageQueue.Enqueue(queueItem);
        }

        private void EnqueueOutput(object message = null, [CallerMemberName] string method = null)
        {
            WriteQueue.Enqueue(ChannelWriteHelper.FormatOutput(message, method));
        }

        private void OnTcpClientDisconnected()
        {
            ActiveConnection = null;
            if (!IsOnline) return;
            IsOnline = false;
            OnDisconnected?.Invoke(this);
        }
        public override string ToString()
        {
            return $"Client[{Domain}/{UserId}/{Name}]";
        }

        public void GetPeerList(Message message)
        {
            OnGetPeerList?.Invoke(this, message);
        }

        public event OnRegisteredClientEventHandler OnConnected;
        public event OnRegisteredClientEventHandler OnDisconnected;
        public event OnRegisteredClientMessageEventHandler OnGetPeerList;
        public delegate void OnRegisteredClientMessageEventHandler(RegisteredClient sender, IMessage message);
        public delegate void OnRegisteredClientEventHandler(RegisteredClient sender);


    }
}