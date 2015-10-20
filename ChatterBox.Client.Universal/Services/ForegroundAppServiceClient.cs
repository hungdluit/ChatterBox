using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using ChatterBox.Client.Universal.Common;

namespace ChatterBox.Client.Universal.Services
{
    public class ForegroundAppServiceClient
    {
        private AppServiceConnection _appConnection;

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
            OnSignlingDataUpdated?.Invoke();
        }

        private void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
        }

        public event Action OnSignlingDataUpdated;

        public async Task<ValueSet> SendMessageAsync(ValueSet message)
        {
            var response = await _appConnection.SendMessageAsync(message);
            return response.Status == AppServiceResponseStatus.Success ? response.Message : null;
        }
    }
}