using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Universal.Background;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using webrtc_winrt_api;

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipContext
    {
        private bool _isWebRTCInitialized;

        public VoipContext()
        {
            SwitchState(new VoipState_Idle());
        }

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
                    _peerConnection.OnIceCandidate += evt => { State.SendLocalIceCandidate(evt.Candidate); };
                }
            }
        }

        public BaseVoipState State { get; private set; }

        public void InitializeWebRTC()
        {
            if (!_isWebRTCInitialized)
            {
                WebRTC.Initialize(null);
                _isWebRTCInitialized = true;
            }
        }

        public void SendToPeer(string peerId, string tag, string payload)
        {
            Hub.Instance.SignalingClient.Relay(new RelayMessage
            {
                FromUserId = RegistrationSettings.UserId,
                ToUserId = peerId,
                Tag = tag,
                Payload = payload
            });
        }

        public void SwitchState(BaseVoipState newState)
        {
            lock (this)
            {
                Debug.WriteLine($"VoipContext.SwitchState {State?.GetType().Name} -> {newState.GetType().Name}");
                State?.LeaveState();
                State = newState;
                State.EnterState(this);
            }
        }
    }
}