using ChatterBox.Client.Common.Background.DeferralWrappers;
using ChatterBox.Client.Common.Notifications;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

                                    var readOp = socket.InputStream.ReadAsync(readBuf, length, InputStreamOptions.Partial);
                                    // This delay is to limit how long we wait for reading.
                                    // StreamSocket has no ability to peek to see if there's any
                                    // data waiting to be read.  So we have to limit it this way.
                                    for (int i = 0; i < 25 && readOp.Status == Windows.Foundation.AsyncStatus.Started; ++i)
                                    {
                                        await Task.Delay(10);
                                    }
                                    await socket.CancelIOAsync();

                                    try
                                    {
                                        var localBuffer = await readOp;
                                        var dataReader = DataReader.FromBuffer(localBuffer);
                                        request = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine($"ReadAsync exception probably due to timeout: {ex.Message}");
                                    }
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