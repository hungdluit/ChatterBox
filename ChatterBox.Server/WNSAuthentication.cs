using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Web;
using System.Net;
using NotificationsExtensions;

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


        private static readonly Lazy<WNSAuthentication> lazy =
        new Lazy<WNSAuthentication>(() => new WNSAuthentication());

        private WnsAccessTokenProvider tokenProvider;

        public OAuthToken oAuthToken { get; set; }

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

        public async void AuthenticateWithWNS(string sid, string secret)
        {
            var urlEncodedSid = HttpUtility.UrlEncode(sid);
            var urlEncodedSecret = HttpUtility.UrlEncode(secret);

            var body = String.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=notify.windows.com", urlEncodedSid, urlEncodedSecret);

            string response = null;
            Exception exception = null;
            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                try
                {
                    response = await client.UploadStringTaskAsync(new Uri("https://login.live.com/accesstoken.srf"), body);
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }

            if (exception == null && response != null)
                oAuthToken = GetOAuthTokenFromJson(response);
        }

        public WnsAccessTokenProvider GetAccessTokenProvider()
        {
            if (tokenProvider == null)
                tokenProvider = new WnsAccessTokenProvider(WNS_PACKAGE_SECURITY_IDENTIFIER, WNS_SECRET_KEY);

            return tokenProvider;
        }
    }
}