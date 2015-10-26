using System;
using System.Diagnostics;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class ForegroundAppServiceTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var triggerDetail = (AppServiceTriggerDetails) taskInstance.TriggerDetails;
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += (s, e) => _deferral?.Complete();
            Hub.Instance.ForegroundConnection = triggerDetail.AppServiceConnection;

            if (Hub.Instance.WebRtcTaskTrigger == null)
            {
                string taskName = nameof(VoipTask);
                string taskEntryPoint = typeof(VoipTask).FullName;

                IBackgroundTaskRegistration2 registration = null;

                foreach (var cur in BackgroundTaskRegistration.AllTasks)
                {
                    if (cur.Value.Name == taskName)
                    {
                        registration = cur.Value as IBackgroundTaskRegistration2;
                    }
                }

                if (registration != null)
                {
                    Debug.WriteLine("Found old VoipTask.");
                    Hub.Instance.WebRtcTaskTrigger = registration.Trigger as ApplicationTrigger;
                }
                else
                {
                    // TODO: Determine if we can get rid of this code.
                    Hub.Instance.WebRtcTaskTrigger = new ApplicationTrigger();
                    var builder = new BackgroundTaskBuilder()
                    {
                        Name = taskName,
                        TaskEntryPoint = taskEntryPoint,
                    };
                    builder.SetTrigger(Hub.Instance.WebRtcTaskTrigger);
                    var taskRegistration = builder.Register();
                    Debug.WriteLine($"VoipTask registered.");
                }
            }
        }
    }
}