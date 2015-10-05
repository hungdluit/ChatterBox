using System.Linq;
using Windows.Storage;
using ChatterBox.Client.Settings;

namespace ChatterBox.Client.Signaling.Shared
{
    public static class ContactService
    {
        private static ApplicationDataContainer ContactsContainer
        {
            get
            {
                if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(nameof(ContactsContainer)))
                {
                    ApplicationData.Current.LocalSettings.CreateContainer(nameof(ContactsContainer), ApplicationDataCreateDisposition.Always);
                }
                return ApplicationData.Current.LocalSettings.Containers[nameof(ContactsContainer)];
            }
        }

        public static Contact[] Peers
        {
            get
            {
                return ContactsContainer.Containers.Select(s => new Contact
                {
                    Name = s.Value.Values[nameof(Contact.Name)].ToString(),
                    IsOnline = (bool)s.Value.Values[nameof(Contact.IsOnline)],
                    UserId = s.Key
                }).ToArray();
            }
        }

        public static void AddOrUpdate(Contact contact)
        {
            var exists = ContactsContainer.Containers.Any(s => s.Key == contact.UserId);
            var contactContainer = exists
                ? ContactsContainer.Containers[contact.UserId]
                : ContactsContainer.CreateContainer(contact.UserId, ApplicationDataCreateDisposition.Always);

            contactContainer.Values.AddOrUpdate(nameof(Contact.Name), contact.Name);
            contactContainer.Values.AddOrUpdate(nameof(Contact.IsOnline), contact.IsOnline);
        }

        public static void Reset()
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(nameof(ContactsContainer));
        }
    }
}
