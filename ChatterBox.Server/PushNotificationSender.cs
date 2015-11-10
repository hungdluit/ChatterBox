using System;
using System.Text.RegularExpressions;
using Common.Logging;
using NotificationsExtensions;
using NotificationsExtensions.RawContent;

namespace ChatterBox.Server
{
    public class PushNotificationSender
    {
        public static void SendNotification(string chanellURI, string message)
        {
            if (Regex.Match(chanellURI, "^https://db5.notify.windows.com").Success)
            {
                var notificationContent = RawContentFactory.CreateRaw();
                notificationContent.Content = message;
                var result = notificationContent.Send(new Uri(chanellURI),
                    WNSAuthentication.Instance.GetAccessTokenProvider());
                if (result.Exception != null)
                {
                    LogManager.GetLogger(nameof(PushNotificationSender))
                        .Warn($"Exception occured when sending a push notification. Error: {result.Exception.Message}");
                }
            }
        }
    }
}