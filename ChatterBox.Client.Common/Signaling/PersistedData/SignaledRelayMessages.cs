using System;
using System.Linq;
using Windows.Storage;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Common.Signaling.PersistedData
{
    public static class SignaledRelayMessages
    {
        public static RelayMessage[] Messages
        {
            get
            {
                return RelayMessageContainer.Containers.Select(s => new RelayMessage
                {
                    Id = s.Value.Values[nameof(RelayMessage.Id)].ToString(),
                    SentDateTimeUtc = (DateTimeOffset) s.Value.Values[nameof(RelayMessage.SentDateTimeUtc)],
                    FromUserId = s.Value.Values[nameof(RelayMessage.FromUserId)].ToString(),
                    ToUserId = s.Value.Values[nameof(RelayMessage.ToUserId)].ToString(),
                    Payload = s.Value.Values[nameof(RelayMessage.Payload)]?.ToString(),
                    Tag = s.Value.Values[nameof(RelayMessage.Tag)].ToString()
                }).ToArray();
            }
        }

        private static ApplicationDataContainer RelayMessageContainer
        {
            get
            {
                if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(nameof(RelayMessageContainer)))
                {
                    ApplicationData.Current.LocalSettings.CreateContainer(nameof(RelayMessageContainer),
                        ApplicationDataCreateDisposition.Always);
                }
                return ApplicationData.Current.LocalSettings.Containers[nameof(RelayMessageContainer)];
            }
        }

        public static void Add(RelayMessage message)
        {
            var messageContainer = RelayMessageContainer.CreateContainer(message.Id,
                ApplicationDataCreateDisposition.Always);
            messageContainer.Values.AddOrUpdate(nameof(RelayMessage.Id), message.Id);
            messageContainer.Values.AddOrUpdate(nameof(RelayMessage.SentDateTimeUtc), message.SentDateTimeUtc);
            messageContainer.Values.AddOrUpdate(nameof(RelayMessage.FromUserId), message.FromUserId);
            messageContainer.Values.AddOrUpdate(nameof(RelayMessage.ToUserId), message.ToUserId);
            messageContainer.Values.AddOrUpdate(nameof(RelayMessage.Payload), message.Payload);
            messageContainer.Values.AddOrUpdate(nameof(RelayMessage.Tag), message.Tag);
        }

        public static void Delete(string messageId)
        {
            if (!RelayMessageContainer.Containers.ContainsKey(messageId))
            {
                return;
            }
            RelayMessageContainer.DeleteContainer(messageId);
        }

        public static bool IsMessageReceived(string messageId)
        {
            return RelayMessageContainer.Containers.ContainsKey(messageId);
        }

        public static void Reset()
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(nameof(RelayMessageContainer));
        }

        private static ApplicationDataContainer ReceivedPushNotificationsContainer
        {
            get
            {
                if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(nameof(ReceivedPushNotificationsContainer)))
                {
                    ApplicationData.Current.LocalSettings.CreateContainer(nameof(ReceivedPushNotificationsContainer),
                        ApplicationDataCreateDisposition.Always);
                }
                return ApplicationData.Current.LocalSettings.Containers[nameof(ReceivedPushNotificationsContainer)];
            }
        }

        public static void AddPushNotificationMessageID(string messageID)
        {
            if (messageID != null)
            {
                ReceivedPushNotificationsContainer.CreateContainer(messageID, ApplicationDataCreateDisposition.Always);
            }
        }

        public static Boolean IsPushNotificationReceived(string messageID)
        {
            Boolean ret = false;

            if (messageID != null)
            {
                ret = ReceivedPushNotificationsContainer.Containers.ContainsKey(messageID);
                if (ret)
                    ReceivedPushNotificationsContainer.DeleteContainer(messageID);
            }
            return ret;
        }
    }
}