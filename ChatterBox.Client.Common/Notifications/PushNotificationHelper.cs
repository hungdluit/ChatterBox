using System;
using System.Diagnostics;
using Windows.Networking.PushNotifications;
using ChatterBox.Client.Common.Settings;

namespace ChatterBox.Client.Common.Notifications
{
    public sealed class PushNotificationHelper
    {
        public static async void RegisterPushNotificationChannel()
        {
            bool registered = false;
            try
            {
                var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                if (channel != null)
                {
                    registered = true;
                    RegistrationSettings.PushNotificationChannelURI = channel.Uri;
                    Debug.WriteLine($"Push token chanell URI: {channel.Uri}");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception during push token chanell URI reistration: {e.Message}");
            }

            if (!registered)
                Debug.WriteLine($"Push token chanell URI is NOT obtained");

        }
    }
}