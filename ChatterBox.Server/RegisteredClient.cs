using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ChatterBox.Shared.Communication.Contracts;
using ChatterBox.Shared.Communication.Helpers;
using ChatterBox.Shared.Communication.Messages.Interfaces;
using ChatterBox.Shared.Communication.Messages.Registration;
using ChatterBox.Shared.Communication.Messages.Standard;
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

        private ChannelInvoker<IClientChannel> ClientReadProxy { get; }
        private TcpClient ActiveConnection { get; set; }
        private ConcurrentQueue<MessageQueueItem> MessageQueue { get; } = new ConcurrentQueue<MessageQueueItem>();
        private ConcurrentQueue<string> WriteQueue { get; set; } = new ConcurrentQueue<string>();


        public RegisteredClient()
        {
            ClientReadProxy = new ChannelInvoker<IClientChannel>(this);
        }




        public void SetActiveConnection(UnregisteredConnection connection, RegistrationMessage message)
        {
            Logger.Debug("Handling new TCP connection.");
            ActiveConnection = connection.TcpClient;
            WriteQueue = new ConcurrentQueue<string>();
            RegistrationConfirmation(OkReply.For(message));
            StartReading();
            StartWriting();
            StartMessageQueueProcessing();
        }
        private void StartReading()
        {
            Task.Run(async () =>
            {
                try
                {
                    using (var reader = new StreamReader(ActiveConnection.GetStream()))
                    {
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

                }
                catch (Exception exception)
                {
                    Logger.Warn($"Disconnected. Reason: {exception.Message}");
                    OnDisconnected();
                }
            });
        }
        private void StartWriting()
        {
            Task.Run(async () =>
            {
                try
                {
                    using (var writer = new StreamWriter(ActiveConnection.GetStream())
                    {
                        AutoFlush = true
                    })
                    {
                        while (true)
                        {
                            while (!WriteQueue.IsEmpty)
                            {
                                string message;
                                if (!WriteQueue.TryDequeue(out message)) continue;

                                Logger.Trace($"Sent: {message}");
                                await writer.WriteLineAsync(message);
                            }
                            await Task.Delay(100);
                        }
                    }

                }
                catch (Exception exception)
                {
                    Logger.Warn($"Disconnected. Reason: {exception.Message}");
                    OnDisconnected();
                }
            });
        }
        private void StartMessageQueueProcessing()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (MessageQueue.IsEmpty) continue;
                    while (true)
                    {
                        MessageQueueItem item;
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




        public void Register(RegistrationMessage message)
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
            WriteQueue.Enqueue(ChannelWriteHelper<IServerChannel>.FormatOutput(confirmation));
        }
        public void ServerReceivedInvalidMessage(InvalidMessage reply)
        {
            WriteQueue.Enqueue(ChannelWriteHelper<IServerChannel>.FormatOutput(reply));
        }
        public void ServerError(ErrorReply reply)
        {
            Enqueue(reply);
        }
        public void ServerHeartBeat()
        {
            if (ActiveConnection == null) return;
            WriteQueue.Enqueue(ChannelWriteHelper<IServerChannel>.FormatOutput());
        }
        
        public void RegistrationConfirmation(OkReply reply)
        {
            Enqueue(reply);
        }
        



        private void Enqueue(IMessage message, [CallerMemberName]string method = null)
        {
            var serializedString = ChannelWriteHelper<IServerChannel>.FormatOutput(message, method);
            MessageQueue.Enqueue(new MessageQueueItem
            {
                SerializedMessage = serializedString,
                Message = message
            });
        }
        private void OnDisconnected()
        {
            ActiveConnection = null;
        }
        public override string ToString()
        {
            return $"Client[{Domain}/{UserId}/{Name}]";
        }

        private class MessageQueueItem
        {
            public string SerializedMessage { get; set; }

            public IMessage Message { get; set; }

            public bool IsDelivered { get; set; }
            public bool IsSent { get; set; }
        }
    }
}