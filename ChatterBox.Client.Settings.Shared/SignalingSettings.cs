using Windows.Storage;

namespace ChatterBox.Client.Settings.Shared
{
    public static class SignalingSettings
    {
        public static string SignalingServerHost
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(SignalingServerHost)))
                {
                    return (string)ApplicationData.Current.LocalSettings.Values[nameof(SignalingServerHost)];
                }
                SignalingServerHost = "localhost";
                return SignalingServerHost;
            }
            set { ApplicationData.Current.LocalSettings.Values.Add(nameof(SignalingServerHost), value); }
        }


        public static string SignalingServerPort
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(SignalingServerPort)))
                {
                    return (string)ApplicationData.Current.LocalSettings.Values[nameof(SignalingServerPort)];
                }
                SignalingServerPort = "50000";
                return SignalingServerPort;
            }
            set { ApplicationData.Current.LocalSettings.Values.Add(nameof(SignalingServerPort), value); }
        }
    }
}
