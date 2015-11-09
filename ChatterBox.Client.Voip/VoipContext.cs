using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
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
                    _peerConnection.OnIceCandidate += evt =>
                    {
                        if (evt.Candidate != null)
                        {
                            State.SendLocalIceCandidate(evt.Candidate);
                        }
                    };
                }
            }
        }

        public string PeerId { get; set; }
        private BaseVoipState State { get; set; }
        public VoipPhoneCall VoipCall { get; internal set; }

        internal VoipState GetVoipState()
        {
            var stateVal = (VoipStateEnum) Enum.Parse(typeof (VoipStateEnum), State.GetType().Name.Split('_')[1]);

            return new VoipState
            {
                PeerId = PeerId,
                HasPeerConnection = PeerConnection != null,
                State = stateVal
            };
        }

        public void InitializeWebRTC()
        {
            if (!_isWebRTCInitialized)
            {
                WebRTC.Initialize(null);
                _isWebRTCInitialized = true;
            }
        }

        public void SendToPeer(string tag, string payload)
        {
            Hub.Instance.SignalingClient.Relay(new RelayMessage
            {
                FromUserId = RegistrationSettings.UserId,
                ToUserId = PeerId,
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

                Task.Run(() => Hub.Instance.ForegroundClient.OnVoipState(GetVoipState()));
            }
        }

        public void WithState(Action<BaseVoipState> fn)
        {
            lock (this)
            {
                fn(State);
            }
        }
    }
}