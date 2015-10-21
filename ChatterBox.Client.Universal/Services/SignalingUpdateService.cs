using System;
using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;

namespace ChatterBox.Client.Universal.Services
{
    public sealed class SignalingUpdateService : DispatcherBindableBase, ISignalingUpdateService
    {
        public SignalingUpdateService(ForegroundAppServiceClient foregroundAppServiceClient, CoreDispatcher uiDispatcher)
            : base(uiDispatcher)
        {
            foregroundAppServiceClient.OnSignlingDataUpdated += OnSignlingDataUpdated;
        }

        public event Action OnUpdate;

        public void RaiseUpdate()
        {
            RunOnUiThread(() => { OnUpdate?.Invoke(); });
        }

        private void OnSignlingDataUpdated()
        {
            RaiseUpdate();
        }
    }
}