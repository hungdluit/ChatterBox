using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class ForegroundAppServiceTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        #region IBackgroundTask Members

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try {
                var triggerDetail = (AppServiceTriggerDetails) taskInstance.TriggerDetails;
                _deferral = taskInstance.GetDeferral();

                Hub.Instance.ForegroundConnection = triggerDetail.AppServiceConnection;
                Hub.Instance.ForegroundTask = this;

                taskInstance.Canceled += (s, e) => Close();
                triggerDetail.AppServiceConnection.ServiceClosed += (s, e) => Close();
            }
            catch (System.Exception e)
            {
                if (Hub.Instance.IsAppInsightsEnabled)
                {
                    Hub.Instance.RTCStatsManager.TrackException(e);
                }
                if (_deferral != null)
                {
                    _deferral.Complete();
                }
                throw e;
            }
        }

        #endregion

        private void Close()
        {
            Hub.Instance.ForegroundTask = null;
            _deferral?.Complete();
        }
    }
}