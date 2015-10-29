using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Universal.Background;
using ChatterBox.Client.Voip.Dto;
using ChatterBox.Common.Communication.Serialization;
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
        public VoipState_ActiveCall()
        {
        }

        internal override void SendLocalIceCandidate(RTCIceCandidate candidate)
        {
            //Context.SendToPeer(RelayMessageTags.IceCandidate, candidate.Candidate);
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(DtoIceCandidate.ToDto(candidate)));
        }

        public override async void OnIceCandidate(RelayMessage message)
        {
            //var candidate = new RTCIceCandidate { Candidate = message.Payload };
            var candidate = DtoIceCandidate.FromDto((DtoIceCandidate)JsonConvert.Deserialize(message.Payload, typeof(DtoIceCandidate)));
            await Context.PeerConnection.AddIceCandidate(candidate);
        }

        public override void Hangup()
        {
            Context.SwitchState(new VoipState_HangingUp());
        }

        public override void OnRemoteHangup(RelayMessage message)
        {
            Context.SwitchState(new VoipState_HangingUp());
        }
    }

}
