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

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_EstablishOutgoing : BaseVoipState
    {
        private OutgoingCallRequest _request;

        public VoipState_EstablishOutgoing(OutgoingCallRequest request)
        {
            _request = request;
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

            _media = await Media.CreateMediaAsync();
            await _media.EnumerateAudioVideoCaptureDevices();
            Context.LocalStream = await _media.GetUserMedia(new RTCMediaStreamConstraints
            {
                videoEnabled = _request.Video,
                audioEnabled = true
            });
            Context.PeerConnection.AddStream(Context.LocalStream);
            var sdpOffer = await Context.PeerConnection.CreateOffer();
            await Context.PeerConnection.SetLocalDescription(sdpOffer);

            var tracks = Context.LocalStream.GetVideoTracks();
            if (tracks.Count > 0)
            {
                var source = _media.CreateMediaStreamSource(tracks[0], 30, "LOCAL");
                Context.LocalVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source);
            }

            Context.SendToPeer(RelayMessageTags.SdpOffer, sdpOffer.Sdp);
        }

        Media _media;

        internal override void OnAddStream(MediaStream stream)
        {
            Context.RemoteStream = stream;
            var tracks = stream.GetVideoTracks();
            if (tracks.Count > 0)
            {
                var source = _media.CreateMediaStreamSource(tracks[0], 30, "PEER");
                Context.RemoteVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source);
            }
        }

        public override async void OnIceCandidate(RelayMessage message)
        {
            //var candidate = new RTCIceCandidate { Candidate = message.Payload };
            var candidate =
                DtoExtensions.FromDto(
                    (DtoIceCandidate) JsonConvert.Deserialize(message.Payload, typeof (DtoIceCandidate)));
            await Context.PeerConnection.AddIceCandidate(candidate);
        }

        public override async void OnSdpAnswer(RelayMessage message)
        {
            await
                Context.PeerConnection.SetRemoteDescription(new RTCSessionDescription(RTCSdpType.Answer, message.Payload));
            Context.SwitchState(new VoipState_ActiveCall());
        }

        public override void SendLocalIceCandidate(RTCIceCandidate candidate)
        {
            //Context.SendToPeer(RelayMessageTags.IceCandidate, candidate.Candidate);
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(DtoExtensions.ToDto(candidate)));
        }

        public override void Hangup()
        {
            var hangingUpState = Context.UnityContainer.Resolve<IHangingUp>();
            Context.SwitchState((BaseVoipState)hangingUpState);
        }

        public override void OnRemoteHangup(RelayMessage message)
        {
            var hangingUpState = Context.UnityContainer.Resolve<IHangingUp>();
            Context.SwitchState((BaseVoipState)hangingUpState);
        }
    }
}