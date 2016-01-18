using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using ChatterBox.Client.Common.Avatars;
using ChatterBox.Client.Common.Signaling.PersistedData;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public sealed class ContactsViewModel : BindableBase
    {
        private readonly Func<ConversationViewModel> _contactFactory;
        private bool _isConversationsListVisible;
        private bool _isSeparatorVisible;
        private ConversationViewModel _selectedConversation;

        public ContactsViewModel(IForegroundUpdateService foregroundUpdateService,
            Func<ConversationViewModel> contactFactory)
        {
            _contactFactory = contactFactory;
            foregroundUpdateService.OnPeerDataUpdated += OnPeerDataUpdated;
            OnPeerDataUpdated();

            LayoutService.Instance.LayoutChanged += LayoutChanged;
            ShowSettings = new DelegateCommand(() => OnShowSettings?.Invoke());
        }

        public ObservableCollection<ConversationViewModel> Conversations { get; } =
            new ObservableCollection<ConversationViewModel>();

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

        public ConversationViewModel SelectedConversation
        {
            get { return _selectedConversation; }
            set
            {
                _selectedConversation?.OnNavigatedFrom();
                SetProperty(ref _selectedConversation, value);
                _selectedConversation?.OnNavigatedTo();
            }
        }

        public DelegateCommand ShowSettings { get; set; }

        private void Contact_OnCloseConversation(ConversationViewModel obj)
        {
            SelectedConversation = null;
        }

        private void LayoutChanged(LayoutType layout)
        {
            UpdateSelection();
        }

        private void OnPeerDataUpdated()
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
                    contact.ProfileSource = new BitmapImage(new Uri(AvatarLink.EmbeddedLinkFor(peer.Avatar)));
                    contact.OnCloseConversation += Contact_OnCloseConversation;
                    contact.OnIsInCallMode += Contact_OnIsInCallMode;
                    var sortList = Conversations.ToList();
                    sortList.Add(contact);
                    sortList = sortList.OrderBy(s => s.Name).ToList();
                    Conversations.Insert(sortList.IndexOf(contact), contact);
                    contact.Initialize();
                }
                contact.IsOnline = peer.IsOnline;
            }

            UpdateSelection();
        }

        private void Contact_OnIsInCallMode(ConversationViewModel conversation)
        {
            SelectedConversation = conversation;
        }

        public event Action OnShowSettings;

        private void UpdateSelection()
        {
            IsConversationsListVisible = Conversations.Count > 0;
            IsSeparatorVisible = LayoutService.Instance.LayoutType == LayoutType.Parallel;
            if (SelectedConversation == null && LayoutService.Instance.LayoutType == LayoutType.Parallel)
            {
                SelectedConversation = Conversations.FirstOrDefault();
            }
        }

        public bool SelectConversation(string userId)
        {
            foreach (var conversation in Conversations)
            {
                if (conversation.UserId == userId)
                {
                    SelectedConversation = conversation;
                    return true;
                }
            }
            return false;
        }
    }
}