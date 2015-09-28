using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using ChatterBox.Shared.Communication.Contracts;
using ChatterBox.Shared.Communication.Helpers;
using ChatterBox.Shared.Communication.Messages.Peers;
using ChatterBox.Shared.Communication.Messages.Registration;
using ChatterBox.Shared.Communication.Messages.Standard;

namespace ChatterBox.Client.Signaling
{
    public sealed class ChatterBoxClient : IClientChannel, IServerChannel
    {
        private readonly ISignalingSocketService _signalingSocketService;

        private ChannelInvoker<IServerChannel> ServerReadProxy { get; }




        public ChatterBoxClient(ISignalingSocketService signalingSocketService)
        {
            _signalingSocketService = signalingSocketService;
            ServerReadProxy = new ChannelInvoker<IServerChannel>(this);
        }


        public async void Register(Registration message)
        {
            await SendToServer(ChannelWriteHelper<IClientChannel>.FormatOutput(message));
        }

        public async void ClientConfirmation(Confirmation confirmation)
        {
            await SendToServer(ChannelWriteHelper<IClientChannel>.FormatOutput(confirmation));
        }



        public async void GetPeerList(Message message)
        {
            await SendToServer(ChannelWriteHelper<IClientChannel>.FormatOutput(message));
        }

        public void ClientHeartBeat()
        {

        }




        public void ServerConfirmation(Confirmation confirmation)
        {
        }

        public void ServerReceivedInvalidMessage(InvalidMessage reply)
        {

        }

        public void ServerError(ErrorReply reply)
        {

        }

        public void OnPeerPresence(PeerInformation peer)
        {
            ClientConfirmation(Confirmation.For(peer));
            GetPeerList(new Message());
        }

        public void OnPeerList(PeerList peerList)
        {
            ClientConfirmation(Confirmation.For(peerList));
            foreach (var peer in peerList.Peers)
            {

            }
        }


        public void OnRegistrationConfirmation(OkReply reply)
        {
            ClientConfirmation(Confirmation.For(reply));
        }



        public void ServerHeartBeat()
        {

        }



        private async Task SendToServer(string message)
        {
            var socket = _signalingSocketService.GetSocket();
            if (socket != null)
            {

                using (var writer = new DataWriter(socket.OutputStream))
                {
                    writer.WriteString($"{message}{Environment.NewLine}");
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                }
                _signalingSocketService.HandleSocket(socket);
            }
        }
    }
}
