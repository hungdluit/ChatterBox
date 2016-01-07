using System;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Presentation.Shared.MVVM;
using Windows.ApplicationModel;
using System.Collections.ObjectModel;
using webrtc_winrt_api;
using System.Linq;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Microsoft.Practices.Unity;
using ChatterBox.Client.Presentation.Shared.Services;
using System.Collections.Generic;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private string _domain;
        private ApplicationDataContainer _localSettings;
        private string _signalingServerHost;
        private int _signalingServerPort;
        private IWebRTCSettingsService _webrtcSettingsService;
        private CoreDispatcher _dispatcher;
        private bool _appInsightsEnabled;
        private bool _webrtcTraceEnabled;
        private string _webRTCTraceServerIP="localhost";
        private string _webRTCTraceServerPort="55000";
        private readonly string[] incompatibleAudioCodecs =
            new string[] { "CN32000", "CN16000", "CN8000", "red8000", "telephone-event8000" };
        private string SelectedFrameRateId = nameof(SelectedFrameRateId) + "Frame";

        public SettingsViewModel(IUnityContainer container,
                                 CoreDispatcher dispatcher)
        {
            _localSettings = ApplicationData.Current.LocalSettings;
            _dispatcher = dispatcher;

            _webrtcSettingsService = container.Resolve<IWebRTCSettingsService>();

            CloseCommand = new DelegateCommand(OnCloseCommandExecute);
            SaveCommand = new DelegateCommand(OnSaveCommandExecute);
            DeleteIceServerCommand = new DelegateCommand<IceServerViewModel>(OnDeleteIceServerCommandExecute);
            AddIceServerCommand = new DelegateCommand(OnAddIceServerCommandExecute);            
        }

        #region Navigation

        public void OnNavigatedTo()
        {
            Reset();
        }

        public DelegateCommand CloseCommand { get; set; }

        public event Action OnClose;

        private void OnCloseCommandExecute()
        {
            OnClose?.Invoke();
        }

        private void OnSaveCommandExecute()
        {
            SignalingSettings.SignalingServerPort = SignalingServerPort.ToString();
            SignalingSettings.SignalingServerHost = SignalingServerHost;
            RegistrationSettings.Domain = Domain;

#if WIN10
            SignalingSettings.AppInsightsEnabled = AppInsightsEnabled;
#endif

            if (SelectedCamera != null)
            {
                _webrtcSettingsService.VideoDevice = SelectedCamera;
                _localSettings.Values[nameof(SelectedCamera)] = SelectedCamera.Id;
            }

            if (SelectedMicrophone != null)
            {
                _webrtcSettingsService.AudioDevice = SelectedMicrophone;
                _localSettings.Values[nameof(SelectedMicrophone)] = SelectedMicrophone.Id;
            }

            if (SelectedVideoCodec != null)
            {
                _webrtcSettingsService.VideoCodec = SelectedVideoCodec;
                _localSettings.Values[nameof(SelectedVideoCodec)] = SelectedVideoCodec.Id;
            }

            if (SelectedAudioCodec != null)
            {
                _webrtcSettingsService.AudioCodec = SelectedAudioCodec;
                _localSettings.Values[nameof(SelectedAudioCodec)] = SelectedAudioCodec.Id;
            }

            if (SelectedCapFPSItem != null)
            {
                _webrtcSettingsService.SetPreferredVideoCaptureFormat((int)SelectedCapFPSItem.Width,
                                                                      (int)SelectedCapFPSItem.Height,
                                                                      (int)SelectedCapFPSItem.FrameRate);
                _localSettings.Values[nameof(SelectedCapResItem)] = SelectedCapResItem;
                _localSettings.Values[SelectedFrameRateId] = (SelectedCapFPSItem != null) ? SelectedCapFPSItem.FrameRate : 0;
            }

            var newList = new List<IceServer>();
            foreach (var iceServerVm in IceServers)
            {
                if (iceServerVm.Apply())
                {
                    newList.Add(iceServerVm.IceServer);
                }
            }
            IceServerSettings.IceServers = newList;

            OnCloseCommandExecute();
        }

        private void Reset()
        {
            SignalingServerPort = int.Parse(SignalingSettings.SignalingServerPort);
            SignalingServerHost = SignalingSettings.SignalingServerHost;
            Domain = RegistrationSettings.Domain;
#if WIN10
            AppInsightsEnabled = SignalingSettings.AppInsightsEnabled;
#endif

            Cameras = new ObservableCollection<MediaDevice>(_webrtcSettingsService.VideoCaptureDevices);
            if (_localSettings.Values[nameof(SelectedCamera)] != null)
            {
                var id = (string)_localSettings.Values[nameof(SelectedCamera)];
                var camera = Cameras.SingleOrDefault(c => c.Id.Equals(id));
                if (camera != null)
                {
                    SelectedCamera = camera;
                    _webrtcSettingsService.VideoDevice = camera;
                }
            }

            Microphones = new ObservableCollection<MediaDevice>(_webrtcSettingsService.AudioCaptureDevices);
            if (_localSettings.Values[nameof(SelectedMicrophone)] != null)
            {
                var id = (string)_localSettings.Values[nameof(SelectedMicrophone)];
                var mic = Microphones.SingleOrDefault(m => m.Id.Equals(id));
                if (mic != null)
                {
                    SelectedMicrophone = mic;
                    _webrtcSettingsService.AudioDevice = mic;
                }
            }

            AudioCodecs = new ObservableCollection<CodecInfo>();
            var audioCodecList = WebRTC.GetAudioCodecs();
            foreach (var audioCodec in audioCodecList)
            {
                if (!incompatibleAudioCodecs.Contains(audioCodec.Name + audioCodec.Clockrate))
                {
                    AudioCodecs.Add(audioCodec);
                }
            }
            if (_localSettings.Values[nameof(SelectedAudioCodec)] != null)
            {
                var audioCodecId = (int)_localSettings.Values[nameof(SelectedAudioCodec)];
                var audioCodec = AudioCodecs.SingleOrDefault(a => a.Id.Equals(audioCodecId));
                if (audioCodec != null)
                {
                    SelectedAudioCodec = audioCodec;
                    _webrtcSettingsService.AudioCodec = audioCodec;
                }
            }

            var videoCodecList = WebRTC.GetVideoCodecs().OrderBy(codec =>
            {
                switch (codec.Name)
                {
                    case "VP8": return 1;
                    case "VP9": return 2;
                    case "H264": return 3;
                    default: return 99;
                }
            });
            VideoCodecs = new ObservableCollection<CodecInfo>(videoCodecList);
            if (_localSettings.Values[nameof(SelectedVideoCodec)] != null)
            {
                var videoCodecId = (int)_localSettings.Values[nameof(SelectedVideoCodec)];
                var videoCodec = VideoCodecs.SingleOrDefault(v => v.Id.Equals(videoCodecId));
                if (videoCodec != null)
                {
                    SelectedVideoCodec = videoCodec;
                    _webrtcSettingsService.VideoCodec = videoCodec;
                }
            }

            IceServers = new ObservableCollection<IceServerViewModel>(
                IceServerSettings.IceServers.Select(ices => new IceServerViewModel(ices)));
        }

        public DelegateCommand<IceServerViewModel> DeleteIceServerCommand { get; }

        private void OnDeleteIceServerCommandExecute(IceServerViewModel iceServerVm)
        {
            IceServers.Remove(iceServerVm);
        }

        public DelegateCommand AddIceServerCommand { get; }

        private void OnAddIceServerCommandExecute()
        {
            IceServers.Insert(0, new IceServerViewModel(new IceServer()));
        }

        #endregion

        #region ChatterBox settings

        public string Domain
        {
            get { return _domain; }
            set { SetProperty(ref _domain, value); }
        }

        public DelegateCommand SaveCommand { get; set; }

        public string SignalingServerHost
        {
            get { return _signalingServerHost; }
            set { SetProperty(ref _signalingServerHost, value); }
        }

        public int SignalingServerPort
        {
            get { return _signalingServerPort; }
            set { SetProperty(ref _signalingServerPort, value); }
        }

        public string WebRTCTraceServerIp
        {
            get { return _webRTCTraceServerIP; }
            set { SetProperty(ref _webRTCTraceServerIP, value); }
        }

        public string WebRTCTraceServerPort
        {
            get { return _webRTCTraceServerPort; }
            set { SetProperty(ref _webRTCTraceServerPort, value); }
        }

        public string ApplicationVersion
        {
            get
            {
                return "ChatterBox " + string.Format("Version: {0}.{1}.{2}.{3}",
                  Package.Current.Id.Version.Major,
                  Package.Current.Id.Version.Minor,
                  Package.Current.Id.Version.Build,
                  Package.Current.Id.Version.Revision);
            }
        }

        public bool AppInsightsEnabled
        {
            get
            {
                return _appInsightsEnabled;
            }
            set
            {
                SetProperty(ref _appInsightsEnabled, value);
            }
        }
        public bool WebRTCTraceEnabled
        {
            get
            {
                return _webrtcTraceEnabled;
            }
            set
            {
                if (!SetProperty(ref _webrtcTraceEnabled, value))
                {
                    return;
                }

                if (_webrtcTraceEnabled)
                {
                    _webrtcSettingsService.StartTrace();
                }
                else
                {
                    _webrtcSettingsService.StopTrace();
                    _webrtcSettingsService.SaveTrace(_webRTCTraceServerIP, Int32.Parse(_webRTCTraceServerPort));
                }
            }
        }

        public bool IsWin10App
        {
            get
            {
#if WIN10
                    return true;
#endif
                return false;
           }
        }

        #endregion

        #region WebRTC settings

        private ObservableCollection<MediaDevice> _cameras;
        public ObservableCollection<MediaDevice> Cameras
        {
            get { return _cameras; }
            set { SetProperty(ref _cameras, value); }
        }

        private ObservableCollection<MediaDevice> _microphones;
        public ObservableCollection<MediaDevice> Microphones
        {
            get { return _microphones; }
            set { SetProperty(ref _microphones, value); }
        }

        private MediaDevice _selectedCamera;
        public MediaDevice SelectedCamera
        {
            get { return _selectedCamera; }
            set { SetProperty(ref _selectedCamera, value); SetSelectedCamera(); }
        }

        private MediaDevice _selectedMicrophone;
        public MediaDevice SelectedMicrophone
        {
            get { return _selectedMicrophone; }
            set
            {
                SetProperty(ref _selectedMicrophone, value);
                var localSettings = ApplicationData.Current.LocalSettings;
            }
        }

        private ObservableCollection<string> _allCapRes;
        public ObservableCollection<string> AllCapRes
        /// <summary>
        /// The list of all capture resolutions.
        /// </summary>
        {
            get { return _allCapRes; }
            set { SetProperty(ref _allCapRes, value); }
        }

        private string _selectedCapResItem;
        public string SelectedCapResItem
        {
            get { return _selectedCapResItem; }
            set { SetProperty(ref _selectedCapResItem, value); SetSelectedCapResItem(); }
        }

        private ObservableCollection<CaptureCapability> _allCapFPS;
        public ObservableCollection<CaptureCapability> AllCapFPS
        /// <summary>
        /// The list of all capture frame rates.
        /// </summary>
        {
            get { return _allCapFPS; }
            set { SetProperty(ref _allCapFPS, value); }
        }

        private CaptureCapability _selectedCapFPSItem;
        public CaptureCapability SelectedCapFPSItem
        /// <summary>
        /// The selected capture frame rate.
        /// </summary>
        {
            get { return _selectedCapFPSItem; }
            set { SetProperty(ref _selectedCapFPSItem, value); }
        }

        private ObservableCollection<CodecInfo> _videoCodecs;
        /// <summary>
        /// The list of video codecs.
        /// </summary>
        public ObservableCollection<CodecInfo> VideoCodecs
        {
            get { return _videoCodecs; }
            set { SetProperty(ref _videoCodecs, value); }
        }

        private ObservableCollection<CodecInfo> _audioCodecs;
        /// <summary>
        /// The list of audio codecs.
        /// </summary>
        public ObservableCollection<CodecInfo> AudioCodecs
        {
            get { return _audioCodecs; }
            set { SetProperty(ref _audioCodecs, value); }
        }

        private CodecInfo _selectedVideoCodec;
        public CodecInfo SelectedVideoCodec
        {
            get { return _selectedVideoCodec; }
            set { SetProperty(ref _selectedVideoCodec, value); }
        }

        private CodecInfo _selectedAudioCodec;
        public CodecInfo SelectedAudioCodec
        {
            get { return _selectedAudioCodec; }
            set { SetProperty(ref _selectedAudioCodec, value); }
        }

        private ObservableCollection<IceServerViewModel> _iceServers;
        public ObservableCollection<IceServerViewModel> IceServers
        {
            get { return _iceServers; }
            set { SetProperty(ref _iceServers, value); }
        }

        private IceServerViewModel _selectedIceServer;
        public IceServerViewModel SelectedIceServer
        {
            get { return _selectedIceServer; }
            set
            {
                if (_selectedIceServer != null) _selectedIceServer.IsSelected = false;
                SetProperty(ref _selectedIceServer, value);
                if (_selectedIceServer != null) _selectedIceServer.IsSelected = true;
            }
        }

        #endregion

        #region WebRTC settings helpers

        void SetSelectedCamera()
        {
            if (SelectedCamera == null)
            {
                string errorMsg = "SetSelectedCamera: Skip GetVideoCaptureCapabilities (Trying to set Null)";
                Debug.WriteLine(errorMsg);
                return;
            }
            if (_allCapRes == null)
            {
                _allCapRes = new ObservableCollection<string>();
            }
            else
            {
                _allCapRes.Clear();
            }
            var opRes = SelectedCamera.GetVideoCaptureCapabilities();
            opRes.AsTask().ContinueWith(resolutions =>
            {
                var task = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (resolutions.IsFaulted)
                    {
                        Exception ex = resolutions.Exception;
                        while (ex is AggregateException && ex.InnerException != null)
                            ex = ex.InnerException;
                        string errorMsg = "SetSelectedCamera: Failed to GetVideoCaptureCapabilities (Error: " + ex.Message + ")";
                        Debug.WriteLine(errorMsg);
                        var msgDialog = new MessageDialog(errorMsg);
                        await msgDialog.ShowAsync();
                        return;
                    }
                    if (resolutions.Result == null)
                    {
                        string errorMsg = "SetSelectedCamera: Failed to GetVideoCaptureCapabilities (Result is null)";
                        Debug.WriteLine(errorMsg);
                        var msgDialog = new MessageDialog(errorMsg);
                        await msgDialog.ShowAsync();
                        return;
                    }
                    var uniqueRes = resolutions.Result.GroupBy(test => test.ResolutionDescription).Select(grp => grp.First()).ToList();
                    CaptureCapability defaultResolution = null;
                    foreach (var resolution in uniqueRes)
                    {
                        if (defaultResolution == null)
                        {
                            defaultResolution = resolution;
                        }
                        _allCapRes.Add(resolution.ResolutionDescription);
                        if ((resolution.Width == 640) && (resolution.Height == 480))
                        {
                            defaultResolution = resolution;
                        }
                    }
                    string selectedCapResItem = string.Empty;

                    if (_localSettings.Values[nameof(SelectedCapResItem)] != null)
                    {
                        selectedCapResItem = (string)_localSettings.Values[nameof(SelectedCapResItem)];
                    }

                    if (!string.IsNullOrEmpty(selectedCapResItem) && _allCapRes.Contains(selectedCapResItem))
                    {
                        SelectedCapResItem = selectedCapResItem;
                    }
                    else
                    {
                        SelectedCapResItem = defaultResolution.ResolutionDescription;
                    }
                });
                var uiTask = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                {
                    OnPropertyChanged(nameof(AllCapRes));
                }));
            });
        }

        void SetSelectedCapResItem()
        {
            if (AllCapFPS == null)
            {
                AllCapFPS = new ObservableCollection<CaptureCapability>();
            }
            else
            {
                AllCapFPS.Clear();
            }
            var opCap = SelectedCamera.GetVideoCaptureCapabilities();
            opCap.AsTask().ContinueWith(caps =>
            {
                var fpsList = from cap in caps.Result where cap.ResolutionDescription == SelectedCapResItem select cap;
                var t = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        CaptureCapability defaultFPS = null;
                        uint selectedCapFPSFrameRate = 0;
                        if (_localSettings.Values[SelectedFrameRateId] != null)
                        {
                            selectedCapFPSFrameRate = (uint)_localSettings.Values[SelectedFrameRateId];
                        }

                        foreach (var fps in fpsList)
                        {
                            if (selectedCapFPSFrameRate != 0 && fps.FrameRate == selectedCapFPSFrameRate)
                            {
                                defaultFPS = fps;
                            }
                            AllCapFPS.Add(fps);
                            if (defaultFPS == null)
                            {
                                defaultFPS = fps;
                            }
                        }
                        SelectedCapFPSItem = defaultFPS;
                        _webrtcSettingsService.SetPreferredVideoCaptureFormat((int)SelectedCapFPSItem.Width,
                                                                              (int)SelectedCapFPSItem.Height,
                                                                              (int)SelectedCapFPSItem.FrameRate);
                    });
                var uiTask = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                {
                    OnPropertyChanged(nameof(AllCapFPS));
                }));
            });
        }

        #endregion
    }
}