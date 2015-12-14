using ChatterBox.Client.Voip;
using System;
using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Common.Communication.Messages.Relay;
using System.Collections.Generic;
using Microsoft.Practices.Unity;

namespace ChatterBox.Client.Win8dot1.Voip
{
    internal class Hub : IHub
    {
        private IUnityContainer _container;
        private SignalingClient _signalingClient;
        private IForegroundChannel _foregroundChannel;

        public Hub(IUnityContainer container)
        {
            _container = container;
        }

        public void OnVoipState(VoipState voipState)
        {
            if (_foregroundChannel == null)
            {
                _foregroundChannel = _container.Resolve<IForegroundChannel>();
            }
            _foregroundChannel.OnVoipState(voipState);
        }

        public void Relay(RelayMessage message)
        {
            if (_signalingClient == null)
            {
                _signalingClient = _container.Resolve<SignalingClient>();
            }
            _signalingClient.Relay(message);
        }

		public void InitialiazeStatsManager(webrtc_winrt_api.RTCPeerConnection pc)
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 yet");
        }

        public void ToggleStatsManagerConnectionState(bool enable)
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 yet");
        }

        public void TrackStatsManagerEvent(string name, IDictionary<string, string> props)
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 yet");
        }

        public void TrackStatsManagerMetric(string name, double value)
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 yet");
        }

        public void StartStatsManagerCallWatch()
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 yet");
        }

        public void StopStatsManagerCallWatch()
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 yet");
        }

        public bool IsAppInsightsEnabled
        {
            get
            {
                return false;
            }
            set {}
        }

        public void OnUpdateFrameFormat(FrameFormat frameFormat)
        {
        }
    }
}
