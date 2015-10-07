using Windows.Storage;
using ChatterBox.Client.Settings;

namespace ChatterBox.Client.Signaling.Shared
{
    public static class SignalingStatus
    {
        public static bool IsRegistered
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(IsRegistered)))
                {
                    return (bool) ApplicationData.Current.LocalSettings.Values[nameof(IsRegistered)];
                }
                return false;
            }
            set { ApplicationData.Current.LocalSettings.Values.AddOrUpdate(nameof(IsRegistered), value); }
        }

        private static ApplicationDataContainer SignalingStatusContainer
        {
            get
            {
                if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(nameof(SignalingStatusContainer)))
                {
                    ApplicationData.Current.LocalSettings.CreateContainer(nameof(SignalingStatusContainer),
                        ApplicationDataCreateDisposition.Always);
                }
                return ApplicationData.Current.LocalSettings.Containers[nameof(SignalingStatusContainer)];
            }
        }

        public static void Reset()
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(nameof(SignalingStatusContainer));
        }
    }
}