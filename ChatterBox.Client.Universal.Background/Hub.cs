using System.Linq;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Universal.Background.DeferralWrappers;
using ChatterBox.Client.Universal.Background.Helpers;
using ChatterBox.Client.Universal.Background.Tasks;
using ChatterBox.Common.Communication.Contracts;

namespace ChatterBox.Client.Universal.Background
{
    public sealed class Hub
    {
        private static volatile Hub _instance;
        private static readonly object SyncRoot = new object();
        private AppServiceConnection _foregroundConnection;

        private Hub()
        {
            //TODO: Put in handler for the voip channel instead of null
            SignalingClient = new SignalingClient(SignalingSocketService, ForegroundClient, VoipChannel);
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
        public IVoipChannel VoipChannel { get; } = new VoipChannel();
        public VoipTask VoipTaskInstance { get; set; }
        public ApplicationTrigger WebRtcTaskTrigger { get; set; }

        private void HandleForegroundRequest(
            AppServiceConnection sender,
            AppServiceRequestReceivedEventArgs args)
        {
            using (new AppServiceDeferralWrapper(args.GetDeferral()))
            {
                var channel = args.Request.Message.Single().Key;
                var message = args.Request.Message.Single().Value.ToString();

                if (channel == nameof(ISignalingSocketChannel))
                {
                    AppServiceChannelHelper.HandleRequest(args.Request, SignalingSocketService, message);
                }
                if (channel == nameof(IClientChannel))
                {
                    AppServiceChannelHelper.HandleRequest(args.Request, SignalingClient, message);
                }

                if (channel == nameof(IVoipChannel))
                {
                    //Todo: Change the handler from null to actual handler
                    AppServiceChannelHelper.HandleRequest(args.Request, VoipChannel, message);
                }
            }
        }
    }
}