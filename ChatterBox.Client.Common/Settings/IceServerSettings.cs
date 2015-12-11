using ChatterBox.Client.Common.Helpers;
using System.Collections.Generic;
using Windows.Storage;

namespace ChatterBox.Client.Common.Settings
{
    public sealed class IceServer
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public sealed class IceServerSettings
    {
        public static IEnumerable<IceServer> IceServers
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(IceServers)))
                {
                    var str = ApplicationData.Current.LocalSettings.Values[nameof(IceServers)];
                    return XmlSerializationHelper.FromXml<List<IceServer>>((string)str);
                }
                IceServers = GetDefaultList();
                return IceServers;
            }
            set
            {
                var list = new List<IceServer>(value);
                var str = XmlSerializationHelper.ToXml(list);
                ApplicationData.Current.LocalSettings.Values.AddOrUpdate(nameof(IceServers), str);
            }
        }

        private static List<IceServer> GetDefaultList()
        {
            return new List<IceServer>
            {
                new IceServer
                {
                    Url = "stun:stun.l.google.com:19302"
                },
                new IceServer
                {
                    Url = "stun:stun1.l.google.com:19302"
                },
                new IceServer
                {
                    Url = "stun:stun2.l.google.com:19302"
                },
                new IceServer
                {
                    Url = "stun:stun3.l.google.com:19302"
                },
                new IceServer
                {
                    Url = "stun:stun4.l.google.com:19302"
                },
                new IceServer
                {
                    Url = "turn:40.76.194.255:3478",
                    Username = "testrtc",
                    Password = "rtc123"
                }
            };
        }
    }
}
