using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Signaling.Dto;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Universal.Common;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Client.Universal.Services
{
    public class ForegroundAppServiceClient : DispatcherBindableBase, ISignalingUpdateService, ISignalingCommunicationChannel
    {
        private readonly TaskHelper _taskHelper;
        private AppServiceConnection _appConnection;
        public ForegroundAppServiceClient(CoreDispatcher uiDispatcher, TaskHelper taskHelper) : base(uiDispatcher)
        {
            _taskHelper = taskHelper;
        }

        public async Task<bool> Connect()
        {
            _appConnection = AppServiceConnectionFactory.NewForegroundConnection();
            _appConnection.ServiceClosed += OnServiceClosed;
            _appConnection.RequestReceived += OnRequestReceived;
            var status = await _appConnection.OpenAsync();
            return status == AppServiceConnectionStatus.Success;
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            OnUpdate?.Invoke();
        }

        private void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
        }



        public event Action OnUpdate;






        private ValueSet SendToServer<TContract>(object arg = null, [CallerMemberName] string method = null)
        {
            var channelWriteHelper = new ChannelWriteHelper(typeof(TContract));
            var message = channelWriteHelper.FormatOutput(arg, method);
            var sendMessageTask = _appConnection.SendMessageAsync(new ValueSet { { typeof(TContract).Name, message } }).AsTask();
            sendMessageTask.Wait();
            return sendMessageTask.Result.Status != AppServiceResponseStatus.Success ? null : sendMessageTask.Result.Message;
        }

        private TResult SendToServer<TContract, TResult>(object arg = null, [CallerMemberName] string method = null) where TResult : class
        {
            var resultMessage = SendToServer<TContract>(arg, method);
            if (resultMessage == null) return null;
            return (TResult)JsonConvert.Deserialize(resultMessage.Values.Single().ToString(), typeof(TResult));
        }

        public ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner)
        {
            return SendToServer<ISignalingCommunicationChannel, ConnectionStatus>(new ConnectionOwner
            {
                OwnerId = _taskHelper.GetSignalingTask().TaskId.ToString()
            });
        }

        public ConnectionStatus GetConnectionStatus()
        {
            return SendToServer<ISignalingCommunicationChannel, ConnectionStatus>();
        }

        public void Register()
        {
            SendToServer<ISignalingCommunicationChannel>();
        }
    }
}