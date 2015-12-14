using System;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Signaling.Dto;
using ChatterBox.Client.Voip;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Serialization;
using webrtc_winrt_api;
using ChatterBox.Client.Voip.States.Interfaces;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;
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

        internal override async Task OnAddStream(MediaStream stream)
        {
            Context.RemoteStream = stream;
            var tracks = stream.GetVideoTracks();
            if (tracks.Count > 0)
            {
                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, "PEER");
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

        public override async Task Hangup()
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task OnIceCandidate(RelayMessage message)
        {
            var candidate =
                DtoExtensions.FromDto(
                    (DtoIceCandidate) JsonConvert.Deserialize(message.Payload, typeof (DtoIceCandidate)));
            await Context.PeerConnection.AddIceCandidate(candidate);
        }

        public override async Task OnRemoteHangup(RelayMessage message)
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task SendLocalIceCandidate(RTCIceCandidate candidate)
        {
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(DtoExtensions.ToDto(candidate)));
        }
    }
}