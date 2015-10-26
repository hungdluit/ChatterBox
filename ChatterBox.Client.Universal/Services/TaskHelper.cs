using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using ChatterBox.Client.Universal.Background.Tasks;

namespace ChatterBox.Client.Universal.Services
{
    public class TaskHelper
    {
        public IBackgroundTaskRegistration GetTask(string name)
        {
            return BackgroundTaskRegistration.AllTasks
                .Select(s => s.Value)
                .FirstOrDefault(s => s.Name == name);
        }

        public async Task<IBackgroundTaskRegistration> RegisterTask(string name, string entrypoint, IBackgroundTrigger trigger, bool removeIfRegistered)
        {
            var taskRegistration = GetTask(name);
            if (taskRegistration != null)
            {
                if (removeIfRegistered)
                {
                    taskRegistration.Unregister(true);
                }
                else
                {
                    return taskRegistration;
                }
            }
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            var taskBuilder = new BackgroundTaskBuilder
            {
                Name = name,
                TaskEntryPoint = entrypoint
            };
            taskBuilder.SetTrigger(trigger);
            return taskBuilder.Register();
        }

        public IBackgroundTaskRegistration GetSignalingTask()
        {
            return GetTask(nameof(SignalingTask));
        }

        public Task<IBackgroundTaskRegistration> RegisterSignalingTask(bool removeIfRegistered)
        {
            return RegisterTask(
                nameof(SignalingTask),
                typeof(SignalingTask).FullName,
                new SocketActivityTrigger(),
                removeIfRegistered);
        }

        public Task<IBackgroundTaskRegistration> RegisterVoipTask(bool removeIfRegistered)
        {
            return RegisterTask(
                nameof(VoipTask),
                typeof(VoipTask).FullName,
                new ApplicationTrigger(),
                removeIfRegistered);
        }
    }
}