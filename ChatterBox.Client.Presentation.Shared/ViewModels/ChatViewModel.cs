using System;
using System.Collections.Generic;
using System.Text;
using ChatterBox.Client.Presentation.Shared.Models;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.MVVM.Utils;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public sealed class ChatViewModel : BindableBase
    {
        private ContactModel _contactModel;
        private ActionCommand _closeChat;
        private ActionCommand _call;
        private ActionCommand _callOnlyAudio;

        public ChatViewModel()
        {
            CloseChat = new ActionCommand(CloseChatExecute);
            Call = new ActionCommand(CallExecute);
            CallOnlyAudio = new ActionCommand(CallOnlyAudioExecute);
        }

        public ContactModel Contact
        {
            get { return _contactModel; }
            set { SetProperty(ref _contactModel, value); }
        }

        public ActionCommand CloseChat
        {
            get { return _closeChat; }
            set { SetProperty(ref _closeChat, value); }
        }

        public ActionCommand Call
        {
            get { return _call; }
            set { SetProperty(ref _call, value); }
        }

        public ActionCommand CallOnlyAudio
        {
            get { return _callOnlyAudio; }
            set { SetProperty(ref _callOnlyAudio, value); }
        }

        private void CloseChatExecute(object param)
        {
            OnCompleted?.Invoke();
        }

        private void CallExecute(object param)
        {
            OnCall?.Invoke(false);
        }

        private void CallOnlyAudioExecute(object param)
        {
            OnCall?.Invoke(true);
        }

        internal void OnNavigatedTo(ContactModel contact)
        {
            Contact = contact;
        }

        public event Action OnCompleted;
        public event Action<bool> OnCall;
    }
}
