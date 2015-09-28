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
using Common.Logging;

namespace ChatterBox.Server
{
    public class UnregisteredConnection : IClientChannel, IServerChannel
    {
        private ILog Logger => LogManager.GetLogger(ToString());
        public Guid Id { get; } = Guid.NewGuid();

        public TcpClient TcpClient { get; set; }
        private ChannelWriteHelper ChannelWriteHelper { get; } = new ChannelWriteHelper(typeof(IServerChannel));

        public UnregisteredConnection(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }

        public void WaitForRegistration()
        {
            Task.Run(async () =>
            {
                var reader = new StreamReader(TcpClient.GetStream());
                var clientChannelProxy = new ChannelInvoker(this);
                var message = await reader.ReadLineAsync();
                if (!clientChannelProxy.ProcessRequest(message))
                {
                    OnInvalidRequest(InvalidMessage.For(message));
                }
            });

        }


        public event OnRegisterHandler OnRegister;
        public delegate void OnRegisterHandler(UnregisteredConnection sender, Registration message);


        public override string ToString()
        {
            return $"{nameof(UnregisteredConnection)}[{Id}]";
        }

        public void Register(Registration message)
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
            Write(confirmation);

        }

        public void ServerReceivedInvalidMessage(InvalidMessage reply)
        {
            Write(reply);
        }

        public void ServerError(ErrorReply reply)
        {
        }

        public void OnPeerPresence(PeerInformation peer)
        {

        }

        public void OnPeerList(PeerList peerList)
        {

        }

        public void OnRegistrationConfirmation(OkReply reply)
        {
        }

        public void OnInvalidRequest(InvalidMessage reply)
        {
        }

        public void ServerHeartBeat()
        {
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

        public void GetPeerList(Message message)
        {
        }
    }
}
