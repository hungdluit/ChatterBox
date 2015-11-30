using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using ChatterBox.Client.Common.Avatars;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Common.Signaling.PersistedData;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Messages.Relay;
using System.Diagnostics;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class ConversationViewModel : BindableBase
    {
        private readonly IClientChannel _clientChannel;
        private readonly IVoipChannel _voipChannel;
        private string _instantMessage;
        private bool _isCallConnected;
        private bool _isInCallMode;
        private bool _isLocalRinging;
        private bool _isOnline;
        private bool _isOtherConversationInCallMode;
        private bool _isRemoteRinging;
        private string _name;
        private ImageSource _profileSource;
        private string _userId;
        private long _localSwapChainHandle;
        private long _remoteSwapChainHandle;
        private Windows.Foundation.Size _localNativeVideoSize;
        private Windows.Foundation.Size _remoteNativeVideoSize;

        public ConversationViewModel(IClientChannel clientChannel,
            IForegroundUpdateService foregroundUpdateService,
            IVoipChannel voipChannel)
        {
            _clientChannel = clientChannel;
            _voipChannel = voipChannel;
            foregroundUpdateService.OnRelayMessagesUpdated += OnRelayMessagesUpdated;
            foregroundUpdateService.OnVoipStateUpdate += OnVoipStateUpdate;
            foregroundUpdateService.OnFrameFormatUpdate += OnFrameFormatUpdate;
            SendInstantMessageCommand = new DelegateCommand(OnSendInstantMessageCommandExecute,
                OnSendInstantMessageCommandCanExecute);
            CallCommand = new DelegateCommand(OnCallCommandExecute, OnCallCommandCanExecute);
            VideoCallCommand = new DelegateCommand(OnVideoCallCommandExecute, OnVideoCallCommandCanExecute);
            HangupCommand = new DelegateCommand(OnHangupCommandExecute, OnHangupCommandCanExecute);
            AnswerCommand = new DelegateCommand(OnAnswerCommandExecute, OnAnswerCommandCanExecute);
            RejectCommand = new DelegateCommand(OnRejectCommandExecute, OnRejectCommandCanExecute);
            CloseConversationCommand = new DelegateCommand(OnCloseConversationCommandExecute);
        }

        public DelegateCommand AnswerCommand { get; }
        public DelegateCommand CallCommand { get; }
        public DelegateCommand VideoCallCommand { get; }
        public DelegateCommand CloseConversationCommand { get; }
        public DelegateCommand HangupCommand { get; }

        public string InstantMessage
        {
            get { return _instantMessage; }
            set
            {
                if (SetProperty(ref _instantMessage, value))
                {
                    SendInstantMessageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<InstantMessageViewModel> InstantMessages { get; } =
            new ObservableCollection<InstantMessageViewModel>();

        public bool IsCallConnected
        {
            get { return _isCallConnected; }
            set
            {
                if (SetProperty(ref _isCallConnected, value))
                    UpdateCommandStates();
            }
        }

        public bool IsInCallMode
        {
            get { return _isInCallMode; }
            set
            {
                if (!SetProperty(ref _isInCallMode, value)) return;
                UpdateCommandStates();
                if (value)OnIsInCallMode?.Invoke(this);
            }
        }

        public bool IsLocalRinging
        {
            get { return _isLocalRinging; }
            set
            {
                if (SetProperty(ref _isLocalRinging, value))
                    UpdateCommandStates();
            }
        }

        public bool IsOnline
        {
            get { return _isOnline; }
            set { SetProperty(ref _isOnline, value); }
        }

        public bool IsOtherConversationInCallMode
        {
            get { return _isOtherConversationInCallMode; }
            set
            {
                if (SetProperty(ref _isOtherConversationInCallMode, value))
                    UpdateCommandStates();
            }
        }

        public bool IsRemoteRinging
        {
            get { return _isRemoteRinging; }
            set
            {
                if (SetProperty(ref _isRemoteRinging, value))
                    UpdateCommandStates();
            }
        }

        bool _isPeerVideoAvailable;
        public bool IsPeerVideoAvailable
        {
            get { return _isPeerVideoAvailable; }
            set { SetProperty(ref _isPeerVideoAvailable, value); }
        }

        bool _isSelfVideoAvailable;
        public bool IsSelfVideoAvailable
        {
            get { return _isSelfVideoAvailable; }
            set { SetProperty(ref _isSelfVideoAvailable, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public ImageSource OwnProfileSource { get; } = new BitmapImage(new Uri(AvatarLink.For(SignalingStatus.Avatar)));

        public ImageSource ProfileSource
        {
            get { return _profileSource; }
            set { SetProperty(ref _profileSource, value); }
        }

        public DelegateCommand RejectCommand { get; }
        public DelegateCommand SendInstantMessageCommand { get; }

        public string UserId
        {
            get { return _userId; }
            set { SetProperty(ref _userId, value); }
        }

        public long LocalSwapChainPanelHandle
        {
            get
            {
                return _localSwapChainHandle;
            }
            set
            {
                IsSelfVideoAvailable = value > 0;
                _localSwapChainHandle = value;
                // Don't use SetPropert() because it does nothing if the value
                // doesn't change but in this case it must always update the
                // swap chain panel.
                OnPropertyChanged("LocalSwapChainPanelHandle");
            }
        }

        public long RemoteSwapChainPanelHandle
        {
            get
            {
                return _remoteSwapChainHandle;
            }
            set
            {
                IsPeerVideoAvailable = value > 0;
                _remoteSwapChainHandle = value;
                // Don't use SetPropert() because it does nothing if the value
                // doesn't change but in this case it must always update the
                // swap chain panel.
                OnPropertyChanged("RemoteSwapChainPanelHandle");
            }
        }

        public Windows.Foundation.Size LocalNativeVideoSize
        {
            get
            {
                return _localNativeVideoSize;
            }
            set
            {
                SetProperty(ref _localNativeVideoSize, value);
            }
        }

        public Windows.Foundation.Size RemoteNativeVideoSize
        {
            get
            {
                return _remoteNativeVideoSize;
            }
            set
            {
                SetProperty(ref _remoteNativeVideoSize, value);
            }
        }

        public void Initialize()
        {
            var voipState = _voipChannel.GetVoipState();
            if (voipState != null)
            {
                OnVoipStateUpdate(voipState);
            }
        }

        private bool OnAnswerCommandCanExecute()
        {
            return IsLocalRinging;
        }

        private void OnAnswerCommandExecute()
        {
            _voipChannel.Answer();
        }

        private bool OnCallCommandCanExecute()
        {
            return !IsInCallMode && !IsOtherConversationInCallMode;
        }

        private void OnCallCommandExecute()
        {
            _voipChannel.Call(new OutgoingCallRequest
            {
                PeerUserId = UserId,
                Video = false
            });
            IsSelfVideoAvailable = true;
        }

        private bool OnVideoCallCommandCanExecute()
        {
            return !IsInCallMode && !IsOtherConversationInCallMode;
        }

        private void OnVideoCallCommandExecute()
        {
            _voipChannel.Call(new OutgoingCallRequest
            {
                PeerUserId = UserId,
                Video = true
            });
        }

        public event Action<ConversationViewModel> OnCloseConversation;

        private void OnCloseConversationCommandExecute()
        {
            OnCloseConversation?.Invoke(this);
        }

        private bool OnHangupCommandCanExecute()
        {
            return IsCallConnected || IsRemoteRinging;
        }

        private void OnHangupCommandExecute()
        {
            _voipChannel.Hangup();
        }

        private bool OnRejectCommandCanExecute()
        {
            return IsLocalRinging;
        }

        private void OnRejectCommandExecute()
        {
            _voipChannel.Reject(new IncomingCallReject
            {
                Reason = "Rejected"
            });
        }

        private void OnRelayMessagesUpdated()
        {
            var newMessages = SignaledRelayMessages.Messages
                .Where(s => s.Tag == RelayMessageTags.InstantMessage && s.FromUserId == _userId).ToList();
            foreach (var message in newMessages)
            {
                InstantMessages.Add(new InstantMessageViewModel
                {
                    Message = message.Payload,
                    DateTime = message.SentDateTimeUtc.LocalDateTime,
                    Sender = Name,
                    IsSender = false
                });
                SignaledRelayMessages.Delete(message.Id);
            }
        }

        private bool OnSendInstantMessageCommandCanExecute()
        {
            return !string.IsNullOrWhiteSpace(InstantMessage);
        }

        private void OnSendInstantMessageCommandExecute()
        {
            var message = new RelayMessage
            {
                SentDateTimeUtc = DateTimeOffset.UtcNow,
                ToUserId = UserId,
                FromUserId = RegistrationSettings.UserId,
                Payload = InstantMessage.Trim(),
                Tag = RelayMessageTags.InstantMessage
            };
            InstantMessage = null;
            _clientChannel.Relay(message);
            InstantMessages.Add(new InstantMessageViewModel
            {
                Message = message.Payload,
                DateTime = message.SentDateTimeUtc.LocalDateTime,
                IsSender = true,
                Sender = RegistrationSettings.Name
            });
        }

        private void OnVoipStateUpdate(VoipState voipState)
        {
            switch (voipState.State)
            {
                case VoipStateEnum.Idle:
                    IsInCallMode = false;
                    IsLocalRinging = false;
                    IsRemoteRinging = false;
                    IsCallConnected = false;
                    IsOtherConversationInCallMode = false;
                    LocalSwapChainPanelHandle = 0;
                    RemoteSwapChainPanelHandle = 0;
                    break;
                case VoipStateEnum.LocalRinging:
                    if (voipState.PeerId == UserId)
                    {
                        IsInCallMode = true;
                        IsLocalRinging = true;
                        IsRemoteRinging = false;
                        IsCallConnected = false;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                case VoipStateEnum.RemoteRinging:
                    if (voipState.PeerId == UserId)
                    {
                        IsInCallMode = true;
                        IsLocalRinging = false;
                        IsRemoteRinging = true;
                        IsCallConnected = false;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                case VoipStateEnum.EstablishOutgoing:
                    if (voipState.PeerId == UserId)
                    {
                        IsInCallMode = true;
                        IsLocalRinging = false;
                        IsRemoteRinging = false;
                        IsCallConnected = true;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                case VoipStateEnum.EstablishIncoming:
                    if (voipState.PeerId == UserId)
                    {
                        IsInCallMode = true;
                        IsLocalRinging = false;
                        IsRemoteRinging = false;
                        IsCallConnected = true;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                case VoipStateEnum.HangingUp:
                    if (voipState.PeerId == UserId)
                    {
                        IsInCallMode = true;
                        IsLocalRinging = false;
                        IsRemoteRinging = false;
                        IsCallConnected = true;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                case VoipStateEnum.ActiveCall:
                    if (voipState.PeerId == UserId)
                    {
                        IsInCallMode = true;
                        IsLocalRinging = false;
                        IsRemoteRinging = false;
                        IsCallConnected = true;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void OnFrameFormatUpdate(FrameFormat obj)
        {
            if (!IsInCallMode)
            {
                return;
            }

            if(obj.IsLocal)
            {
                LocalSwapChainPanelHandle = obj.SwapChainHandle;
                var s = new Windows.Foundation.Size();
                s.Width = (float)obj.Width;
                s.Height = (float)obj.Height;
                LocalNativeVideoSize = s;
            }
            else
            {
                RemoteSwapChainPanelHandle = obj.SwapChainHandle;
                var s = new Windows.Foundation.Size();
                s.Width = (float)obj.Width;
                s.Height = (float)obj.Height;
                RemoteNativeVideoSize = s;
            }
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        private void UpdateCommandStates()
        {
            CallCommand.RaiseCanExecuteChanged();
            AnswerCommand.RaiseCanExecuteChanged();
            HangupCommand.RaiseCanExecuteChanged();
            RejectCommand.RaiseCanExecuteChanged();
        }

        public event Action<ConversationViewModel> OnIsInCallMode;
    }
}