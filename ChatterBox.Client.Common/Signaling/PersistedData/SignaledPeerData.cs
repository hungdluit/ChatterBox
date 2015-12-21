using ChatterBox.Client.Common.Settings;
using ChatterBox.Common.Communication.Messages.Peers;
using System.Linq;
using Windows.Storage;

namespace ChatterBox.Client.Common.Signaling.PersistedData
{
    public static class SignaledPeerData
    {
        private static ApplicationDataContainer PeerDataContainer
        {
            get
            {
                if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(nameof(PeerDataContainer)))
                {
                    ApplicationData.Current.LocalSettings.CreateContainer(nameof(PeerDataContainer),
                        ApplicationDataCreateDisposition.Always);
                }
                return ApplicationData.Current.LocalSettings.Containers[nameof(PeerDataContainer)];
            }
        }

        public static PeerData[] Peers
        {
            get
            {
                return PeerDataContainer.Containers.Select(s => new PeerData
                {
                    Name = s.Value.Values[nameof(PeerData.Name)]?.ToString(),
                    IsOnline = (bool) s.Value.Values[nameof(PeerData.IsOnline)],
                    UserId = s.Key,
                    Avatar = (int) (s.Value.Values[nameof(PeerData.Avatar)])
                }).ToArray();
            }
        }

        public static void AddOrUpdate(PeerData contact)
        {
            var exists = PeerDataContainer.Containers.Any(s => s.Key == contact.UserId);
            var contactContainer = exists
                ? PeerDataContainer.Containers[contact.UserId]
                : PeerDataContainer.CreateContainer(contact.UserId, ApplicationDataCreateDisposition.Always);

            contactContainer.Values.AddOrUpdate(nameof(PeerData.Name), contact.Name);
            contactContainer.Values.AddOrUpdate(nameof(PeerData.Avatar), contact.Avatar);
            contactContainer.Values.AddOrUpdate(nameof(PeerData.IsOnline), contact.IsOnline);
        }

        public static void Reset()
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(nameof(PeerDataContainer));
        }
    }
}