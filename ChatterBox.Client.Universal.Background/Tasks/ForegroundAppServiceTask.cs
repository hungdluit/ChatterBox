using System;
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
                Hub.Instance.WebRtcTaskTrigger = new ApplicationTrigger();

                //await BackgroundExecutionManager.RequestAccessAsync();

                var builder = new BackgroundTaskBuilder()
                {
                    Name = "WebRTC",
                    TaskEntryPoint = "ChatterBox.Client.Universal.Background.Tasks.VoipTask",
                };
                builder.SetTrigger(Hub.Instance.WebRtcTaskTrigger);
                BackgroundTaskRegistration task = builder.Register();
            }
        }
    }
}