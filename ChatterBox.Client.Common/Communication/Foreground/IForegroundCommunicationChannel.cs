using ChatterBox.Client.Common.Communication.Foreground.Dto;

namespace ChatterBox.Client.Common.Communication.Foreground
{
    public interface IForegroundCommunicationChannel
    {
        void OnSignaledDataUpdated();

        void OnVoipState(VoipState state);
    }
}