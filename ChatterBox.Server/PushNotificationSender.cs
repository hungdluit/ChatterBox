using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Common.Logging;

namespace ChatterBox.Server
{
    public enum NotificationType
    {
        Tile = 0,
        Toast,
        Badge,
        Raw
    }

    public class PushNotificationSender
    {
        private List<Dictionary<string, string>> listOfNotificationsForSending;
        public event Action OnChannelURIExpired;

        private string _channelURI;
        private NotificationType _notificationType;

        private const string payloadKey = "payload";
        private const string channelURIKey = "channelURI";

        public PushNotificationSender() : this (null, NotificationType.Raw) { }

        public PushNotificationSender(string chanellURI) : this (chanellURI, NotificationType.Raw) { }

        public PushNotificationSender(string chanellURI, NotificationType type)
        {
            _channelURI = chanellURI;
            _notificationType = type;
            listOfNotificationsForSending = new List<Dictionary<string, string>>();
            WNSAuthentication.Instance.OnAuthenticated += OnAuthenticated;
        }

        public string ChannelURI { get { return _channelURI; } set { _channelURI = value; } }

        public void SendNotification(string payload)
        {
            SendNotification(_channelURI, payload, _notificationType);
        }

        public async void SendNotification(string channelURI, string payload, NotificationType type = NotificationType.Raw)
        {
            if (WNSAuthentication.Instance.oAuthToken.AccessToken != null && !WNSAuthentication.Instance.IsRefreshInProgress)
            {
                using (var client = new WebClient())
                {
                    SetHeaders(type, client);
                    try
                    {
                        await client.UploadStringTaskAsync(new Uri(channelURI), payload);
                    }
                    catch (WebException webException)
                    {
                        HandleError(((HttpWebResponse)webException.Response).StatusCode, channelURI, payload);
                        Debug.WriteLine(String.Format("Failed WNS authentication. Error: {0}", webException.Message));
                    }
                    catch (Exception)
                    {
                        HandleError(HttpStatusCode.Unauthorized, channelURI, payload);
                    }
                }
            }
            else
            {
                StoreNotificationForSending(channelURI, payload);
            }
        }

        private void OnAuthenticated()
        {
            while (listOfNotificationsForSending.Count > 0)
            {
                Dictionary<string, string> notification = listOfNotificationsForSending[0];
                listOfNotificationsForSending.RemoveAt(0);
                SendNotification(notification[channelURIKey], notification[payloadKey]);
            }
        }
        private void HandleError(HttpStatusCode errorCode, string channelURI, string payload)
        {
            switch (errorCode)
            {
                case HttpStatusCode.Unauthorized:
                    StoreNotificationForSending(channelURI, payload);
                    break;
                case HttpStatusCode.Gone:
                case HttpStatusCode.NotFound:
                    OnChannelURIExpired?.Invoke();
                    break;

                case HttpStatusCode.NotAcceptable:
                    break;

                default:
                    break;
            }
        }

        private void StoreNotificationForSending(string channelURI, string payload)
        {
            Dictionary<string, string> notificationDict = new Dictionary<string, string>();
            notificationDict[channelURIKey] = channelURI;
            notificationDict[payloadKey] = payload;
            listOfNotificationsForSending.Add(notificationDict);
        }

        private static void SetHeaders(NotificationType type, WebClient client)
        {
            client.Headers.Add("X-WNS-Type", GetHeaderType(type));
            client.Headers.Add("Authorization", string.Format("Bearer {0}", WNSAuthentication.Instance.oAuthToken.AccessToken));
        }


        private static string GetHeaderType(NotificationType type)
        {
            string ret = null;
            switch (type)
            {
                case NotificationType.Badge:
                    ret = "wns/badge";
                    break;
                case NotificationType.Tile:
                    ret = "wns/tile";
                    break;
                case NotificationType.Toast:
                    ret = "wns/toast";
                    break;
                case NotificationType.Raw:
                    ret = "wns/raw";
                    break;
                default:
                    ret = "wns/raw";
                    break;
            }
            return ret;
        }

    }
}
