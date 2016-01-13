using System.Collections.Generic;
using System.Threading.Tasks;
using webrtc_winrt_api;

namespace ChatterBox.Client.Presentation.Shared.Services
{
    public interface IWebRTCSettingsService
    {
        IEnumerable<MediaDevice> VideoCaptureDevices { get; }

        IEnumerable<MediaDevice> AudioCaptureDevices { get; }

        IEnumerable<MediaDevice> AudioPlayoutDevices { get; }

        IEnumerable<CodecInfo> AudioCodecs { get; }

        IEnumerable<CodecInfo> VideoCodecs { get; }

        MediaDevice VideoDevice { get; set; }

        MediaDevice AudioDevice { get; set; }

        CodecInfo VideoCodec { get; set; }

        CodecInfo AudioCodec { get; set; }

        MediaDevice AudioPlayoutDevice { get; set; }

        void SetPreferredVideoCaptureFormat(int Width, int Height, int FrameRate);

        Task InitializeWebRTC();
        void StartTrace();
        void StopTrace();
        void SaveTrace(string ip, int port);

        void ReleaseDevices();
    }
}
