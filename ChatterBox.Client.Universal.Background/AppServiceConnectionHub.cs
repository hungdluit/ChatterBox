using System;
using System.Linq;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Signaling.Dto;
using ChatterBox.Client.Universal.Common.DeferralWrappers;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Client.Universal.Background
{
    public sealed class AppServiceConnectionHub : ISignalingCommunicationChannel
    {
        private static volatile AppServiceConnectionHub _instance;
        private static readonly object SyncRoot = new object();
        private AppServiceConnection _foregroundConnection;
        private AppServiceConnection _signalingConnection;
        private readonly SignalingSocketService _signalingSocketService;

        private AppServiceConnectionHub()
        {
            _signalingSocketService = new SignalingSocketService();
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

        private async void ForegroundConnectionOnRequestReceived(AppServiceConnection sender,
            AppServiceRequestReceivedEventArgs args)
        {
            using (new AppServiceDeferralWrapper(args.GetDeferral()))
            {
                var requestValues = args.Request.Message.ToList();
                foreach (var valuePair in requestValues)
                {
                    if (valuePair.Key == nameof(ISignalingCommunicationChannel))
                    {
                        var invoker = new ChannelInvoker(this);
                        var result = invoker.ProcessRequest(valuePair.Value.ToString());
                        if (result.Result != null)
                        {
                            await
                                args.Request.SendResponseAsync(new ValueSet
                                {
                                    {Guid.NewGuid().ToString(), JsonConvert.Serialize(result.Result)}
                                });
                        }
                    }


                }
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

        public ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner)
        {
            _signalingSocketService.SignalingTaskId = Guid.Parse(connectionOwner.OwnerId);
            _signalingSocketService.Connect();
            return null;
        }

        public ConnectionStatus GetConnectionStatus()
        {
            return new ConnectionStatus
            {
                IsConnected = _signalingSocketService.GetSocket() != null
            };
        }

        public void Register()
        {
            throw new NotImplementedException();
        }
    }
}