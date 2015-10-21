using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Messages.Peers;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Standard;
using ChatterBox.Common.Communication.Shared.Messages.Registration;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using Common.Logging;

namespace ChatterBox.Server
{
    public class UnregisteredConnection : IClientChannel, IServerChannel
    {
        public delegate void OnRegisterHandler(UnregisteredConnection sender, Registration message);

        public UnregisteredConnection(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }

        private ChannelWriteHelper ChannelWriteHelper { get; } = new ChannelWriteHelper(typeof (IServerChannel));
        public Guid Id { get; } = Guid.NewGuid();
        private ILog Logger => LogManager.GetLogger(ToString());
        public TcpClient TcpClient { get; set; }

        public void ClientConfirmation(Confirmation confirmation)
        {
        }

        public void ClientHeartBeat()
        {
        }

        public void GetPeerList(Message message)
        {
        }

        public void Register(Registration message)
        {
            ServerConfirmation(Confirmation.For(message));
            OnRegister?.Invoke(this, message);
        }

        public void Relay(RelayMessage message)
        {
        }

        public void OnPeerList(PeerList peerList)
        {
        }

        public void OnPeerPresence(PeerUpdate peer)
        {
        }

        public void OnRegistrationConfirmation(RegisteredReply reply)
        {
        }

        public void ServerConfirmation(Confirmation confirmation)
        {
            Write(confirmation);
        }

        public void ServerError(ErrorReply reply)
        {
        }

        public void ServerHeartBeat()
        {
        }

        public void ServerReceivedInvalidMessage(InvalidMessage reply)
        {
            Write(reply);
        }

        public void ServerRelay(RelayMessage message)
        {
        }

        public void OnInvalidRequest(InvalidMessage reply)
        {
        }

        public event OnRegisterHandler OnRegister;

        public override string ToString()
        {
            return $"{nameof(UnregisteredConnection)}[{Id}]";
        }

        public void WaitForRegistration()
        {
            Task.Run(async () =>
            {
                var reader = new StreamReader(TcpClient.GetStream());
                var clientChannelProxy = new ChannelInvoker(this);
                var message = await reader.ReadLineAsync();
                if (!clientChannelProxy.ProcessRequest(message).Invoked)
                {
                    OnInvalidRequest(InvalidMessage.For(message));
                }
            });
        }

        private void Write(object arg = null, [CallerMemberName] string method = null)
        {
            var message = ChannelWriteHelper.FormatOutput(arg, method);
            var writer = new StreamWriter(TcpClient.GetStream())
            {
                AutoFlush = true
            };
            writer.WriteLine(message);
        }
    }
}