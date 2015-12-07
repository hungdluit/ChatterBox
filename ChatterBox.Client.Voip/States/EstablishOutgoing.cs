using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Signaling.Dto;
using ChatterBox.Client.Voip;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Serialization;
using webrtc_winrt_api;
using Microsoft.Practices.Unity;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using System.Linq;
using Windows.UI.Core;
using ChatterBox.Client.Voip.Utils;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_EstablishOutgoing : BaseVoipState
    {
        private OutgoingCallRequest _callRequest;

        public VoipState_EstablishOutgoing(OutgoingCallRequest request)
        {
            _callRequest = request;
        }

        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.EstablishOutgoing;
            }
        }

        public override void Hangup()
        {
            var hangingUpState = new VoipState_HangingUp();
            Context.SwitchState(hangingUpState);
        }

        public override void OnRemoteHangup(RelayMessage message)
        {
            var hangingUpState = new VoipState_HangingUp();
            Context.SwitchState(hangingUpState);
        }

        public override async void OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);

            var config = new RTCConfiguration
            {
                IceServers = new List<RTCIceServer>
                {
                    new RTCIceServer {Url = "stun:stun.l.google.com:19302"},
                    new RTCIceServer {Url = "stun:stun1.l.google.com:19302"},
                    new RTCIceServer {Url = "stun:stun2.l.google.com:19302"},
                    new RTCIceServer {Url = "stun:stun3.l.google.com:19302"},
                    new RTCIceServer {Url = "stun:stun4.l.google.com:19302"}
                }
            };
            Context.PeerConnection = new RTCPeerConnection(config);

            Context.LocalStream = await Context.Media.GetUserMedia(new RTCMediaStreamConstraints
            {
                videoEnabled = _callRequest.VideoEnabled,
                audioEnabled = true
            });
            Context.PeerConnection.AddStream(Context.LocalStream);
            var sdpOffer = await Context.PeerConnection.CreateOffer();
            var sdpString = sdpOffer.Sdp;
            SdpUtils.SelectCodecs(ref sdpString, Context.AudioCodec, Context.VideoCodec);
            sdpOffer.Sdp = sdpString;
            await Context.PeerConnection.SetLocalDescription(sdpOffer);

            var tracks = Context.LocalStream.GetVideoTracks();
            if (tracks.Count > 0)
            {
                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, "LOCAL");
                Context.LocalVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source);
            }

            Context.SendToPeer(RelayMessageTags.SdpOffer, sdpOffer.Sdp);
        }

        internal override void OnAddStream(MediaStream stream)
        {
            Context.RemoteStream = stream;
            var tracks = stream.GetVideoTracks();
            if (tracks.Count > 0)
            {
                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, "PEER");
                Context.RemoteVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source);
            }
        }

        public override async void OnIceCandidate(RelayMessage message)
        {
            //var candidate = new RTCIceCandidate { Candidate = message.Payload };
            var candidate =
                DtoExtensions.FromDto(
                    (DtoIceCandidate)JsonConvert.Deserialize(message.Payload, typeof(DtoIceCandidate)));
            await Context.PeerConnection.AddIceCandidate(candidate);
        }

        public override async void OnSdpAnswer(RelayMessage message)
        {
            await
                Context.PeerConnection.SetRemoteDescription(new RTCSessionDescription(RTCSdpType.Answer, message.Payload));
            Context.SwitchState(new VoipState_ActiveCall(_callRequest));
        }

        public override void SendLocalIceCandidate(RTCIceCandidate candidate)
        {
            //Context.SendToPeer(RelayMessageTags.IceCandidate, candidate.Candidate);
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(DtoExtensions.ToDto(candidate)));
        }
    }
}
