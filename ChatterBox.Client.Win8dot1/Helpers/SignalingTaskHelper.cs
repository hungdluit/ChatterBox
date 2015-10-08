using ChatterBox.Client.Tasks.Signaling.Win8dot1;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;

namespace ChatterBox.Client.Win8dot1
{
    public class SignalingTaskHelper
    {
        private const int keepAliveInterval = 30;
        private ControlChannelTrigger _channelTrigger;
        private StreamSocket _socket;

        public async Task<IBackgroundTaskRegistration> RegisterTask()
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            //TODO: Treat access grant
            try
            {
                _socket = new StreamSocket();

                _channelTrigger = new ControlChannelTrigger("channelid", keepAliveInterval);

                var signalingTaskRegistration =
                    BackgroundTaskRegistration.AllTasks.Select(s => s.Value).FirstOrDefault(
                        s => s.Name == typeof(SignalingTask).Name);

                if (signalingTaskRegistration != null) return signalingTaskRegistration;

                var backgTask = new BackgroundTaskBuilder();
                backgTask.Name = typeof(SignalingTask).Name;
                backgTask.TaskEntryPoint = typeof(SignalingTask).FullName;
                backgTask.SetTrigger(_channelTrigger.PushNotificationTrigger);

                var keepAlive = new BackgroundTaskBuilder();
                keepAlive.Name = typeof(KeepAliveTask).Name;
                keepAlive.TaskEntryPoint = typeof(KeepAliveTask).FullName;
                keepAlive.SetTrigger(_channelTrigger.KeepAliveTrigger);

                _channelTrigger.UsingTransport(_socket);
                
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

        public ControlChannelTrigger ControlChannelTrigger
        {
            get { return _channelTrigger; }
        }
    }
}
