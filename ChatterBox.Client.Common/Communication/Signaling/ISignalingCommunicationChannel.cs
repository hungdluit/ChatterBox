using ChatterBox.Client.Common.Communication.Signaling.Dto;

namespace ChatterBox.Client.Common.Communication.Signaling
{
    public interface ISignalingCommunicationChannel
    {
        ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner);
        ConnectionStatus GetConnectionStatus();
        void Register();
    }
}
