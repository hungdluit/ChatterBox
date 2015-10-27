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
        private bool _isSettingsVisible;
        private ConversationViewModel _selectedConversation;
        private SettingsViewModel _settingsViewModel;
        private DelegateCommand _showSettings;

        public ContactsViewModel(ISignalingUpdateService signalingUpdateService,
            SettingsViewModel settingsViewModel,
            Func<ConversationViewModel> contactFactory)
        {
            _contactFactory = contactFactory;
            settingsViewModel.Close += Settings_OnClose;
            SettingsViewModel = settingsViewModel;

            signalingUpdateService.OnPeerDataUpdated += OnPeerDataUpdated;
            OnPeerDataUpdated();

            LayoutService.Instance.LayoutChanged += LayoutChanged;
            ShowSettings = new DelegateCommand(ShowSettingsExecute);
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

        public bool IsSettingsVisible
        {
            get { return _isSettingsVisible; }
            set { SetProperty(ref _isSettingsVisible, value); }
        }

        public ConversationViewModel SelectedConversation
        {
            get { return _selectedConversation; }
            set { SetProperty(ref _selectedConversation, value); }
        }

        public SettingsViewModel SettingsViewModel
        {
            get { return _settingsViewModel; }
            set { SetProperty(ref _settingsViewModel, value); }
        }

        public DelegateCommand ShowSettings
        {
            get { return _showSettings; }
            set { SetProperty(ref _showSettings, value); }
        }

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
                    contact.ProfileSource = new BitmapImage(new Uri(AvatarLink.For(peer.Avatar)));
                    contact.OnCloseConversation += Contact_OnCloseConversation;
                    var sortList = Conversations.ToList();
                    sortList.Add(contact);
                    sortList = sortList.OrderBy(s => s.Name).ToList();
                    Conversations.Insert(sortList.IndexOf(contact), contact);
                }
                contact.IsOnline = peer.IsOnline;
            }

            UpdateSelection();
        }

        private void Settings_OnClose()
        {
            IsSettingsVisible = false;
        }

        private void ShowSettingsExecute()
        {
            IsSettingsVisible = true;
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