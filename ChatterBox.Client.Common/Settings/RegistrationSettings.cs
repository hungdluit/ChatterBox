using System;
using Windows.Storage;

namespace ChatterBox.Client.Common.Settings
{
    public static class RegistrationSettings
    {
        public static string Domain
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(Domain)))
                {
                    return (string) ApplicationData.Current.LocalSettings.Values[nameof(Domain)];
                }
                Domain = "chatterbox.microsoft.com";
                return Domain;
            }
            set { ApplicationData.Current.LocalSettings.Values.AddOrUpdate(nameof(Domain), value); }
        }

        public static string Name
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(Name)))
                {
                    return (string) ApplicationData.Current.LocalSettings.Values[nameof(Name)];
                }
                return null;
            }
            set { ApplicationData.Current.LocalSettings.Values.AddOrUpdate(nameof(Name), value); }
        }

        public static string UserId
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(UserId)))
                {
                    return (string) ApplicationData.Current.LocalSettings.Values[nameof(UserId)];
                }
                UserId = Guid.NewGuid().ToString();
                return UserId;
            }
            set { ApplicationData.Current.LocalSettings.Values.AddOrUpdate(nameof(UserId), value); }
        }

        public static string ChannelURI
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(ChannelURI)))
                {
                    return (string)ApplicationData.Current.LocalSettings.Values[nameof(ChannelURI)];
                }
                return null;
            }
            set { ApplicationData.Current.LocalSettings.Values.AddOrUpdate(nameof(ChannelURI), value); }
        }

        public static void Reset()
        {
            ApplicationData.Current.LocalSettings.Values.Remove(nameof(Domain));
            ApplicationData.Current.LocalSettings.Values.Remove(nameof(Name));
            ApplicationData.Current.LocalSettings.Values.Remove(nameof(UserId));
            ApplicationData.Current.LocalSettings.Values.Remove(nameof(ChannelURI));
        }
    }
}