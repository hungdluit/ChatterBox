using System;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using ChatterBox.Client.Common.Notifications;
using ChatterBox.Client.Universal.Background.DeferralWrappers;
using Buffer = Windows.Storage.Streams.Buffer;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class SignalingTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            using (new BackgroundTaskDeferralWrapper(taskInstance.GetDeferral()))
            {
                try
                {
                    var details = (SocketActivityTriggerDetails)taskInstance.TriggerDetails;
                   
                    switch (details.Reason)
                    {
                        case SocketActivityTriggerReason.SocketActivity:
                            const uint length = 65536;
                            var socket = Hub.Instance.SignalingSocketService.GetSocket();
                            var readBuf = new Buffer(length);
                            var localBuffer = await socket.InputStream
                                .ReadAsync(readBuf, length, InputStreamOptions.Partial);
                            var dataReader = DataReader.FromBuffer(localBuffer);
                            var request = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                            Hub.Instance.SignalingSocketService.HandoffSocket(socket);
                            Hub.Instance.SignalingClient.HandleRequest(request);
                            break;
                        case SocketActivityTriggerReason.KeepAliveTimerExpired:
                            Hub.Instance.SignalingClient.ClientHeartBeat();
                            break;
                        case SocketActivityTriggerReason.SocketClosed:
                            ToastNotificationService.ShowToastNotification("Disconnected.");
                            break;
                    }
                }
                catch (Exception exception)
                {
                    ToastNotificationService.ShowToastNotification(string.Format("Error in SignalingTask: {0}",
                        exception.Message));
                }
            }
        }
    }
}