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

        internal void OnNavigatedTo(ContactModel contact)
        {
            Contact = contact;
        }

        public event Action OnCompleted;
    }
}
