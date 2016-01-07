namespace ChatterBox.Client.Common.Settings
{
    public sealed class WebRTCSettingsIds
    {
        public static string VideoDeviceSettings { get { return "VideoDeviceSettingsId"; } }

        public static string AudioDeviceSettings { get { return "AudioDeviceSettingsId"; } }

        public static string AudioPlayoutDeviceSettings { get { return "AudioDevicePlayoutSettingsId"; } }

        public static string VideoCodecSettings { get { return "VideoCodecSettingsId"; } }

        public static string AudioCodecSettings { get { return "AudioCodecSettingsId"; } }

        public static string PreferredVideoCaptureWidth { get { return "PreferredVideoCaptureWidth"; } }

        public static string PreferredVideoCaptureHeight { get { return "PreferredVideoCaptureHeight"; } }
        
        public static string PreferredVideoCaptureFrameRate { get { return "PreferredVideoCaptureFrameRate"; } }
    }
}
