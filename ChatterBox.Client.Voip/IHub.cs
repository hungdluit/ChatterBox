using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatterBox.Client.Voip
{
    internal interface IHub
    {
        void Relay(RelayMessage message);

        void OnVoipState(VoipState voipState);

        void InitialiazeStatsManager(webrtc_winrt_api.RTCPeerConnection pc);

        void ToggleStatsManagerConnectionState(bool enable);

        void TrackStatsManagerEvent(string name, IDictionary<string, string> props);

        void TrackStatsManagerMetric(string name, double value);

        void StartStatsManagerCallWatch();

        void StopStatsManagerCallWatch();
    }
}
