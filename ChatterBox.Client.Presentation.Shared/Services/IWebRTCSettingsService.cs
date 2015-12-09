using System.Collections.Generic;
using webrtc_winrt_api;

namespace ChatterBox.Client.Presentation.Shared.Services
{
    public interface IWebRTCSettingsService
    {
        IEnumerable<MediaDevice> VideoCaptureDevices { get; }

        IEnumerable<MediaDevice> AudioCaptureDevices { get; }

        MediaDevice VideoDevice { get; set; }

        MediaDevice AudioDevice { get; set; }

        CodecInfo VideoCodec { get; set; }

        CodecInfo AudioCodec { get; set; }

        void SetPreferredVideoCaptureFormat(int Width, int Height, int FrameRate);
    }
}
