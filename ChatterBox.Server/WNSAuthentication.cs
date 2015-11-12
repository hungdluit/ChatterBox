using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Web;
using System.Net;


using System.Threading;

namespace ChatterBox.Server
{
    [DataContract]
    public class OAuthToken
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }
        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }

        [DataMember(Name = "expires_in")]
        public double  ExpireTime { get; set; }

    }

    public sealed class WNSAuthentication
    {
        //These values are obtained from Windows Dev Center Dashboard. THESE VALUES MUST NOT BE PUBLIC.
        private const string WNS_PACKAGE_SECURITY_IDENTIFIER = "ms-app://s-1-15-2-480391716-3273138829-3268582380-534771994-102819520-3620776998-3780754916";
        private const string WNS_SECRET_KEY = "u/w9VhqrzVZjzl4TznCsG/FddOuDHIkX";

        private const string PayloadFormat = "grant_type=client_credentials&client_id={0}&client_secret={1}&scope={2}";
        private const string UrlEncoded = "application/x-www-form-urlencoded";
        private const string AccessTokenUrl = "https://login.live.com/accesstoken.srf";
        private const string AccessScope = "notify.windows.com";

        private static readonly object _oAuthTokenLock = new object();
        private static readonly object _refreshLock = new object();

        private System.Timers.Timer timerTokenRefresh;
        private OAuthToken _oAuthToken;
        private bool _isRefreshInProgress;

        public event Action OnAuthenticated;

        private static readonly Lazy<WNSAuthentication> lazy =
        new Lazy<WNSAuthentication>(() => new WNSAuthentication());

        public OAuthToken oAuthToken
        {
            get { lock (_oAuthTokenLock) { return _oAuthToken; } }
            set { lock (_oAuthTokenLock) { _oAuthToken = value; } }
        }

        public bool IsRefreshInProgress
        {
            get { lock (_refreshLock) { return _isRefreshInProgress; } }
            set { lock (_refreshLock) { _isRefreshInProgress = value; } }
        }

        public static WNSAuthentication Instance { get { return lazy.Value; } }

        private WNSAuthentication()
        {
        }

        private OAuthToken GetOAuthTokenFromJson(string jsonString)
        {
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
            {
                var ser = new DataContractJsonSerializer(typeof(OAuthToken));
                var oAuthToken = (OAuthToken)ser.ReadObject(ms);
                return oAuthToken;
            }
        }

        public async void AuthenticateWithWNS()
        {
            IsRefreshInProgress = true;

            var urlEncodedSid = HttpUtility.UrlEncode(WNS_PACKAGE_SECURITY_IDENTIFIER);
            var urlEncodedSecret = HttpUtility.UrlEncode(WNS_SECRET_KEY);

            var body = String.Format(PayloadFormat, urlEncodedSid, urlEncodedSecret, AccessScope);

            string response = null;
            Exception exception = null;
            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", UrlEncoded);
                try
                {
                    response = await client.UploadStringTaskAsync(new Uri(AccessTokenUrl), body);
                }
                catch (Exception e)
                {
                    exception = e;
                    Debug.WriteLine(String.Format("Failed WNS authentication. Error: {0}",e.Message));
                }
            }

            if (exception == null && response != null)
            {
                oAuthToken = GetOAuthTokenFromJson(response);
                ScheduleTokenRefreshing();
            }

            IsRefreshInProgress = false;
            OnAuthenticated?.Invoke();
        }

        private void ScheduleTokenRefreshing()
        {
            if (oAuthToken != null)
            {
                timerTokenRefresh = new System.Timers.Timer((oAuthToken.ExpireTime - 600) * 1000);
                timerTokenRefresh.AutoReset = false;
                timerTokenRefresh.Enabled = true;
                timerTokenRefresh.Elapsed += RefreshToken;
            }
        }

        private void ResetTimer(System.Timers.Timer timer)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }
        private void RefreshToken(object sender, System.Timers.ElapsedEventArgs e)
        {
            ResetTimer(timerTokenRefresh);
            AuthenticateWithWNS();
        }
    }
}