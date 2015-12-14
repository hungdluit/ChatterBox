using ChatterBox.Client.Common.Background.DeferralWrappers;
using ChatterBox.Client.Common.Notifications;
using System;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Buffer = Windows.Storage.Streams.Buffer;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class SignalingTask : IBackgroundTask
    {
        #region IBackgroundTask Members

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

                            string request = null;
                            using (var socketOperation = Hub.Instance.SignalingSocketService.SocketOperation)
                            {
                                if (socketOperation.Socket != null)
                                {
                                    var socket = socketOperation.Socket;
                                    const uint length = 65536;
                                    var readBuf = new Buffer(length);
                                    var localBuffer =
                                        await socket.InputStream.ReadAsync(readBuf, length, InputStreamOptions.Partial);
                                    var dataReader = DataReader.FromBuffer(localBuffer);
                                    request = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                                }
                            }
                            if (request != null)
                            {
                                Hub.Instance.SignalingClient.HandleRequest(request);
                            }
                            break;
                        case SocketActivityTriggerReason.KeepAliveTimerExpired:
                            Hub.Instance.SignalingClient.ClientHeartBeat();
                            break;
                        case SocketActivityTriggerReason.SocketClosed:
                            Hub.Instance.SignalingClient.ServerConnectionError();
                            //ToastNotificationService.ShowToastNotification("Disconnected.");
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Hub.Instance.SignalingClient.ServerConnectionError();
                    ToastNotificationService.ShowToastNotification(string.Format("Error in SignalingTask: {0}",
                        exception.Message));
                }
            }
        }

        #endregion
    }
}