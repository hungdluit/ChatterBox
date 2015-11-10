using System;
using System.Diagnostics;
using Windows.Networking.PushNotifications;
using ChatterBox.Client.Common.Settings;

namespace ChatterBox.Client.Universal.Services
{
    public class PushNotificationHelper
    {
        public static async void RegisterPushNotificationChannel()
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            if (channel != null)
            {
                RegistrationSettings.PushNotificationChannelURI = channel.Uri;
                Debug.WriteLine($"Push token chanell URI: {channel.Uri}");
            }
        }
    }
}