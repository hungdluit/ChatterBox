using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Presentation.Shared.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using webrtc_winrt_api;

namespace ChatterBox.Client.Win8dot1.Services
{
    internal class WebRTCSettingsService : IWebRTCSettingsService
    {
        private VoipContext _voipContext;

        public WebRTCSettingsService(VoipContext voipContext)
        {
            _voipContext = voipContext;
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

        public IEnumerable<MediaDevice> AudioPlayoutDevices
        {
            get
            {
                return _voipContext.Media.GetAudioPlayoutDevices();
            }
        }

        public IEnumerable<CodecInfo> AudioCodecs
        {
            get
            {
                return WebRTC.GetAudioCodecs();
            }
        }

        public IEnumerable<CodecInfo> VideoCodecs
        {
            get
            {
                return WebRTC.GetVideoCodecs();
            }
        }

        private MediaDevice _audioPlayoutDevice;
        public MediaDevice AudioPlayoutDevice
        {
            get
            {
                return _audioPlayoutDevice;
            }

            set
            {
                _voipContext.Media.SelectAudioPlayoutDevice(value);
                _audioPlayoutDevice = value;
            }
        }

        public void SetPreferredVideoCaptureFormat(int Width, int Height, int FrameRate)
        {
            WebRTC.SetPreferredVideoCaptureFormat(Width, Height, FrameRate);
        }

        Task IWebRTCSettingsService.InitializeWebRTC()
        {
            return _voipContext.InitializeWebRTC();
        }
        
        public void StartTrace()
        {
            _voipContext.StartTrace();
        }

        public void StopTrace()
        {
            _voipContext.StopTrace();
        }
        public void SaveTrace(string ip, int port)
        {
            _voipContext.SaveTrace(ip, port);

        }
    }
}
