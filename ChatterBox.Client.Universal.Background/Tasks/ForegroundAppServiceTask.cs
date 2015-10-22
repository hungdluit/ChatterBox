using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class ForegroundAppServiceTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var triggerDetail = (AppServiceTriggerDetails) taskInstance.TriggerDetails;
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += (s, e) => _deferral?.Complete();
            Hub.Instance.ForegroundConnection = triggerDetail.AppServiceConnection;
        }
    }
}