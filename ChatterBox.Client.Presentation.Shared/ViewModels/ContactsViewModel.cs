using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Signaling;
using ChatterBox.Client.Signaling.Shared.Avatars;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public sealed class ContactsViewModel : BindableBase
    {
        private readonly Func<ConversationViewModel> _contactFactory;
        private ConversationViewModel _selectedConversation;
        private bool _isConversationsListVisible;
        private bool _isSeparatorVisible;

        public ContactsViewModel(ISignalingUpdateService signalingUpdateService,
            Func<ConversationViewModel> contactFactory)
        {
            _contactFactory = contactFactory;
            signalingUpdateService.OnUpdate += OnSignalingUpdate;
            LayoutService.Instance.LayoutChanged += LayoutChanged;
        }

        public ObservableCollection<ConversationViewModel> Conversations { get; } =
            new ObservableCollection<ConversationViewModel>();

        public ConversationViewModel SelectedConversation
        {
            get { return _selectedConversation; }
            set { SetProperty(ref _selectedConversation, value); }
        }
                
        public bool IsConversationsListVisible
        {
            get { return _isConversationsListVisible; }
            set { SetProperty(ref _isConversationsListVisible, value); }
        }

        public bool IsSeparatorVisible
        {
            get { return _isSeparatorVisible; }
            set { SetProperty(ref _isSeparatorVisible, value); }
        }
        
        private void Contact_OnCloseConversation(ConversationViewModel obj)
        {
            SelectedConversation = null;
        }

        private void LayoutChanged(LayoutType layout)
        {
            UpdateSelection();
        }

        private void OnSignalingUpdate()
        {
            var peers = SignaledPeerData.Peers;
            foreach (var peer in peers)
            {
                var contact = Conversations.SingleOrDefault(s => s.UserId == peer.UserId);
                if (contact == null)
                {
                    contact = _contactFactory();
                    contact.Name = peer.Name;
                    contact.UserId = peer.UserId;
                    contact.ProfileSource = new BitmapImage(new Uri(AvatarLink.For(peer.Avatar)));
                    contact.OnCloseConversation += Contact_OnCloseConversation;
                    Conversations.Add(contact);
                }
                contact.IsOnline = peer.IsOnline;
            }

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            IsConversationsListVisible = Conversations.Count > 0;
            IsSeparatorVisible = LayoutService.Instance.LayoutType == LayoutType.Parallel;
            if (SelectedConversation == null && LayoutService.Instance.LayoutType == LayoutType.Parallel)
            {
                SelectedConversation = Conversations.FirstOrDefault();
            }
        }
    }
}