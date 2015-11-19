using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Universal.Background.DeferralWrappers;
using ChatterBox.Client.Universal.Background.Helpers;
using ChatterBox.Client.Universal.Background.Tasks;
using ChatterBox.Client.Universal.Background.Voip;
using ChatterBox.Client.Voip;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Messages.Relay;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Universal.Background
{
    internal sealed class Hub : IHub
    {
        private static volatile Hub _instance;
        private static readonly object SyncRoot = new object();
        private AppServiceConnection _foregroundConnection;

        private Hub()
        {
            var renderResolver = new Func<IVideoRenderHelper>(() => new VideoRenderHelper());

            var voipCoordinator = new VoipCoordinator(this);
            var voipContext = new VoipContext(this, null, renderResolver, voipCoordinator);
            VoipChannel = new VoipChannel(this, null, voipCoordinator, voipContext);

            SignalingClient = new SignalingClient(SignalingSocketService, ForegroundClient, VoipChannel);
        }

        public ForegroundClient ForegroundClient { get; } = new ForegroundClient();

        public StatsManager RTCStatsManager { get; } = new StatsManager();

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

        public IBackgroundTask ForegroundTask { get; set; }

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
        public IVoipChannel VoipChannel { get; }
        public VoipTask VoipTaskInstance { get; set; }

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
                    AppServiceChannelHelper.HandleRequest(args.Request, VoipChannel, message);
                }
            }
        }

        #region IHub members

        public void Relay(RelayMessage message)
        {
            SignalingClient.Relay(message);
        }

        public void OnVoipState(VoipState voipState)
        {
            ForegroundClient.OnVoipState(voipState);
        }

        public void InitialiazeStatsManager(webrtc_winrt_api.RTCPeerConnection pc)
        {
            RTCStatsManager.Initialize(pc);
        }

        public void ToggleStatsManagerConnectionState(bool enable)
        {
            RTCStatsManager.IsStatsCollectionEnabled = enable;
        }

        public void OnUpdateFrameFormat(FrameFormat frameFormat)
        {
            ForegroundClient.OnUpdateFrameFormat(frameFormat);
        }

        public void TrackStatsManagerEvent(String name, IDictionary<string, string> props)
        {
            RTCStatsManager.TrackEvent(name, props);
        }

        public void TrackStatsManagerMetric(String name, double value)
        {
            RTCStatsManager.TrackMetric(name, value);
        }

        public void StartStatsManagerCallWatch()
        {
            RTCStatsManager.StartCallWatch();
        }

        public void StopStatsManagerCallWatch()
        {
            RTCStatsManager.StopCallWatch();
        }
        #endregion
    }
}
