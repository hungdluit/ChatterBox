using ChatterBox.Client.Presentation.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using webrtc_winrt_api;

namespace ChatterBox.Client.Universal.Services
{
    internal class WebRTCSettingsService : IWebRTCSettingsService
    {
        public WebRTCSettingsService()
        {
            WebRTC.Initialize(null);
        }

        public IEnumerable<MediaDevice> AudioCaptureDevices
        {
            get
            {
                return new List<MediaDevice>();
            }
        }

        public CodecInfo AudioCodec
        {
            get;
            set;
        }

        public MediaDevice AudioDevice
        {
            get;
            set;
        }

        public IEnumerable<MediaDevice> VideoCaptureDevices
        {
            get
            {
                return new List<MediaDevice>();
            }
        }

        public CodecInfo VideoCodec
        {
            get;
            set;
        }

        public MediaDevice VideoDevice
        {
            get;
            set;
        }

        public void SetPreferredVideoCaptureFormat(int Width, int Height, int FrameRate)
        {
        }
    }
}
