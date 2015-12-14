using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Client.Voip;
using ChatterBox.Client.Voip.States.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using webrtc_winrt_api;
using System.Threading;
using Windows.UI.Core;

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

            WebRTC.Initialize(_dispatcher);

            var idleState = new VoipState_Idle();
            SwitchState(idleState).Wait();

            Media = Media.CreateMedia();
            InitializeWebRTC();

            ResetRenderers();
        }

        private async void InitializeWebRTC()
        {
            await Media.EnumerateAudioVideoCaptureDevices();

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

        public Media Media { get; set; }

        public bool IsVideoEnabled { get; set; }

        public IVoipCoordinator VoipCoordinator { get; set; }

        public MediaStream LocalStream { get; set; }
        public MediaStream RemoteStream { get; set; }

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
                            Task.Run(async () =>
                            {
                                await WithState(async st => await st.SendLocalIceCandidate(evt.Candidate));
                            });
                        }
                    };
                    _hub.InitialiazeStatsManager(_peerConnection);
                    _hub.ToggleStatsManagerConnectionState(true);
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
                    _hub.ToggleStatsManagerConnectionState(false);
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
                    Media.SetDisplayOrientation(_displayOrientation);
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
    }
}
