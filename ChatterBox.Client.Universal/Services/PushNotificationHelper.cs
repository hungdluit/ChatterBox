using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using ChatterBox.Client.Common.Settings;

namespace ChatterBox.Client.Universal.Services
{
    public class PushNotificationHelper
    {
        public async static void RegisterPushNotificationChannel()
        {
            PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            if (channel != null)
            {
                RegistrationSettings.PushNotificationChannelURI = channel.Uri;
                Debug.WriteLine($"Push token chanell URI: {channel.Uri}");
            }
        }
    }
}
