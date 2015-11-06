using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Client.Presentation.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using Windows.UI.Core;

namespace ChatterBox.Client.Win8dot1.Services
{
    internal class ForegroundSignalingUpdateService : ISignalingUpdateService, IForegroundChannel
    {
        public event Action OnPeerDataUpdated;
        public event Action OnRegistrationStatusUpdated;
        public event Action OnRelayMessagesUpdated;

        private CoreDispatcher _uiDispatcher;

        public ForegroundSignalingUpdateService(CoreDispatcher uiDispatcher)
        {
            _uiDispatcher = uiDispatcher;
        }

        public void OnSignaledPeerDataUpdated()
        {
            RunOnUiThread(() => OnPeerDataUpdated?.Invoke());
        }

        public void OnSignaledRegistrationStatusUpdated()
        {
            RunOnUiThread(() => OnRegistrationStatusUpdated?.Invoke());
        }

        public void OnSignaledRelayMessagesUpdated()
        {
            RunOnUiThread(() => OnRelayMessagesUpdated?.Invoke());
        }

        public void OnVoipState(VoipState state)
        {
            throw new NotImplementedException();
        }

        protected void RunOnUiThread(Action fn)
        {
            _uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(fn));
        }
    }
}
