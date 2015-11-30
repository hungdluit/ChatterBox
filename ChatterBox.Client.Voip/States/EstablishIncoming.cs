using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Signaling.Dto;
using ChatterBox.Client.Voip;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Serialization;
using webrtc_winrt_api;
using ChatterBox.Client.Voip.States.Interfaces;
using Microsoft.Practices.Unity;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_EstablishIncoming : BaseVoipState
    {
        public override void Hangup()
        {
            var hangingUpState = Context.UnityContainer.Resolve<IHangingUp>();
            Context.SwitchState((BaseVoipState)hangingUpState);
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

            var media = await Media.CreateMediaAsync();
            await media.EnumerateAudioVideoCaptureDevices();
            var stream = await media.GetUserMedia(new RTCMediaStreamConstraints
            {
                videoEnabled = true,
                audioEnabled = true
            });
            Context.PeerConnection.AddStream(stream);

            var tracks = stream.GetVideoTracks();
            if (tracks.Count > 0)
            {
                var source = media.CreateMediaStreamSource(tracks[0], 30, "LOCAL");
                Context.LocalVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source);
            }
        }

        internal override async void OnAddStream(MediaStream stream)
        {
            var tracks = stream.GetVideoTracks();
            if (tracks.Count > 0)
            {
                var media = await Media.CreateMediaAsync();
                var source = media.CreateMediaStreamSource(tracks[0], 30, "PEER");
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

        public override async void OnSdpOffer(RelayMessage message)
        {
            await
                Context.PeerConnection.SetRemoteDescription(new RTCSessionDescription(RTCSdpType.Offer, message.Payload));
            var sdpAnswer = await Context.PeerConnection.CreateAnswer();
            await Context.PeerConnection.SetLocalDescription(sdpAnswer);
            Context.SendToPeer(RelayMessageTags.SdpAnswer, sdpAnswer.Sdp);
            Context.SwitchState(new VoipState_ActiveCall());
        }

        public override void SendLocalIceCandidate(RTCIceCandidate candidate)
        {
            //Context.SendToPeer(RelayMessageTags.IceCandidate, candidate.Candidate);
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(DtoExtensions.ToDto(candidate)));
        }
    }
}