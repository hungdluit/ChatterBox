using ChatterBox.Client.Common.Background;
using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Foreground.Dto.ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Signaling.Dto;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Universal.Background.DeferralWrappers;
using ChatterBox.Client.Universal.Background.Helpers;
using ChatterBox.Client.Universal.Background.Tasks;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Messages.Standard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using webrtc_winrt_api;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Universal.Services
{
    public class HubClient : DispatcherBindableBase,
        IForegroundUpdateService,
        ISignalingSocketChannel,
        IClientChannel,
        IVoipChannel,
        IForegroundChannel,
        IWebRTCSettingsService
    {
        private readonly TaskHelper _taskHelper;
        private AppServiceConnection _appConnection;
        private ApplicationDataContainer _localSettings;

        public HubClient(CoreDispatcher uiDispatcher, TaskHelper taskHelper) : base(uiDispatcher)
        {
            _taskHelper = taskHelper;
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        public bool IsConnected { get; private set; }

        #region IWebRTCSettingsService

        private Media _media;

        public IEnumerable<MediaDevice> VideoCaptureDevices
        {
            get
            {
                return _media.GetVideoCaptureDevices();
            }
        }

        public IEnumerable<MediaDevice> AudioCaptureDevices
        {
            get
            {
                return _media.GetAudioCaptureDevices();
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
                _localSettings.Values[WebRTCSettingsIds.VideoDeviceSettings] = value?.Id;
            }
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
                _localSettings.Values[WebRTCSettingsIds.AudioDeviceSettings] = value?.Id;
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
                _localSettings.Values[WebRTCSettingsIds.VideoCodecSettings] = value?.Id;
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
                _localSettings.Values[WebRTCSettingsIds.AudioCodecSettings] = value?.Id;
            }
        }

        public IEnumerable<MediaDevice> AudioPlayoutDevices
        {
            get
            {
                return _media.GetAudioPlayoutDevices();
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
                _audioPlayoutDevice = value;
                _localSettings.Values[WebRTCSettingsIds.AudioPlayoutDeviceSettings] = value?.Id;
            }
        }

        public void SetPreferredVideoCaptureFormat(int width, int height, int frameRate)
        {
            _localSettings.Values[WebRTCSettingsIds.PreferredVideoCaptureWidth] = width;
            _localSettings.Values[WebRTCSettingsIds.PreferredVideoCaptureHeight] = height;
            _localSettings.Values[WebRTCSettingsIds.PreferredVideoCaptureFrameRate] = frameRate;
        }

        public async Task InitializeWebRTC()
        {
            WebRTC.Initialize(_uiDispatcher);
            _media = await Media.CreateMediaAsync();
            await _media.EnumerateAudioVideoCaptureDevices();

            Debug.WriteLine("WebRTC initialized");
        }

        public void StartTrace()
        {
          InvokeHubChannel<IVoipChannel>();
        }

        public void StopTrace()
        {
           InvokeHubChannel<IVoipChannel>();
        }
        public void SaveTrace(TraceServerConfig traceServer)
        {
          InvokeHubChannel<IVoipChannel>(traceServer);
        }
        public void SaveTrace(string ip, int port)
        {
          TraceServerConfig traceServer = new TraceServerConfig
          {
            Ip = ip,
            Port = port
          };
          SaveTrace(traceServer);
        }

        public void ReleaseDevices()
        {
            Media.OnAppSuspending();
        }

        #endregion

        #region IClientChannel Members

        public void ClientConfirmation(Confirmation confirmation)
        {
            InvokeHubChannel<IClientChannel>(confirmation);
        }

        public void ClientHeartBeat()
        {
            InvokeHubChannel<IClientChannel>();
        }

        public void GetPeerList(Message message)
        {
            InvokeHubChannel<IClientChannel>(message);
        }

        public void Register(Registration message)
        {
            InvokeHubChannel<IClientChannel>(message);
        }

        public void Relay(RelayMessage message)
        {
            InvokeHubChannel<IClientChannel>(message);
        }

        #endregion

        #region IForegroundChannel Members

        public void OnSignaledPeerDataUpdated()
        {
            RunOnUiThread(() => OnPeerDataUpdated?.Invoke());
        }

        public void OnSignaledRegistrationStatusUpdated()
        {
            RunOnUiThread(() => OnRegistrationStatusUpdated?.Invoke());
        }

        public void OnSignaledRelayMessagesUpdated()
        {
            RunOnUiThread(() => OnRelayMessagesUpdated?.Invoke());
        }

        public void OnVoipState(VoipState state)
        {
            RunOnUiThread(() => OnVoipStateUpdate?.Invoke(state));
        }

        public void OnUpdateFrameFormat(FrameFormat frameFormat)
        {
            RunOnUiThread(() => OnFrameFormatUpdate?.Invoke(frameFormat));
        }

        public ForegroundState GetForegroundState()
        {
            return new ForegroundState { IsForegroundVisible = true };
        }

        public string GetShownUserId()
        {
            if (GetShownUser != null)
                return GetShownUser();
            return string.Empty;
        }

        #endregion

        #region IForegroundUpdateService Members

        public event Action OnPeerDataUpdated;
        public event Action OnRegistrationStatusUpdated;
        public event Action OnRelayMessagesUpdated;
        public event Action<VoipState> OnVoipStateUpdate;
        public event Action<FrameFormat> OnFrameFormatUpdate;
        public event Func<string> GetShownUser;

        #endregion

        #region ISignalingSocketChannel Members

        public ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner)
        {
            return InvokeHubChannel<ISignalingSocketChannel, ConnectionStatus>(new ConnectionOwner
            {
                OwnerId = _taskHelper.GetTask(nameof(SignalingTask)).TaskId.ToString()
            });
        }
        public void DisconnectSignalingServer()
        {
            InvokeHubChannel<ISignalingSocketChannel>();
        }

        public ConnectionStatus GetConnectionStatus()
        {
            return InvokeHubChannel<ISignalingSocketChannel, ConnectionStatus>();
        }

        #endregion

        #region IVoipChannel Members

        public void SetForegroundProcessId(uint processId)
        {
            InvokeHubChannel<IVoipChannel>(processId);
        }

        public void Answer()
        {
            InvokeHubChannel<IVoipChannel>();
        }

        public void Call(OutgoingCallRequest request)
        {
            InvokeHubChannel<IVoipChannel>(request);
        }

        public VoipState GetVoipState()
        {
            return InvokeHubChannel<IVoipChannel, VoipState>();
        }

        public void Hangup()
        {
            InvokeHubChannel<IVoipChannel>();
        }

        public void OnIceCandidate(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnIncomingCall(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnOutgoingCallAccepted(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnOutgoingCallRejected(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnRemoteHangup(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnSdpAnswer(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnSdpOffer(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void Reject(IncomingCallReject reason)
        {
            InvokeHubChannel<IVoipChannel>(reason);
        }

        public void DisplayOrientationChanged(DisplayOrientations orientation)
        {
            InvokeHubChannel<IVoipChannel>(orientation);
        }

        public void ConfigureMicrophone(MicrophoneConfig config)
        {
            InvokeHubChannel<IVoipChannel>(config);
        }

        public void SuspendVoipVideo()
        {
            InvokeHubChannel<IVoipChannel>();
        }

        public void ResumeVoipVideo()
        {
            InvokeHubChannel<IVoipChannel>();
        }

        public void ConfigureVideo(VideoConfig config)
        {
            InvokeHubChannel<IVoipChannel>(config);
        }
        #endregion

        public async Task<bool> Connect()
        {
            _appConnection = new AppServiceConnection
            {
                AppServiceName = nameof(ForegroundAppServiceTask),
                PackageFamilyName = Package.Current.Id.FamilyName
            };
            _appConnection.ServiceClosed += OnServiceClosed;
            _appConnection.RequestReceived += OnRequestReceived;
            var status = await _appConnection.OpenAsync();
            IsConnected = (status == AppServiceConnectionStatus.Success);
            return IsConnected;
        }

        private void InvokeHubChannel<TContract>(object arg = null, [CallerMemberName] string method = null)
        {
            _appConnection.InvokeChannel(typeof(TContract), arg, method);
        }

        private TResult InvokeHubChannel<TContract, TResult>(object arg = null, [CallerMemberName] string method = null)
            where TResult : class
        {
            return (TResult)_appConnection.InvokeChannel(typeof(TContract), arg, method, typeof(TResult));
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            using (new AppServiceDeferralWrapper(args.GetDeferral()))
            {
                var message = args.Request.Message.Single().Value.ToString();
                AppServiceChannelHelper.HandleRequest(args.Request, this, message);
            }
        }

        private void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            IsConnected = false;
            Debug.WriteLine("HubClient.OnServiceClosed()");
            OnDisconnectedFromHub?.Invoke();
        }

        public void OnSignaledDataUpdated()
        {
            RunOnUiThread(() => OnUpdate?.Invoke());
        }

        public void RegisterVideoElements(MediaElement self, MediaElement peer)
        {
        }

        public event Action OnUpdate;


        public event Action OnDisconnectedFromHub;
    }
}