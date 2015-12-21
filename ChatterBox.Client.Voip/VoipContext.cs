using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Client.Voip;
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

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipContext
    {
        private CoreDispatcher _dispatcher;
        private Func<IVideoRenderHelper> _renderResolver;
        private IHub _hub;

        public VoipContext(IHub hub,
                           CoreDispatcher dispatcher,
                           Func<IVideoRenderHelper> renderResolver,
                           IVoipCoordinator coordinator)
        {
            _hub = hub;
            _dispatcher = dispatcher;
            _renderResolver = renderResolver;
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
                // TODO: Remove once those are driven by the UI.
                var audioCaptureDevices = Media.GetAudioCaptureDevices();
                Media.SelectAudioDevice(audioCaptureDevices[0]);
                var audioPlayoutDevices = Media.GetAudioPlayoutDevices();
                Media.SelectAudioPlayoutDevice(audioPlayoutDevices[0]);
            }
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

        public bool IsVideoEnabled { get; set; }

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
    }
}
