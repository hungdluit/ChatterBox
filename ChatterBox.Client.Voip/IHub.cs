using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using System.Collections.Generic;

namespace ChatterBox.Client.Voip
{
    internal interface IHub
    {
        void Relay(RelayMessage message);

        void OnVoipState(VoipState voipState);

        void InitialiazeStatsManager(webrtc_winrt_api.RTCPeerConnection pc);

        void ToggleStatsManagerConnectionState(bool enable);

        void OnUpdateFrameFormat(FrameFormat frameFormat);

        void TrackStatsManagerEvent(string name, IDictionary<string, string> props);

        void TrackStatsManagerMetric(string name, double value);

        void StartStatsManagerCallWatch();

        void StopStatsManagerCallWatch();

        bool IsAppInsightsEnabled
        {
            get;
            set;
        }
    }
}
