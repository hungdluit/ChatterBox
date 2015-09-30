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

        public CallViewModel()
        {
            CloseCall = new ActionCommand(CloseCallExecute);
            SwitchMic = new ActionCommand(SwitchMicExecute);
            SwitchVideo = new ActionCommand(SwitchVideoExecute);

            IsMicEnabled = true;
            IsVideoEnabled = true;
        }

        public ActionCommand CloseCall
        {
            get { return _closeCall; }
            set { SetProperty(ref _closeCall, value); }
        }

        private void CloseCallExecute(object param)
        {
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

        public event Action OnCompleted;

        internal void OnNvigatedTo(bool onlyAudio)
        {
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
        }
    }
}
