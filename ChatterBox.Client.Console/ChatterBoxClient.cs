using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatterBox.Shared.Communication.Contracts;
using ChatterBox.Shared.Communication.Helpers;
using ChatterBox.Shared.Communication.Messages.Registration;
using ChatterBox.Shared.Communication.Messages.Standard;
using Common.Logging;

namespace ChatterBox.Client.Console
{
    public class ChatterBoxClient : IClientChannel, IServerChannel
    {

        public string Label { get; set; }

        private ILog Logger => LogManager.GetLogger(string.IsNullOrWhiteSpace(Label) ? nameof(ChatterBoxClient) : Label);
        private ChannelInvoker<IServerChannel> ServerReadProxy { get; }
        private ConcurrentQueue<string> WriteQueue { get; } = new ConcurrentQueue<string>();
        private TcpClient TcpClient { get; } = new TcpClient();
        private StreamReader _reader;
        private StreamWriter _writer;




        public ChatterBoxClient()
        {
            ServerReadProxy = new ChannelInvoker<IServerChannel>(this);
        }


        public void Connect(string host = "localhost", int port = 50000)
        {
            TcpClient.Connect(host, port);
            var networkStream = TcpClient.GetStream();
            _reader = new StreamReader(networkStream);
            _writer = new StreamWriter(networkStream) { AutoFlush = true };
            StartReading();
            StartWriting();
        }


        private void StartReading()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        var request = await _reader.ReadLineAsync();
                        if (request == null) break;
                        Logger.Debug($"RECEIVED: {request}");
                        ServerReadProxy.ProcessRequest(request);
                    }
                }
                catch (Exception exception)
                {
                    Logger.Warn("Disconnected during socket read operation due to exception",exception);
                }
            });
        }

        private void StartWriting()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        while (!WriteQueue.IsEmpty)
                        {
                            string message;
                            if (!WriteQueue.TryDequeue(out message)) continue;
                            Logger.Trace($"SENT: {message}");
                            await _writer.WriteLineAsync(message);
                        }
                        await Task.Delay(200);
                    }
                }
                catch (Exception exception)
                {
                    Logger.Warn("Disconnected during socket WRITE operation due to exception", exception); 
                }

            });
        }




        public void Register(RegistrationMessage request)
        {
            WriteQueue.Enqueue(ChannelWriteHelper<IClientChannel>.FormatOutput(request));
        }

        public void ClientConfirmation(Confirmation confirmation)
        {
            WriteQueue.Enqueue(ChannelWriteHelper<IClientChannel>.FormatOutput(confirmation));
        }

        public void ClientHeartBeat()
        {
            
        }


        public void ServerConfirmation(Confirmation confirmation)
        {
        }

        public void ServerReceivedInvalidMessage(InvalidMessage reply)
        {
            Logger.Error($"Server signaled invalid message: {reply.OriginalMessage}");
        }

        public void ServerError(ErrorReply reply)
        {
            Logger.Error($"Server signaled error for message {reply.ReplyFor} with message {reply.ErrorMessage}");
        }

        public void RegistrationConfirmation(OkReply reply)
        {
            ClientConfirmation(Confirmation.For(reply));
        }

       

        public void ServerHeartBeat()
        {
            
        }
    }
}
