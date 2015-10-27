using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using webrtc_winrt_api;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_EstablishIncoming : BaseVoipState
    {
        private readonly string _peerId;

        public VoipState_EstablishIncoming(string peerId)
        {
            _peerId = peerId;
        }

        public override void Hangup()
        {
            Context.SwitchState(new VoipState_HangingUp(_peerId));
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
        }

        public override async void OnIceCandidate(RelayMessage message)
        {
            await Context.PeerConnection.AddIceCandidate(new RTCIceCandidate(message.Payload, "", 0 /*TODO*/));
        }

        public override async void OnSdpOffer(RelayMessage message)
        {
            await
                Context.PeerConnection.SetRemoteDescription(new RTCSessionDescription(RTCSdpType.Offer, message.Payload));
            var sdpAnswer = await Context.PeerConnection.CreateAnswer();
            await Context.PeerConnection.SetLocalDescription(sdpAnswer);
            Context.SendToPeer(_peerId, RelayMessageTags.SdpAnswer, sdpAnswer.Sdp);
            Context.SwitchState(new VoipState_ActiveCall(_peerId));
        }

        internal override void SendLocalIceCandidate(RTCIceCandidate candidate)
        {
            // TODO: Pass mid and index.
            Context.SendToPeer(_peerId, RelayMessageTags.IceCandidate, candidate.Candidate);
        }
    }
}