using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class SignalingAppServiceTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var triggerDetail = (AppServiceTriggerDetails) taskInstance.TriggerDetails;
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += (s, e) => _deferral?.Complete();
            AppServiceConnectionHub.Instance.SignalingConnection = triggerDetail.AppServiceConnection;
        }
    }
}