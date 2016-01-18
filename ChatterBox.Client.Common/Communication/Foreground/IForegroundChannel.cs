using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Foreground.Dto.ChatterBox.Client.Common.Communication.Foreground.Dto;

namespace ChatterBox.Client.Common.Communication.Foreground
{
    public interface IForegroundChannel
    {
        void OnSignaledPeerDataUpdated();
        void OnSignaledRegistrationStatusUpdated();
        void OnSignaledRelayMessagesUpdated();
        void OnVoipState(VoipState state);
        void OnUpdateFrameFormat(FrameFormat frameFormat);
        ForegroundState GetForegroundState();
        string GetShownUserId();
    }
}