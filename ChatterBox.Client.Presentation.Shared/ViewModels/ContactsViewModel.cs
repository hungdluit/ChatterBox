using System.Collections.ObjectModel;
using System.Linq;
using ChatterBox.Client.Presentation.Shared.Models;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Signaling.Shared;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public sealed class ContactsViewModel : BindableBase
    {
        public ObservableCollection<ContactModel> Contacts { get; } = new ObservableCollection<ContactModel>();

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
                        UserId = contact.UserId
                    };
                    Contacts.Add(toUpdate);
                }
                toUpdate.IsOnline = contact.IsOnline;
            }
        }
    }
}
