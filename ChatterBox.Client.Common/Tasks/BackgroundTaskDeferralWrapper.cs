using System;
using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Common.Background.DeferralWrappers
{
    public sealed class BackgroundTaskDeferralWrapper : IDisposable
    {
        private BackgroundTaskDeferral _deferral;

        public BackgroundTaskDeferralWrapper(BackgroundTaskDeferral deferral)
        {
            _deferral = deferral;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _deferral.Complete();
            _deferral = null;
        }

        #endregion
    }
}