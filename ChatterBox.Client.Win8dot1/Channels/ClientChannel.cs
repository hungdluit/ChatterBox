using ChatterBox.Common.Communication.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Standard;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using ChatterBox.Client.Common.Signaling;

namespace ChatterBox.Client.Win8dot1.Channels
{
    internal class ClientChannel : IClientChannel
    {
        private SignalingClient _signalingClient;

        public ClientChannel(SignalingClient signalingClient)
        {
            _signalingClient = signalingClient;
        }

        public void ClientConfirmation(Confirmation confirmation)
        {
            _signalingClient.ClientConfirmation(confirmation);
        }

        public void ClientHeartBeat()
        {
            _signalingClient.ClientHeartBeat();
        }

        public void GetPeerList(Message message)
        {
            _signalingClient.GetPeerList(message);
        }

        public void Register(Registration message)
        {
            _signalingClient.Register(message);
        }

        public void Relay(RelayMessage message)
        {
            _signalingClient.Relay(message);
        }
    }
}
