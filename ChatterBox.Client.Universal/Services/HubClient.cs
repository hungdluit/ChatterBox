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
        ISignalingUpdateService,
        ISignalingSocketChannel,
        IClientChannel,
        IVoipChannel,
        IForegroundCommunicationChannel
    {
        private readonly TaskHelper _taskHelper;
        private AppServiceConnection _appConnection;

        public HubClient(CoreDispatcher uiDispatcher, TaskHelper taskHelper) : base(uiDispatcher)
        {
            _taskHelper = taskHelper;
        }

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

        public void OnSignaledDataUpdated()
        {
            RunOnUiThread(() => OnUpdate?.Invoke());
        }

        public void OnVoipState(VoipState state)
        {
            
        }

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

        public event Action OnUpdate;

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

        public void HandleSdpAnswer(SdpAnswer sdpAnswer)
        {
            InvokeHubChannel<IVoipChannel>(sdpAnswer);
        }
    }
}