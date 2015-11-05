using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.UI.Core;
using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Signaling.Dto;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Universal.Background.DeferralWrappers;
using ChatterBox.Client.Universal.Background.Helpers;
using ChatterBox.Client.Universal.Background.Tasks;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Standard;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Universal.Services
{
    public class HubClient : DispatcherBindableBase,
        IForegroundUpdateService,
        ISignalingSocketChannel,
        IClientChannel,
        IVoipChannel,
        IForegroundChannel
    {
        private readonly TaskHelper _taskHelper;
        private AppServiceConnection _appConnection;

        public HubClient(CoreDispatcher uiDispatcher, TaskHelper taskHelper) : base(uiDispatcher)
        {
            _taskHelper = taskHelper;
        }

        #region IClientChannel Members

        public void ClientConfirmation(Confirmation confirmation)
        {
            InvokeHubChannel<IClientChannel>(confirmation);
        }

        public void ClientHeartBeat()
        {
            InvokeHubChannel<IClientChannel>();
        }

        public void GetPeerList(Message message)
        {
            InvokeHubChannel<IClientChannel>(message);
        }

        public void Register(Registration message)
        {
            InvokeHubChannel<IClientChannel>(message);
        }

        public void Relay(RelayMessage message)
        {
            InvokeHubChannel<IClientChannel>(message);
        }

        #endregion

        #region IForegroundChannel Members

        public void OnSignaledPeerDataUpdated()
        {
            RunOnUiThread(() => OnPeerDataUpdated?.Invoke());
        }

        public void OnSignaledRegistrationStatusUpdated()
        {
            RunOnUiThread(() => OnRegistrationStatusUpdated?.Invoke());
        }

        public void OnSignaledRelayMessagesUpdated()
        {
            RunOnUiThread(() => OnRelayMessagesUpdated?.Invoke());
        }

        public void OnVoipState(VoipState state)
        {
            RunOnUiThread(() => OnVoipStateUpdate?.Invoke(state));
        }

        #endregion

        #region IForegroundUpdateService Members

        public event Action OnPeerDataUpdated;
        public event Action OnRegistrationStatusUpdated;
        public event Action OnRelayMessagesUpdated;
        public event Action<VoipState> OnVoipStateUpdate;

        #endregion

        #region ISignalingSocketChannel Members

        public ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner)
        {
            return InvokeHubChannel<ISignalingSocketChannel, ConnectionStatus>(new ConnectionOwner
            {
                OwnerId = _taskHelper.GetSignalingTask().TaskId.ToString()
            });
        }

        public ConnectionStatus GetConnectionStatus()
        {
            return InvokeHubChannel<ISignalingSocketChannel, ConnectionStatus>();
        }

        #endregion

        #region IVoipChannel Members

        public void Answer()
        {
            InvokeHubChannel<IVoipChannel>();
        }

        public void Call(OutgoingCallRequest request)
        {
            InvokeHubChannel<IVoipChannel>(request);
        }

        public VoipState GetVoipState()
        {
            return InvokeHubChannel<IVoipChannel, VoipState>();
        }

        public void Hangup()
        {
            InvokeHubChannel<IVoipChannel>();
        }

        public void OnIceCandidate(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnIncomingCall(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnOutgoingCallAccepted(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnOutgoingCallRejected(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnRemoteHangup(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnSdpAnswer(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnSdpOffer(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void Reject(IncomingCallReject reason)
        {
            InvokeHubChannel<IVoipChannel>(reason);
        }

        #endregion

        public async Task<bool> Connect()
        {
            _appConnection = new AppServiceConnection
            {
                AppServiceName = nameof(ForegroundAppServiceTask),
                PackageFamilyName = Package.Current.Id.FamilyName
            };
            _appConnection.ServiceClosed += OnServiceClosed;
            _appConnection.RequestReceived += OnRequestReceived;
            var status = await _appConnection.OpenAsync();
            return status == AppServiceConnectionStatus.Success;
        }

        private void InvokeHubChannel<TContract>(object arg = null, [CallerMemberName] string method = null)
        {
            _appConnection.InvokeChannel(typeof (TContract), arg, method);
        }

        private TResult InvokeHubChannel<TContract, TResult>(object arg = null, [CallerMemberName] string method = null)
            where TResult : class
        {
            return (TResult) _appConnection.InvokeChannel(typeof (TContract), arg, method, typeof (TResult));
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            using (new AppServiceDeferralWrapper(args.GetDeferral()))
            {
                var message = args.Request.Message.Single().Value.ToString();
                AppServiceChannelHelper.HandleRequest(args.Request, this, message);
            }
        }

        private void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
        }

        public void OnSignaledDataUpdated()
        {
            RunOnUiThread(() => OnUpdate?.Invoke());
        }

        public event Action OnUpdate;
    }
}