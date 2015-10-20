using System;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using ChatterBox.Client.Universal.Common.DeferralWrappers;

namespace ChatterBox.Client.Universal.Tasks.AppService
{
    public sealed class AppServiceConnectionHub
    {
        private static volatile AppServiceConnectionHub _instance;
        private static readonly object SyncRoot = new object();
        private AppServiceConnection _foregroundConnection;
        private AppServiceConnection _signalingConnection;

        private AppServiceConnectionHub()
        {
        }

        public AppServiceConnection ForegroundConnection
        {
            get { return _foregroundConnection; }
            set
            {
                if (_foregroundConnection != null)
                {
                    _foregroundConnection.RequestReceived -= ForegroundConnectionOnRequestReceived;
                }
                _foregroundConnection = value;
                _foregroundConnection.RequestReceived += ForegroundConnectionOnRequestReceived;
            }
        }

        public static AppServiceConnectionHub Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (SyncRoot)
                {
                    if (_instance == null) _instance = new AppServiceConnectionHub();
                }

                return _instance;
            }
        }

        public AppServiceConnection SignalingConnection
        {
            get { return _signalingConnection; }
            set
            {
                if (_signalingConnection != null)
                {
                    _signalingConnection.RequestReceived -= SignalingConnectionOnRequestReceived;
                }
                _signalingConnection = value;
                _signalingConnection.RequestReceived += SignalingConnectionOnRequestReceived;
            }
        }

        private void ForegroundConnectionOnRequestReceived(AppServiceConnection sender,
            AppServiceRequestReceivedEventArgs args)
        {
            using (new AppServiceDeferralWrapper(args.GetDeferral()))
            {
                var response = new ValueSet();
                var request = args.Request;
                var message = request.Message;
            }
        }

        private async void SignalingConnectionOnRequestReceived(AppServiceConnection sender,
            AppServiceRequestReceivedEventArgs args)
        {
            using (new AppServiceDeferralWrapper(args.GetDeferral()))
            {
                if (ForegroundConnection == null) return;
                await ForegroundConnection.SendMessageAsync(args.Request.Message);
            }
        }
    }
}