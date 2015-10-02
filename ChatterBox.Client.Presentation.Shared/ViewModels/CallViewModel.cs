using ChatterBox.Client.Presentation.Shared.Models;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.MVVM.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public sealed class CallViewModel : BindableBase
    {
        private ActionCommand _closeCall;
        private ActionCommand _switchMic;
        private ActionCommand _switchVideo;
        private bool _isMicEnabled;
        private bool _isVideoEnabled;
        private Uri _peerPlaceholderImage;
        private Uri _selfPlaceholderImage;
        private bool _isPeerVideoAvailable;
        private bool _isSelfVideoAvailable;
        private bool _isInCall;
        private ContactModel _contact;

        public CallViewModel()
        {
            CloseCall = new ActionCommand(CloseCallExecute);
            SwitchMic = new ActionCommand(SwitchMicExecute);
            SwitchVideo = new ActionCommand(SwitchVideoExecute);

            IsMicEnabled = true;
            IsVideoEnabled = true;

            PeerPlaceholderImage = new Uri("ms-appx:///Assets/profile_4.png");
            SelfPlaceholderImage = new Uri("ms-appx:///Assets/profile_1.png");
        }

        public ContactModel Contact
        {
            get { return _contact; }
            set { SetProperty(ref _contact, value); }
        }

        public bool IsInCall
        {
            get { return _isInCall; }
            set { SetProperty(ref _isInCall, value); }
        }

        public Uri PeerPlaceholderImage
        {
            get { return _peerPlaceholderImage; }
            set { SetProperty(ref _peerPlaceholderImage, value); }
        }

        public Uri SelfPlaceholderImage
        {
            get { return _selfPlaceholderImage; }
            set { SetProperty(ref _selfPlaceholderImage, value); }
        }

        public ActionCommand CloseCall
        {
            get { return _closeCall; }
            set { SetProperty(ref _closeCall, value); }
        }

        private void CloseCallExecute(object param)
        {
            IsInCall = false;
            OnCompleted?.Invoke();
        }

        public ActionCommand SwitchMic
        {
            get { return _switchMic; }
            set { SetProperty(ref _switchMic, value); }
        }

        private void SwitchMicExecute(object param)
        {
            IsMicEnabled = !IsMicEnabled;
        }

        public bool IsMicEnabled
        {
            get { return _isMicEnabled; }
            set { SetProperty(ref _isMicEnabled, value); }
        }

        public ActionCommand SwitchVideo
        {
            get { return _switchVideo; }
            set { SetProperty(ref _switchVideo, value); }
        }

        private void SwitchVideoExecute(object param)
        {
            IsVideoEnabled = !IsVideoEnabled;
        }

        public bool IsVideoEnabled
        {
            get { return _isVideoEnabled; }
            set { SetProperty(ref _isVideoEnabled, value); }
        }

        public bool IsPeerVideoAvailable
        {
            get { return _isPeerVideoAvailable; }
            set { SetProperty(ref _isPeerVideoAvailable, value); }
        }

        public bool IsSelfVideoAvailable
        {
            get { return _isSelfVideoAvailable; }
            set { SetProperty(ref _isSelfVideoAvailable, value); }
        }

        public event Action OnCompleted;

        internal void OnNvigatedTo(ContactModel contact, bool onlyAudio)
        {
            Contact = contact;
            if (onlyAudio)
            {
                IsVideoEnabled = false;
                IsMicEnabled = true;
            }
            else
            {
                IsVideoEnabled = true;
                IsMicEnabled = true;
            }
            IsInCall = true;
        }
    }
}
