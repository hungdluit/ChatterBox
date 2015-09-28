using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using ChatterBox.Shared.Communication.Contracts;
using ChatterBox.Shared.Communication.Helpers;
using ChatterBox.Shared.Communication.Messages.Peers;
using ChatterBox.Shared.Communication.Messages.Registration;
using ChatterBox.Shared.Communication.Messages.Standard;

namespace ChatterBox.Client.Tasks.Signaling.Shared
{
    public class SignalingClient : IClientChannel, IServerChannel
    {
        private ChannelInvoker<IServerChannel> ServerReadProxy { get; }





        public SignalingClient()
        {
            ServerReadProxy = new ChannelInvoker<IServerChannel>(this);
        }


        public async Task<bool> Connect()
        {
            return await SocketController.Connect(Guid.Empty);
        }





        public async void Register(Registration request)
        {
            await SendToServer(ChannelWriteHelper<IClientChannel>.FormatOutput(request));
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
            //Logger.Error($"Server signaled invalid message: {reply.OriginalMessage}");
        }

        public void ServerError(ErrorReply reply)
        {
            //Logger.Error($"Server signaled error for message {reply.ReplyFor} with message {reply.ErrorMessage}");
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
                //Logger.Trace($"{peer.Name} - {(peer.IsOnline ? "Online" : "Offline")}");
            }
        }


        public void OnRegistrationConfirmation(OkReply reply)
        {
            ClientConfirmation(Confirmation.For(reply));
        }



        public void ServerHeartBeat()
        {

        }


        private async Task SendToServer(string formatOutput)
        {
            
        }
    }
}
