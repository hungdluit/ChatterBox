using System;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using ChatterBox.Client.Common.Notifications;
using ChatterBox.Client.Universal.Background.DeferralWrappers;
using System.Reflection;
using ChatterBox.Common.Communication.Serialization;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using ChatterBox.Client.Common.Avatars;
using ChatterBox.Client.Common.Signaling.PersistedData;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class PushNotificationTask : IBackgroundTask
    {
        private const int elapsedCallTime = 60;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            using (new BackgroundTaskDeferralWrapper(taskInstance.GetDeferral()))
            {
                try
                {
                    RawNotification rawNotification = (RawNotification)taskInstance.TriggerDetails;
                    string rawContent = rawNotification.Content;

                   var serializedParameter =
                        rawContent.Substring(rawContent.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase) + 1);
                    Type type = typeof(RelayMessage);
                    RelayMessage message = (RelayMessage) JsonConvert.Deserialize(serializedParameter,type);

                    if (message != null)
                    {
                        bool showNotification = false;
                        if (message.Tag == RelayMessageTags.SdpAnswer)
                        {
                            var timespan = DateTimeOffset.UtcNow - message.SentDateTimeUtc;
                            showNotification = timespan.TotalSeconds < elapsedCallTime;
                        }
                        else if (message.Tag == RelayMessageTags.InstantMessage)
                        {
                            showNotification = !SignaledRelayMessages.IsMessageReceived(message.Id);
                        }

                         if (showNotification)
                            ToastNotificationService.ShowInstantMessageNotification(message.FromName, AvatarLink.For(message.FromAvatar), message.Payload);
                    }
                }
                catch (Exception exception)
                {
                    return;
                }
            }
        }
    }
}