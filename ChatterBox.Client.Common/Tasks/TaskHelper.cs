using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;

namespace ChatterBox.Client.Common.Background
{
    public sealed class TaskHelper
    {
        public IBackgroundTaskRegistration GetTask(string name)
        {
            return BackgroundTaskRegistration.AllTasks
                .Select(s => s.Value)
                .FirstOrDefault(s => s.Name == name);
        }

        public IAsyncOperation<IBackgroundTaskRegistration> RegisterTask(string name, string entrypoint,
            IBackgroundTrigger trigger, bool removeIfRegistered)
        {
            return RegisterTaskP(
                name,
                entrypoint,
                trigger,
                removeIfRegistered).AsAsyncOperation();
        }

        private async Task<IBackgroundTaskRegistration> RegisterTaskP(string name, string entrypoint,
            IBackgroundTrigger trigger, bool removeIfRegistered)
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
    }
}