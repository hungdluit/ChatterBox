﻿using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Universal.Tasks.AppService
{
    public sealed class ForegroundAppServiceTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var triggerDetail = (AppServiceTriggerDetails) taskInstance.TriggerDetails;
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += (s, e) => _deferral?.Complete();
            AppServiceConnectionHub.Instance.ForegroundConnection = triggerDetail.AppServiceConnection;
        }
    }
}