using System;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Signaling.Dto;
using ChatterBox.Client.Voip;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Serialization;
using webrtc_winrt_api;
using ChatterBox.Client.Voip.States.Interfaces;
using Microsoft.Practices.Unity;
using ChatterBox.Client.Common.Communication.Foreground.Dto;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_ActiveCall : BaseVoipState
    {
        private OutgoingCallRequest _callRequest;

        public VoipState_ActiveCall(OutgoingCallRequest callRequest)
        {
            _callRequest = callRequest;
        }

        internal override async void OnAddStream(MediaStream stream)
        {
            Context.RemoteStream = stream;
            var tracks = stream.GetVideoTracks();
            if (tracks.Count > 0)
            {
                var media = await Media.CreateMediaAsync();
                var source = media.CreateMediaStreamSource(tracks[0], 30, "PEER");
                Context.RemoteVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source);
		    }
		}
        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.ActiveCall;
            }
        }

        public override void Hangup()
        {
            var hangingUpState = new VoipState_HangingUp();
            Context.SwitchState(hangingUpState);
        }

        public override async void OnIceCandidate(RelayMessage message)
        {
            //var candidate = new RTCIceCandidate { Candidate = message.Payload };
            var candidate =
                DtoExtensions.FromDto(
                    (DtoIceCandidate) JsonConvert.Deserialize(message.Payload, typeof (DtoIceCandidate)));
            await Context.PeerConnection.AddIceCandidate(candidate);
        }

        public override void OnRemoteHangup(RelayMessage message)
        {
            var hangingUpState = new VoipState_HangingUp();
            Context.SwitchState(hangingUpState);
        }

        public override void SendLocalIceCandidate(RTCIceCandidate candidate)
        {
            //Context.SendToPeer(RelayMessageTags.IceCandidate, candidate.Candidate);
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(DtoExtensions.ToDto(candidate)));
        }
    }
}