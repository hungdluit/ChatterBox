using System;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using ChatterBox.Client.Common.Notifications;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Universal.Common.DeferralWrappers;
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
                    var details = taskInstance.TriggerDetails as SocketActivityTriggerDetails;
                    if (details == null)
                    {
                        return;
                    }

                    var signalingSocketService = new SignalingSocketService
                    {
                        SignalingTaskId = taskInstance.InstanceId
                    };
                    var client = new SignalingClient(signalingSocketService, null);

                    switch (details.Reason)
                    {
                        case SocketActivityTriggerReason.SocketActivity:

                            const uint length = 65536;
                            var socket = signalingSocketService.GetSocket();
                            var readBuf = new Buffer(length);
                            var localBuffer =
                                await socket.InputStream.ReadAsync(readBuf, length, InputStreamOptions.Partial);
                            var dataReader = DataReader.FromBuffer(localBuffer);
                            var request = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                            signalingSocketService.HandoffSocket(socket);

                            client.HandleRequest(request);
                            break;
                        case SocketActivityTriggerReason.KeepAliveTimerExpired:
                            client.ClientHeartBeat();
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