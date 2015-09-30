using System.Collections.ObjectModel;
using System.Linq;
using ChatterBox.Client.Presentation.Shared.Models;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Signaling.Shared;
using Windows.UI.Xaml.Media.Imaging;
using System;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public sealed class ContactsViewModel : BindableBase
    {
        private ContactModel _selectedContact;

        public ObservableCollection<ContactModel> Contacts { get; } = new ObservableCollection<ContactModel>();

        public ContactModel SelectedContactModel
        {
            get { return _selectedContact; }
            set
            {
                SetProperty(ref _selectedContact, value);
                if (value != null)
                {
                    ContactSelected?.Invoke(value);
                }
            }
        }

        public void Update(Contact[] contacts)
        {
            foreach (var contact in contacts)
            {
                var toUpdate = Contacts.SingleOrDefault(s => s.UserId == contact.UserId);
                if (toUpdate == null)
                {
                    toUpdate = new ContactModel
                    {
                        Name = contact.Name,
                        UserId = contact.UserId,
                        ProfileSource = new BitmapImage(new Uri("ms-appx:///Assets/profile_2.png"))
                    };
                    Contacts.Add(toUpdate);
                }
                toUpdate.IsOnline = contact.IsOnline;
            }
        }

        public event Action<ContactModel> ContactSelected;
    }
}
