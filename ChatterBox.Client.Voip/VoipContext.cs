using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Universal.Background;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using System;
using System.Collections.Generic;
using System.Text;
using webrtc_winrt_api;

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipContext
    {
        private RTCPeerConnection _peerConnection { get; set; }
        public RTCPeerConnection PeerConnection
        {
            get { return _peerConnection; }
            set
            {
                _peerConnection = value;
                if (_peerConnection != null)
                {
                    // Register to the events from the peer connection.
                    // We'll forward them to the state.
                    _peerConnection.OnIceCandidate += evt =>
                    {
                        State.SendLocalIceCandidate(evt.Candidate);
                    };
                }
            }
        }

        public void SwitchState(BaseVoipState newState)
        {
            lock (this)
            {
                State.LeaveState();
                State = newState;
                State.EnterState(this);
            }
        }

        public BaseVoipState State { get; private set; }

        public void SendToPeer(string peerId, string tag, string payload)
        {
            Hub.Instance.SignalingClient.Relay(new RelayMessage
            {
                FromUserId = RegistrationSettings.UserId,
                ToUserId = peerId,
                Tag = tag,
                Payload = payload,
            });
        }
    }
}
