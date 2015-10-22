using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using ChatterBox.Client.Universal.Background.Tasks;

namespace ChatterBox.Client.Universal.Services
{
    public class TaskHelper
    {
        public IBackgroundTaskRegistration GetSignalingTask()
        {
            return BackgroundTaskRegistration.AllTasks
                .Select(s => s.Value)
                .FirstOrDefault(s => s.Name == nameof(SignalingTask));
        }

        public async Task<IBackgroundTaskRegistration> RegisterSignalingTask(bool removeIfRegistered)
        {
            var signalingTaskRegistration = GetSignalingTask();
            if (signalingTaskRegistration != null)
            {
                if (removeIfRegistered)
                {
                    signalingTaskRegistration.Unregister(true);
                }
                else
                {
                    return signalingTaskRegistration;
                }
            }
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            var signalingTaskBuilder = new BackgroundTaskBuilder
            {
                Name = nameof(SignalingTask),
                TaskEntryPoint = typeof (SignalingTask).FullName
            };
            var trigger = new SocketActivityTrigger();
            signalingTaskBuilder.SetTrigger(trigger);
            return signalingTaskBuilder.Register();
        }
    }
}