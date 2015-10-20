using System;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using ChatterBox.Client.Signaling.Shared;
using ChatterBox.Client.Universal.Common;

namespace ChatterBox.Client.Universal.Tasks.Signaling
{
    public sealed class AppServiceSignaledDataUpdateNotifier : ISignaledDataUpdateNotifier
    {
        public void RaiseSignaledDataUpdated()
        {
            var appConnection = AppServiceConnectionFactory.NewSignalingConnection();
            var connectTask = appConnection.OpenAsync().AsTask();
            connectTask.Wait();
            var status = connectTask.Result;
            if (status != AppServiceConnectionStatus.Success) return;
            var sendMessageTask = appConnection.SendMessageAsync(new ValueSet()).AsTask();
            sendMessageTask.Wait();
        }
    }
}