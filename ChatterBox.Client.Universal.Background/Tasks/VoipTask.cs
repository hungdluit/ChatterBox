using Windows.ApplicationModel.Background;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Common.Communication.Serialization;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class VoipTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            if (Hub.Instance.VoipTaskInstance != null)
                return;

            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += (s, e) => _deferral.Complete();
        }

        public void CloseVoipTask()
        {
            Hub.Instance.VoipTaskInstance = null;
            _deferral.Complete();
        }
    }
}