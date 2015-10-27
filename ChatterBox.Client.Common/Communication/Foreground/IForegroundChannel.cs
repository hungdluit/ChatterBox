using ChatterBox.Client.Common.Communication.Foreground.Dto;

namespace ChatterBox.Client.Common.Communication.Foreground
{
    public interface IForegroundChannel
    {
        void OnSignaledRegistrationStatusUpdated();
        void OnSignaledPeerDataUpdated();
        void OnSignaledRelayMessagesUpdated();

        void OnVoipState(VoipState state);
    }
}