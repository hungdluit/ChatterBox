using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Universal.Background;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using webrtc_winrt_api;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_ActiveCall : BaseVoipState
    {
        public VoipState_ActiveCall(string peerId)
        {
            _peerId = peerId;
        }

        private string _peerId;

        internal override void SendLocalIceCandidate(RTCIceCandidate candidate)
        {
            if (candidate != null)
            {
                Context.SendToPeer(_peerId, RelayMessageTags.IceCandidate, candidate.Candidate);
            }
        }

        public override async void OnIceCandidate(RelayMessage message)
        {
            await Context.PeerConnection.AddIceCandidate(new RTCIceCandidate(message.Payload, "", 0 /*TODO*/));
        }

        public override void Hangup()
        {
            Context.SwitchState(new VoipState_HangingUp(_peerId));
        }

        public override void OnRemoteHangup(RelayMessage message)
        {
            Context.SwitchState(new VoipState_HangingUp(_peerId));
        }
    }

}
