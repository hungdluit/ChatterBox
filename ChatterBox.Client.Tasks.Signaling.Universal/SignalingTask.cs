using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using Windows.UI.Notifications;
using ChatterBox.Client.Signaling;

namespace ChatterBox.Client.Tasks.Signaling.Universal
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

                    var signalingProxy = new SignalingClient(new SignalingSocketService(taskInstance.InstanceId));

                    switch (details.Reason)
                    {
                        case SocketActivityTriggerReason.SocketActivity:
                            signalingProxy.Read();
                            break;
                        case SocketActivityTriggerReason.KeepAliveTimerExpired:
                            signalingProxy.ClientHeartBeat();
                            break;
                        case SocketActivityTriggerReason.SocketClosed:
                            ToastNotificationService.ShowToastNotification("Disconnected");
                            break;
                    }
                }
                catch (Exception exception)
                {
                    ToastNotificationService.ShowToastNotification(string.Format("Error in SignalingTask: {0}", exception.Message));
                }
            }
        }
    }

    public sealed class ToastNotificationService
    {
        public static void ShowToastNotification(string message)
        {
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes.First().AppendChild(toastXml.CreateTextNode(message));
            toastNotifier.Show(new ToastNotification(toastXml));
        }
    }
}
