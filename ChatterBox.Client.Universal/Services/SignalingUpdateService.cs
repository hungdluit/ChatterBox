using System;
using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;

namespace ChatterBox.Client.Universal.Services
{
    public sealed class SignalingUpdateService : DispatcherBindableBase, ISignalingUpdateService
    {
        public SignalingUpdateService(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
        }

        public event Action OnUpdate;

        public void RaiseUpdate()
        {
            RunOnUiThread(() => { OnUpdate?.Invoke(); });
        }
    }
}