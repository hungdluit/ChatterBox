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
using ChatterBox.Common.Communication.Shared.Messages.Relay;

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

        public ConversationViewModel(IClientChannel clientChannel,
            IForegroundUpdateService foregroundUpdateService,
            IVoipChannel voipChannel)
        {
            _clientChannel = clientChannel;
            _voipChannel = voipChannel;
            foregroundUpdateService.OnRelayMessagesUpdated += OnRelayMessagesUpdated;
            foregroundUpdateService.OnVoipStateUpdate += OnVoipStateUpdate;
            SendInstantMessageCommand = new DelegateCommand(OnSendInstantMessageCommandExecute,
                OnSendInstantMessageCommandCanExecute);
            CallCommand = new DelegateCommand(OnCallCommandExecute, OnCallCommandCanExecute);
            HangupCommand = new DelegateCommand(OnHangupCommandExecute, OnHangupCommandCanExecute);
            AnswerCommand = new DelegateCommand(OnAnswerCommandExecute, OnAnswerCommandCanExecute);
            RejectCommand = new DelegateCommand(OnRejectCommandExecute, OnRejectCommandCanExecute);
            CloseConversationCommand = new DelegateCommand(OnCloseConversationCommandExecute);
        }

        public DelegateCommand AnswerCommand { get; }
        public DelegateCommand CallCommand { get; }
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
                if (SetProperty(ref _isInCallMode, value))
                    UpdateCommandStates();
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

        public void Initialize()
        {
            var voipState = _voipChannel.GetVoipState();
            OnVoipStateUpdate(voipState);
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
                PeerUserId = UserId
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
    }
}