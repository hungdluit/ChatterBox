using System;
using System.Linq;
using Windows.Storage;
using ChatterBox.Client.Settings;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Signaling
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
                    Payload = s.Value.Values[nameof(RelayMessage.Payload)].ToString(),
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

        public static void Reset()
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(nameof(RelayMessageContainer));
        }
    }
}