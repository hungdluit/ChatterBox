using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatterBox.Shared.Communication.Contracts;
using ChatterBox.Shared.Communication.Helpers;
using ChatterBox.Shared.Communication.Messages.Registration;
using ChatterBox.Shared.Communication.Messages.Standard;
using Common.Logging;

namespace ChatterBox.Server
{
    public class UnregisteredConnection : IClientChannel, IServerChannel
    {
        private ILog Logger => LogManager.GetLogger(ToString());
        public Guid Id { get; } = Guid.NewGuid();

        public TcpClient TcpClient { get; set; }

        public UnregisteredConnection(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }

        public void WaitForRegistration()
        {
            Task.Run(async () =>
            {
                var reader = new StreamReader(TcpClient.GetStream());
                var clientChannelProxy = new ChannelInvoker<IClientChannel>(this);
                var message = await reader.ReadLineAsync();
                if (!clientChannelProxy.ProcessRequest(message))
                {
                    OnInvalidRequest(InvalidMessage.For(message));
                }
            });

        }


        public event OnRegisterHandler OnRegister;
        public delegate void OnRegisterHandler(UnregisteredConnection sender, RegistrationMessage message);


        public override string ToString()
        {
            return $"{nameof(UnregisteredConnection)}[{Id}]";
        }

        public void Register(RegistrationMessage message)
        {
            ServerConfirmation(Confirmation.For(message));
            OnRegister?.Invoke(this, message);
        }

        public void ClientConfirmation(Confirmation confirmation)
        {
            
        }

        public void ClientHeartBeat()
        {
        }


        public void ServerConfirmation(Confirmation confirmation)
        {
            var message = ChannelWriteHelper<IServerChannel>.FormatOutput(confirmation);
            Write(message);

        }

        public void ServerReceivedInvalidMessage(InvalidMessage reply)
        {
            var message = ChannelWriteHelper<IServerChannel>.FormatOutput(reply);
            Write(message);
        }

        public void ServerError(ErrorReply reply)
        {
        }

        public void RegistrationConfirmation(OkReply reply)
        {
        }

        public void OnInvalidRequest(InvalidMessage reply)
        {
        }

        public void ServerHeartBeat()
        {
        }

        private void Write(string message)
        {
            var writer = new StreamWriter(TcpClient.GetStream())
            {
                AutoFlush = true
            };
            writer.WriteLine(message);
        }
    }
}
