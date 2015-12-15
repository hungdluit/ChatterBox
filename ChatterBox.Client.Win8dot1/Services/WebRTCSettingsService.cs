using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Presentation.Shared.Services;
using System.Collections.Generic;
using webrtc_winrt_api;

namespace ChatterBox.Client.Win8dot1.Services
{
    internal class WebRTCSettingsService : IWebRTCSettingsService
    {
        private VoipContext _voipContext;

        public WebRTCSettingsService(VoipContext voipContext)
        {
            _voipContext = voipContext;
            InitializeWebRTC();
        }

        private async void InitializeWebRTC()
        {
            await _voipContext.InitializeWebRTC();
        }

        private MediaDevice _audioDevice;
        public MediaDevice AudioDevice
        {
            get
            {
                return _audioDevice;
            }
            set
            {
                _audioDevice = value;
                _voipContext.Media.SelectAudioDevice(value);
            }
        }

        public IEnumerable<MediaDevice> AudioCaptureDevices
        {
            get
            {
                return _voipContext.Media.GetAudioCaptureDevices();
            }
        }

        private MediaDevice _videoDevice;
        public MediaDevice VideoDevice
        {
            get
            {
                return _videoDevice;
            }
            set
            {
                _videoDevice = value;
                _voipContext.Media.SelectVideoDevice(value);
            }
        }

        private CodecInfo _videoCodec;
        public CodecInfo VideoCodec
        {
            get
            {
                return _videoCodec;
            }
            set
            {
                _videoCodec = value;
                _voipContext.VideoCodec = value;
            }
        }

        private CodecInfo _audioCodec;
        public CodecInfo AudioCodec
        {
            get
            {
                return _audioCodec;
            }
            set
            {
                _audioCodec = value;
                _voipContext.AudioCodec = value;
            }
        }

        public IEnumerable<MediaDevice> VideoCaptureDevices
        {
            get
            {
                return _voipContext.Media.GetVideoCaptureDevices();
            }
        }

        public void SetPreferredVideoCaptureFormat(int Width, int Height, int FrameRate)
        {
            WebRTC.SetPreferredVideoCaptureFormat(Width, Height, FrameRate);
        }
    }
}
