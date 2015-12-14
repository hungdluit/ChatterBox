using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChatterBox.Client.Common.Signaling.Dto;
using ChatterBox.Client.Voip;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Serialization;
using webrtc_winrt_api;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using System.Linq;
using Windows.UI.Core;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Voip.Utils;
using ChatterBox.Client.Common.Settings;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_EstablishIncoming : BaseVoipState
    {
        private RelayMessage _message;
        private OutgoingCallRequest _callRequest;

        public VoipState_EstablishIncoming(RelayMessage message)
        {
            _message = message;
            _callRequest = (OutgoingCallRequest)JsonConvert.Deserialize(message.Payload, typeof(OutgoingCallRequest));
        }

        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.EstablishIncoming;
            }
        }

        public override async Task Hangup()
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);

            var config = new RTCConfiguration
            {
                IceServers = WebRtcSettingsUtils.ToRTCIceServer(IceServerSettings.IceServers)
            };
            Context.PeerConnection = new RTCPeerConnection(config);

            Context.LocalStream = await Context.Media.GetUserMedia(new RTCMediaStreamConstraints
            {
                videoEnabled = _callRequest.VideoEnabled,
                audioEnabled = true
            });
            Context.PeerConnection.AddStream(Context.LocalStream);

            var tracks = Context.LocalStream.GetVideoTracks();
            if (tracks.Count > 0)
            {
                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, "LOCAL");
                Context.LocalVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source);
            }
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

        public override async Task OnIceCandidate(RelayMessage message)
        {
            var candidate =
                DtoExtensions.FromDto(
                    (DtoIceCandidate)JsonConvert.Deserialize(message.Payload, typeof(DtoIceCandidate)));
            await Context.PeerConnection.AddIceCandidate(candidate);
        }

        public override async Task OnSdpOffer(RelayMessage message)
        {
            await
                Context.PeerConnection.SetRemoteDescription(new RTCSessionDescription(RTCSdpType.Offer, message.Payload));
            var sdpAnswer = await Context.PeerConnection.CreateAnswer();
            await Context.PeerConnection.SetLocalDescription(sdpAnswer);
            Context.SendToPeer(RelayMessageTags.SdpAnswer, sdpAnswer.Sdp);
            await Context.SwitchState(new VoipState_ActiveCall(_callRequest));
        }

        public override async Task SendLocalIceCandidate(RTCIceCandidate candidate)
        {
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(DtoExtensions.ToDto(candidate)));
        }
    }
}
