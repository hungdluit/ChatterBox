using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace ChatterBox.Client.Win8dot1.Services
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
