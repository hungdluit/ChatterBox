using System;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using ChatterBox.Client.Common.Avatars;
using ChatterBox.Client.Common.Notifications;
using ChatterBox.Client.Common.Signaling.PersistedData;
using ChatterBox.Client.Common.Background.DeferralWrappers;
using ChatterBox.Common.Communication.Serialization;
using ChatterBox.Common.Communication.Messages.Relay;

namespace ChatterBox.Client.Common.Background
{
    public sealed class PushNotificationTask : IBackgroundTask
    {
        private const int elapsedCallTime = 60;

        #region IBackgroundTask Members

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            using (new BackgroundTaskDeferralWrapper(taskInstance.GetDeferral()))
            {
                try
                {
                    var rawNotification = (RawNotification) taskInstance.TriggerDetails;
                    var rawContent = rawNotification.Content;

                    var serializedParameter =
                        rawContent.Substring(rawContent.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase) + 1);
                    var type = typeof (RelayMessage);
                    var message = (RelayMessage) JsonConvert.Deserialize(serializedParameter, type);

                    if (message != null)
                    {
                        var showNotification = false;
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
                        {
                            ToastNotificationService.ShowInstantMessageNotification(message.FromName,
                                message.FromUserId, AvatarLink.For(message.FromAvatar), message.Payload);

                            SignaledRelayMessages.AddPushNotificationMessageID(message.Id);
                            
                            SignaledRelayMessages.Add(message);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        #endregion
    }
}