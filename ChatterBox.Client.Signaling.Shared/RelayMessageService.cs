using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using ChatterBox.Common.Communication.Serialization;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Signaling.Shared
{
    public static class RelayMessageService
    {
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

        public static async void Add(RelayMessage message)
        {
            try
            {
                var blob = JsonConvert.Serialize(message);
                var blobFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(message.Id.ToString());
                await FileIO.AppendTextAsync(blobFile, blob);
                RelayMessageContainer.Values.Add(message.Id.ToString(), message.Id.ToString());
            }
            catch (Exception exception)
            {
            }
        }

        public static async void Delete(string messageId)
        {
            if (!RelayMessageContainer.Values.ContainsKey(messageId))
            {
                return;
            }
            RelayMessageContainer.Values.Remove(messageId);
            var blobFile = await
                        ApplicationData.Current.LocalFolder.CreateFileAsync(messageId,
                            CreationCollisionOption.OpenIfExists);
            await blobFile.DeleteAsync();
        }

        public static IAsyncOperation<RelayMessage[]> GetMessages()
        {
            return Task.Run(async () =>
            {
                var storedMessages = RelayMessageContainer.Values.Select(s => (string) s.Value).ToList();
                var messages = new List<RelayMessage>();

                foreach (var storedMessage in storedMessages)
                {
                    var blobFile = await
                        ApplicationData.Current.LocalFolder.CreateFileAsync(storedMessage,
                            CreationCollisionOption.OpenIfExists);
                    var messageBlob = await FileIO.ReadTextAsync(blobFile);
                    var message = (RelayMessage) JsonConvert.Deserialize(messageBlob, typeof (RelayMessage));
                    messages.Add(message);
                }

                foreach (var storedMessage in storedMessages)
                {
                    RelayMessageContainer.Values.Remove(storedMessage);
                }
                return messages.ToArray();
            }).AsAsyncOperation();
        }

        public static void Reset()
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(nameof(RelayMessageContainer));
        }
    }
}