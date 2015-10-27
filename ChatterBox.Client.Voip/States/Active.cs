using System;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using webrtc_winrt_api;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_ActiveCall : BaseVoipState
    {
        private readonly string _peerId;

        public VoipState_ActiveCall(string peerId)
        {
            _peerId = peerId;
        }

        public override void Hangup()
        {
            Context.SwitchState(new VoipState_HangingUp(_peerId));
        }

        public override async void OnIceCandidate(RelayMessage message)
        {
            await Context.PeerConnection.AddIceCandidate(new RTCIceCandidate(message.Payload, "", 0 /*TODO*/));
        }

        public override void OnRemoteHangup(RelayMessage message)
        {
            Context.SwitchState(new VoipState_HangingUp(_peerId));
        }

        internal override void SendLocalIceCandidate(RTCIceCandidate candidate)
        {
            if (candidate != null)
            {
                Context.SendToPeer(_peerId, RelayMessageTags.IceCandidate, candidate.Candidate);
            }
        }
    }
}