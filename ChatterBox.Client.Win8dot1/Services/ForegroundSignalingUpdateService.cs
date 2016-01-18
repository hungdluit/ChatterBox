using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Foreground.Dto.ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Presentation.Shared.Services;
using System;
using Windows.UI.Core;

namespace ChatterBox.Client.Win8dot1.Services
{
    internal class ForegroundSignalingUpdateService : IForegroundUpdateService, IForegroundChannel
    {
        public event Action OnPeerDataUpdated;
        public event Action OnRegistrationStatusUpdated;
        public event Action OnRelayMessagesUpdated;
        public event Action<VoipState> OnVoipStateUpdate;
        public event Action<FrameFormat> OnFrameFormatUpdate;
        public event Func<string> GetShownUser;

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
            RunOnUiThread(() => OnVoipStateUpdate?.Invoke(state));
        }

        protected void RunOnUiThread(Action fn)
        {
            _uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(fn));
        }

        public void OnUpdateFrameFormat(FrameFormat frameFormat)
        {
            RunOnUiThread(() => OnFrameFormatUpdate?.Invoke(frameFormat));
        }

        public ForegroundState GetForegroundState()
        {
            return new ForegroundState { IsForegroundVisible = true };
        }

        public string GetShownUserId()
        {
            if (GetShownUser != null)
                return GetShownUser();
            return string.Empty;
        }
    }
}
