using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using ChatterBox.Client.Universal.Tasks.Signaling;

namespace ChatterBox.Client.Universal.Helpers
{
    public class SignalingTaskHelper
    {
        public async Task<IBackgroundTaskRegistration> GetSignalingTaskRegistration(bool registerAgain)
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            //TODO: Treat access grant
            try
            {
                var signalingTaskRegistration = BackgroundTaskRegistration.AllTasks
                    .Select(s => s.Value)
                    .FirstOrDefault(s => s.Name == typeof (SignalingTask).Name);

                if (signalingTaskRegistration != null)
                {
                    if (!registerAgain)
                    {
                        return signalingTaskRegistration;
                    }
                    signalingTaskRegistration.Unregister(true);
                    signalingTaskRegistration = null;
                }

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