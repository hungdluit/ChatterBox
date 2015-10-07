using System;
using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Tasks.Signaling.Universal
{
    public sealed class BackgroundTaskDeferralWrapper : IDisposable
    {
        private BackgroundTaskDeferral _deferral;

        public BackgroundTaskDeferralWrapper(BackgroundTaskDeferral deferral)
        {
            _deferral = deferral;
        }

        public void Dispose()
        {
            _deferral.Complete();
            _deferral = null;
        }
    }
}