using System;
using Windows.ApplicationModel.AppService;

namespace ChatterBox.Client.Universal.Background.DeferralWrappers
{
    public sealed class AppServiceDeferralWrapper : IDisposable
    {
        private AppServiceDeferral _deferral;

        public AppServiceDeferralWrapper(AppServiceDeferral deferral)
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