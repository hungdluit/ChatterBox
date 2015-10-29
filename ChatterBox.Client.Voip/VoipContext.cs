using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Universal.Background;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using webrtc_winrt_api;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipContext
    {
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

        internal VoipState GetVoipState()
        {
            var stateVal = (VoipStateEnum)Enum.Parse(typeof(VoipStateEnum), State.GetType().Name.Split('_')[1]);

            return new Foreground.Dto.VoipState
            {
                PeerId = PeerId,
                HasPeerConnection = PeerConnection != null,
                State = stateVal,
            };
        }

        private string _peerId;
        public string PeerId
        {
            get { return _peerId; }
            set { _peerId = value; }
        }

        private bool _isWebRTCInitialized = false;
        public void InitializeWebRTC()
        {
            if (!_isWebRTCInitialized)
            {
                webrtc_winrt_api.WebRTC.Initialize(null);
                _isWebRTCInitialized = true;
            }
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

        public BaseVoipState State { get; private set; }
        public VoipPhoneCall VoipCall { get; internal set; }

        public void SendToPeer(string tag, string payload)
        {
            Hub.Instance.SignalingClient.Relay(new RelayMessage
            {
                FromUserId = RegistrationSettings.UserId,
                ToUserId = PeerId,
                Tag = tag,
                Payload = payload,
            });
        }
    }
}
