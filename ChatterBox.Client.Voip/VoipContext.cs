using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Client.Voip;
using ChatterBox.Client.Voip.Utils;
using ChatterBox.Client.Voip.States.Interfaces;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using webrtc_winrt_api;
using System.Threading;
using Windows.UI.Core;
using System.Collections.Generic;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System.Threading;

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipContext
    {
        private CoreDispatcher _dispatcher;
        private Func<IVideoRenderHelper> _renderResolver;
        private IHub _hub;
        private ApplicationDataContainer _localSettings;

        public VoipContext(IHub hub,
                           CoreDispatcher dispatcher,
                           Func<IVideoRenderHelper> renderResolver,
                           IVoipCoordinator coordinator)
        {
            _hub = hub;
            _dispatcher = dispatcher;
            _renderResolver = renderResolver;
            _localSettings = ApplicationData.Current.LocalSettings;
            VoipCoordinator = coordinator;

            var idleState = new VoipState_Idle();
            SwitchState(idleState).Wait();

            ResetRenderers();
        }

        /// <summary>
        /// On Win10 in a background task, WebRTC initialization has to be done
        /// when we have access to the resources.  That's inside an active
        /// voip call.
        /// This function must be called after VoipCoordinator.StartVoipTask()
        /// </summary>
        /// <returns></returns>
        public async Task InitializeWebRTC()
        {
            if (Media == null)
            {
                WebRTC.Initialize(_dispatcher);
                Media = Media.CreateMedia();
                Media.SetDisplayOrientation(_displayOrientation);
                await Media.EnumerateAudioVideoCaptureDevices();
            }

            string videoDeviceId = string.Empty;
            if (_localSettings.Values.ContainsKey(WebRTCSettingsIds.VideoDeviceSettings))
            {
                videoDeviceId = (string)_localSettings.Values[WebRTCSettingsIds.VideoDeviceSettings];
            }
            var videoDevices = Media.GetVideoCaptureDevices();
            var selectedVideoDevice = videoDevices.FirstOrDefault(d => d.Id.Equals(videoDeviceId));
            selectedVideoDevice = selectedVideoDevice ?? videoDevices.FirstOrDefault();
            if (selectedVideoDevice != null)
            {
                Media.SelectVideoDevice(selectedVideoDevice);
            }

            string audioDeviceId = string.Empty;
            if (_localSettings.Values.ContainsKey(WebRTCSettingsIds.AudioDeviceSettings))
            {
                audioDeviceId = (string)_localSettings.Values[WebRTCSettingsIds.AudioDeviceSettings];
            }
            var audioDevices = Media.GetAudioCaptureDevices();
            var selectedAudioDevice = audioDevices.FirstOrDefault(d => d.Id.Equals(audioDeviceId));
            selectedAudioDevice = selectedAudioDevice ?? audioDevices.FirstOrDefault();
            if (selectedAudioDevice != null)
            {
                Media.SelectAudioDevice(selectedAudioDevice);
            }

            string audioPlayoutDeviceId = string.Empty;
            if (_localSettings.Values.ContainsKey(WebRTCSettingsIds.AudioPlayoutDeviceSettings))
            {
                audioPlayoutDeviceId = (string)_localSettings.Values[WebRTCSettingsIds.AudioPlayoutDeviceSettings];
            }
            var audioPlayoutDevices = Media.GetAudioPlayoutDevices();
            var selectedAudioPlayoutDevice = audioPlayoutDevices.FirstOrDefault(d => d.Id.Equals(audioPlayoutDeviceId));
            selectedAudioPlayoutDevice = selectedAudioPlayoutDevice ?? audioPlayoutDevices.FirstOrDefault();
            if (selectedAudioPlayoutDevice != null)
            {
                Media.SelectAudioPlayoutDevice(selectedAudioPlayoutDevice);
            }

            int videoCodecId = int.MinValue;
            if (_localSettings.Values.ContainsKey(WebRTCSettingsIds.VideoCodecSettings))
            {
                videoCodecId = (int)_localSettings.Values[WebRTCSettingsIds.VideoCodecSettings];
            }
            var videoCodecs = WebRTC.GetVideoCodecs();
            var selectedVideoCodec = videoCodecs.FirstOrDefault(c => c.Id.Equals(videoCodecId));
            VideoCodec = selectedVideoCodec ?? videoCodecs.FirstOrDefault();

            int audioCodecId = int.MinValue;
            if (_localSettings.Values.ContainsKey(WebRTCSettingsIds.AudioCodecSettings))
            {
                audioCodecId = (int)_localSettings.Values[WebRTCSettingsIds.AudioCodecSettings];
            }
            var audioCodecs = WebRTC.GetAudioCodecs();
            var selectedAudioCodec = audioCodecs.FirstOrDefault(c => c.Id.Equals(audioCodecId));
            AudioCodec = selectedAudioCodec ?? audioCodecs.FirstOrDefault();

            if (_localSettings.Values.ContainsKey(WebRTCSettingsIds.PreferredVideoCaptureWidth) &&
                _localSettings.Values.ContainsKey(WebRTCSettingsIds.PreferredVideoCaptureHeight) &&
                _localSettings.Values.ContainsKey(WebRTCSettingsIds.PreferredVideoCaptureFrameRate))
            {
                WebRTC.SetPreferredVideoCaptureFormat((int)_localSettings.Values[WebRTCSettingsIds.PreferredVideoCaptureWidth],
                                                      (int)_localSettings.Values[WebRTCSettingsIds.PreferredVideoCaptureHeight],
                                                      (int)_localSettings.Values[WebRTCSettingsIds.PreferredVideoCaptureFrameRate]);
            }
        }

        public void StartTrace()
        {
            WebRTC.StartTracing();
            AppPerformanceCheck();
        }

        public void StopTrace()
        {
            WebRTC.StopTracing();
            if (_appPerfTimer != null)
            {
              _appPerfTimer.Cancel();
            }
        }
        public void SaveTrace(string ip, int port)
        {
            WebRTC.SaveTrace(ip, port);
        }

        private ThreadPoolTimer _appPerfTimer = null;

        private void AppPerformanceCheck()
        {

            if (_appPerfTimer != null)
            {
              _appPerfTimer.Cancel();
            }

            _appPerfTimer = ThreadPoolTimer.CreatePeriodicTimer(t=> ReportAppPerf(), TimeSpan.FromSeconds(1));

        }


        private void ReportAppPerf()
        {
            WebRTC.UpdateCPUUsage(CPUData.GetCPUUsage());
            WebRTC.UpdateMemUsage(MEMData.GetMEMUsage());

        }


        private void LocalVideoRenderer_RenderFormatUpdate(long swapChainHandle, uint width, uint height)
        {
            _hub.OnUpdateFrameFormat(
                new FrameFormat()
                {
                    IsLocal = true,
                    SwapChainHandle = swapChainHandle,
                    Width = width,
                    Height = height
                });
        }

        private void RemoteVideoRenderer_RenderFormatUpdate(long swapChainHandle, uint width, uint height)
        {
            _hub.OnUpdateFrameFormat(
                new FrameFormat()
                {
                    IsLocal = false,
                    SwapChainHandle = swapChainHandle,
                    Width = width,
                    Height = height
                });
        }

        public CodecInfo AudioCodec { get; set; }

        public CodecInfo VideoCodec { get; set; }

        public Media Media { get; private set; }

        private bool _isVideoEnabled;
        public bool IsVideoEnabled
        {
            get
            {
                lock (this)
                {
                    return _isVideoEnabled;
                }
            }
            set
            {
                lock (this)
                {
                    _isVideoEnabled = value;
                    ApplyVideoConfig();
                }
            }
        }

        public IVoipCoordinator VoipCoordinator { get; set; }

        private MediaStream _localStream;
        public MediaStream LocalStream
        {
            get
            {
                return _localStream;
            }
            set
            {
                _localStream = value;
                ApplyMicrophoneConfig();
                ApplyVideoConfig();
            }
        }
        public MediaStream RemoteStream { get; set; }

        private Timer _iceCandidateBufferTimer;
        private List<RTCIceCandidate> _bufferedIceCandidates = new List<RTCIceCandidate>();
        private SemaphoreSlim _iceBufferSemaphore = new SemaphoreSlim(1, 1);
        private async Task QueueIceCandidate(RTCIceCandidate candidate)
        {
            await _iceBufferSemaphore.WaitAsync();
            _bufferedIceCandidates.Add(candidate);
            if (_iceCandidateBufferTimer == null)
            {
                // Flush the ice candidates in 100ms.
                _iceCandidateBufferTimer = new Timer(FlushBufferedIceCandidates, null, 100, Timeout.Infinite);
            }
            _iceBufferSemaphore.Release();
        }

        private async void FlushBufferedIceCandidates(object state)
        {
            await _iceBufferSemaphore.WaitAsync();
            _iceCandidateBufferTimer = null;

            // Chunk in groups of 10 to not blow the size limit
            // on the storage used by the receiving side.
            while (_bufferedIceCandidates.Count > 0)
            {
                var candidates = _bufferedIceCandidates.Take(10).ToArray();
                _bufferedIceCandidates = _bufferedIceCandidates.Skip(10).ToList();
                await WithState(async st => await st.SendLocalIceCandidates(candidates));
            }

            _iceBufferSemaphore.Release();
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
                            QueueIceCandidate(evt.Candidate);
                        }
                    };

                    if (_hub.IsAppInsightsEnabled)
                    {
                        _hub.InitialiazeStatsManager(_peerConnection);
                        _hub.ToggleStatsManagerConnectionState(true);
                    }
                    _peerConnection.OnAddStream += evt =>
                    {
                        if (evt.Stream != null)
                        {
                            Task.Run(async () =>
                            {
                                await WithState(async st => await st.OnAddStream(evt.Stream));
                            });
                        }
                    };
                }
                else
                {
                    if (_hub.IsAppInsightsEnabled)
                    {
                        _hub.ToggleStatsManagerConnectionState(false);
                    }
                }
            }
        }

        public CoreDispatcher Dispatcher { get { return _dispatcher; } }
        public string PeerId { get; set; }
        private BaseVoipState State { get; set; }

        internal VoipState GetVoipState()
        {
            return new VoipState
            {
                PeerId = PeerId,
                HasPeerConnection = PeerConnection != null,
                State = State.VoipState,
                IsVideoEnabled = IsVideoEnabled
            };
        }

        public void SendToPeer(string tag, string payload)
        {
            _hub.Relay(new RelayMessage
            {
                FromUserId = RegistrationSettings.UserId,
                ToUserId = PeerId,
                Tag = tag,
                Payload = payload
            });
        }

        public async Task SwitchState(BaseVoipState newState)
        {
            Debug.WriteLine($"VoipContext.SwitchState {State?.GetType().Name} -> {newState.GetType().Name}");
            if (State != null)
            {
                await State.LeaveState();
            }
            State = newState;
            await State.EnterState(this);

            Task.Run(() => _hub.OnVoipState(GetVoipState()));
        }

        // Semaphore used to make sure only one call
        // is executed at any given time.
        private readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);

        public async Task WithState(Func<BaseVoipState, Task> fn)
        {
            await _sem.WaitAsync();
            Debug.WriteLine("WithState - Semaphore - Got");

            try
            {
                await fn(State);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Debug.WriteLine("WithState - Semaphore - Release");
                _sem.Release();
            }
        }

        private UInt32 _foregroundProcessId;
        public UInt32 ForegroundProcessId
        {
            get
            {
                lock (this)
                {
                    return _foregroundProcessId;
                }
            }
            set
            {
                lock (this)
                {
                    _foregroundProcessId = value;
                }
            }
        }

        private DisplayOrientations _displayOrientation;
        public DisplayOrientations DisplayOrientation
        {
            get
            {
                lock (this)
                {
                    return _displayOrientation;
                }
            }
            set
            {
                lock (this)
                {
                    _displayOrientation = value;
                    if (Media != null)
                    {
                        Media.SetDisplayOrientation(_displayOrientation);
                    }
                }
            }
        }

        private bool _microphoneMuted;
        public bool MicrophoneMuted
        {
            get
            {
                lock (this)
                {
                    return _microphoneMuted;
                }
            }
            set
            {
                lock (this)
                {
                    _microphoneMuted = value;
                    ApplyMicrophoneConfig();
                }
            }
        }


        public IVideoRenderHelper LocalVideoRenderer { get; private set; }
        public IVideoRenderHelper RemoteVideoRenderer { get; private set; }

        public void ResetRenderers()
        {
            if (LocalVideoRenderer != null)
            {
                LocalVideoRenderer.Teardown();
            }
            if (RemoteVideoRenderer != null)
            {
                RemoteVideoRenderer.Teardown();
            }

            LocalVideoRenderer = _renderResolver();
            RemoteVideoRenderer = _renderResolver();

            LocalVideoRenderer.RenderFormatUpdate += LocalVideoRenderer_RenderFormatUpdate;
            RemoteVideoRenderer.RenderFormatUpdate += RemoteVideoRenderer_RenderFormatUpdate;
            GC.Collect();
        }

        private void ApplyMicrophoneConfig()
        {
            if (LocalStream != null)
            {
                foreach (var audioTrack in LocalStream.GetAudioTracks())
                {
                    audioTrack.Enabled = !_microphoneMuted;
                }
            }
        }

        private void ApplyVideoConfig()
        {
            if (LocalStream != null)
            {
                foreach (var videoTrack in LocalStream.GetVideoTracks())
                {
                    videoTrack.Enabled = _isVideoEnabled;
                }
            }
        }

        private DateTimeOffset _callStartDateTime;

        public void TrackCallStarted()
        {
            _hub.IsAppInsightsEnabled = SignalingSettings.AppInsightsEnabled;
            if (!_hub.IsAppInsightsEnabled)
            {
                return;
            }
            _callStartDateTime = DateTimeOffset.Now;
            var currentConnection = NetworkInformation.GetInternetConnectionProfile();
            string connType;
            switch (currentConnection.NetworkAdapter.IanaInterfaceType)
            {
                case 6:
                    connType = "Cable";
                    break;
                case 71:
                    connType = "WiFi";
                    break;
                case 243:
                    connType = "Mobile";
                    break;
                default:
                    connType = "Unknown";
                    break;
            }
            var properties = new Dictionary<string, string> { { "Connection Type", connType } };
            _hub.TrackStatsManagerEvent("CallStarted", properties);
            // start call watch to count duration for tracking as request
            _hub.StartStatsManagerCallWatch();
        }

        public void TrackCallEnded()
        {
            if (!_hub.IsAppInsightsEnabled)
            {
                return;
            }
            // log call duration as CallEnded event property
            string duration = DateTimeOffset.Now.Subtract(_callStartDateTime).Duration().ToString(@"hh\:mm\:ss");
            var properties = new Dictionary<string, string> { { "Call Duration", duration } };
            _hub.TrackStatsManagerEvent("CallEnded", properties);

            // stop call watch, so the duration will be calculated and tracked as request
            _hub.StopStatsManagerCallWatch();
        }

    }
}
