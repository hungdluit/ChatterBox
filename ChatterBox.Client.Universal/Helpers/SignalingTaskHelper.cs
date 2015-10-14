using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using ChatterBox.Client.Tasks.Signaling.Universal;

namespace ChatterBox.Client.Universal.Helpers
{
    public class SignalingTaskHelper
    {
        public async Task<IBackgroundTaskRegistration> GetSignalingTaskRegistration()
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            //TODO: Treat access grant
            try
            {
                var signalingTaskRegistration = BackgroundTaskRegistration.AllTasks
                    .Select(s => s.Value)
                    .FirstOrDefault(s => s.Name == typeof (SignalingTask).Name);

                if (signalingTaskRegistration != null) return signalingTaskRegistration;

                var signalingTaskBuilder = new BackgroundTaskBuilder
                {
                    Name = typeof (SignalingTask).Name,
                    TaskEntryPoint = typeof (SignalingTask).FullName
                };
                var trigger = new SocketActivityTrigger();
                signalingTaskBuilder.SetTrigger(trigger);
                return signalingTaskBuilder.Register();
            }
            catch
            {
                return null;
            }
        }
    }
}