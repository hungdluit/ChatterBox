using ChatterBox.Client.Common.Communication.Voip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;

namespace ChatterBox.Client.Win8dot1.Channels
{
    internal class VoipChannelTemp : IVoipChannel
    {
        public void Answer()
        {
            throw new NotImplementedException();
        }

        public void Call(OutgoingCallRequest request)
        {
            throw new NotImplementedException();
        }

        public VoipState GetVoipState()
        {
            return new VoipState
            {
                State = VoipStateEnum.Idle,
                HasPeerConnection = false
            };
        }

        public void Hangup()
        {
            throw new NotImplementedException();
        }

        public void OnIceCandidate(RelayMessage message)
        {
            throw new NotImplementedException();
        }

        public void OnIncomingCall(RelayMessage message)
        {
            throw new NotImplementedException();
        }

        public void OnOutgoingCallAccepted(RelayMessage message)
        {
            throw new NotImplementedException();
        }

        public void OnOutgoingCallRejected(RelayMessage message)
        {
            throw new NotImplementedException();
        }

        public void OnRemoteHangup(RelayMessage message)
        {
            throw new NotImplementedException();
        }

        public void OnSdpAnswer(RelayMessage message)
        {
            throw new NotImplementedException();
        }

        public void OnSdpOffer(RelayMessage message)
        {
            throw new NotImplementedException();
        }

        public void Reject(IncomingCallReject reason)
        {
            throw new NotImplementedException();
        }
    }
}
