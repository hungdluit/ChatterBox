using ChatterBox.Client.Voip;
using System;
using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Common.Communication.Messages.Relay;

namespace ChatterBox.Client.Win8dot1.Voip
{
    internal class Hub : IHub
    {
        private SignalingClient _signalingClient;
        private IForegroundChannel _foregroundChannel;

        public Hub(SignalingClient signalingClient, IForegroundChannel foregroundChannel)
        {
            _signalingClient = signalingClient;
            _foregroundChannel = foregroundChannel;
        }

        public void OnVoipState(VoipState voipState)
        {
            _foregroundChannel.OnVoipState(voipState);
        }

        public void Relay(RelayMessage message)
        {
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
    }
}
