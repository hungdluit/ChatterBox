using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Universal.Background.DeferralWrappers;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Client.Universal.Background
{
    public sealed class Hub
    {
        private static volatile Hub _instance;
        private static readonly object SyncRoot = new object();
        private AppServiceConnection _foregroundConnection;

        private Hub()
        {
            SignalingClient = new SignalingClient(SignalingSocketService, ForegroundClient);
        }

        public ForegroundClient ForegroundClient { get; } = new ForegroundClient();

        public AppServiceConnection ForegroundConnection
        {
            get { return _foregroundConnection; }
            set
            {
                if (_foregroundConnection != null)
                {
                    _foregroundConnection.RequestReceived -= HandleForegroundRequest;
                }
                _foregroundConnection = value;
                _foregroundConnection.RequestReceived += HandleForegroundRequest;
            }
        }

        public static Hub Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (SyncRoot)
                {
                    if (_instance == null) _instance = new Hub();
                }

                return _instance;
            }
        }

        public SignalingClient SignalingClient { get; }
        public SignalingSocketService SignalingSocketService { get; } = new SignalingSocketService();

        private async void HandleForegroundRequest(
            AppServiceConnection sender,
            AppServiceRequestReceivedEventArgs args)
        {
            using (new AppServiceDeferralWrapper(args.GetDeferral()))
            {
                var message = args.Request.Message.Single().Value.ToString();
                var channel = args.Request.Message.Single().Key;

                if (channel == nameof(ISignalingSocketChannel))
                {
                    await InvokeOn(args.Request, SignalingSocketService, message);
                }
                if (channel == nameof(IClientChannel))
                {
                    await InvokeOn(args.Request, SignalingClient, message);
                }
            }
        }

        private async Task InvokeOn(AppServiceRequest request, object handler, string message)
        {
            var invoker = new ChannelInvoker(handler);
            var result = invoker.ProcessRequest(message);
            await SendResponse(request, result);
        }

        private async Task SendResponse(AppServiceRequest request, InvocationResult result)
        {
            if (result.Result == null) return;
            await request.SendResponseAsync(new ValueSet
            {
                {Guid.NewGuid().ToString(), JsonConvert.Serialize(result.Result)}
            });
        }
    }
}