using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Signaling.Dto;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Universal.Background.DeferralWrappers;
using ChatterBox.Client.Universal.Background.Tasks;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Standard;
using ChatterBox.Common.Communication.Serialization;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Universal.Services
{
    public class HubClient : DispatcherBindableBase,
        ISignalingUpdateService,
        ISignalingSocketChannel,
        IClientChannel,
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
            InvokeOnHub<IClientChannel>(confirmation);
        }

        public void ClientHeartBeat()
        {
            InvokeOnHub<IClientChannel>();
        }

        public void GetPeerList(Message message)
        {
            InvokeOnHub<IClientChannel>(message);
        }

        public void Register(Registration message)
        {
            InvokeOnHub<IClientChannel>(message);
        }

        public void Relay(RelayMessage message)
        {
            InvokeOnHub<IClientChannel>(message);
        }

        public void OnSignaledDataUpdated()
        {
            RunOnUiThread(() => OnUpdate?.Invoke());
        }

        public ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner)
        {
            return InvokeOnHub<ISignalingSocketChannel, ConnectionStatus>(new ConnectionOwner
            {
                OwnerId = _taskHelper.GetSignalingTask().TaskId.ToString()
            });
        }

        public ConnectionStatus GetConnectionStatus()
        {
            return InvokeOnHub<ISignalingSocketChannel, ConnectionStatus>();
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

        private ValueSet InvokeOnHub<TContract>(object arg = null, [CallerMemberName] string method = null)
        {
            var channelWriteHelper = new ChannelWriteHelper(typeof (TContract));
            var message = channelWriteHelper.FormatOutput(arg, method);
            var sendMessageTask =
                _appConnection.SendMessageAsync(new ValueSet {{typeof (TContract).Name, message}}).AsTask();
            sendMessageTask.Wait();
            return sendMessageTask.Result.Status != AppServiceResponseStatus.Success
                ? null
                : sendMessageTask.Result.Message;
        }

        private TResult InvokeOnHub<TContract, TResult>(object arg = null, [CallerMemberName] string method = null)
            where TResult : class
        {
            var resultMessage = InvokeOnHub<TContract>(arg, method);
            if (resultMessage == null) return null;
            if (!resultMessage.Values.Any()) return null;
            return (TResult) JsonConvert.Deserialize(resultMessage.Values.Single().ToString(), typeof (TResult));
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            using (new AppServiceDeferralWrapper(args.GetDeferral()))
            {
                var requestValues = args.Request.Message.ToList();
                foreach (var valuePair in requestValues)
                {
                    var invoker = new ChannelInvoker(this);
                    var result = invoker.ProcessRequest(valuePair.Value.ToString());
                    if (result.Result != null)
                    {
                        await args.Request.SendResponseAsync(new ValueSet
                        {
                            {Guid.NewGuid().ToString(), JsonConvert.Serialize(result.Result)}
                        });
                    }
                }
            }
        }

        private void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
        }
    }
}