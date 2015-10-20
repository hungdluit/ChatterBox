using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Universal.Tasks.Voip
{
    public sealed class VoipTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += (s, e) => _deferral.Complete();
        }
    }
}