using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using ChatterBox.Client.Tasks.Signaling.Win8dot1;

namespace ChatterBox.Client.Win8dot1
{
    public class SignalingTaskHelper
    {
        private const int keepAliveInterval = 30;
        private StreamSocket _socket;
        public ControlChannelTrigger ControlChannelTrigger { get; private set; }

        public async Task<IBackgroundTaskRegistration> RegisterTask()
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            //TODO: Treat access grant
            try
            {
                _socket = new StreamSocket();

                ControlChannelTrigger = new ControlChannelTrigger("channelid", keepAliveInterval);

                var signalingTaskRegistration =
                    BackgroundTaskRegistration.AllTasks.Select(s => s.Value).FirstOrDefault(
                        s => s.Name == typeof (SignalingTask).Name);

                if (signalingTaskRegistration != null) return signalingTaskRegistration;

                var backgTask = new BackgroundTaskBuilder();
                backgTask.Name = typeof (SignalingTask).Name;
                backgTask.TaskEntryPoint = typeof (SignalingTask).FullName;
                backgTask.SetTrigger(ControlChannelTrigger.PushNotificationTrigger);

                var keepAlive = new BackgroundTaskBuilder();
                keepAlive.Name = typeof (KeepAliveTask).Name;
                keepAlive.TaskEntryPoint = typeof (KeepAliveTask).FullName;
                keepAlive.SetTrigger(ControlChannelTrigger.KeepAliveTrigger);

                ControlChannelTrigger.UsingTransport(_socket);

                return backgTask.Register();
            }
            catch (UnauthorizedAccessException ex)
            {
                // TODO: alert user to add the app to the lock screen (Settings -> Personalization -> Lock Screen)
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}