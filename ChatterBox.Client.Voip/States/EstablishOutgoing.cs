using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Serialization;
using webrtc_winrt_api;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_EstablishOutgoing : BaseVoipState
    {
        private OutgoingCallRequest _request;

        public VoipState_EstablishOutgoing(OutgoingCallRequest request)
        {
            _request = request;
        }

        public override void Hangup()
        {
            Context.SwitchState(new VoipState_HangingUp());
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
            var stream = await media.GetUserMedia(new RTCMediaStreamConstraints
            {
                videoEnabled = false,
                audioEnabled = true
            });
            Context.PeerConnection.AddStream(stream);
            var sdpOffer = await Context.PeerConnection.CreateOffer();
            await Context.PeerConnection.SetLocalDescription(sdpOffer);

            Context.SendToPeer(RelayMessageTags.SdpOffer, sdpOffer.Sdp);
        }

        public override async void OnIceCandidate(RelayMessage message)
        {
            //var candidate = new RTCIceCandidate { Candidate = message.Payload };
            var candidate =
                DtoIceCandidate.FromDto(
                    (DtoIceCandidate) JsonConvert.Deserialize(message.Payload, typeof (DtoIceCandidate)));
            await Context.PeerConnection.AddIceCandidate(candidate);
        }

        public override async void OnSdpAnswer(RelayMessage message)
        {
            await
                Context.PeerConnection.SetRemoteDescription(new RTCSessionDescription(RTCSdpType.Answer, message.Payload));
            Context.SwitchState(new VoipState_ActiveCall());
        }

        internal override void SendLocalIceCandidate(RTCIceCandidate candidate)
        {
            //Context.SendToPeer(RelayMessageTags.IceCandidate, candidate.Candidate);
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(DtoIceCandidate.ToDto(candidate)));
        }
    }
}