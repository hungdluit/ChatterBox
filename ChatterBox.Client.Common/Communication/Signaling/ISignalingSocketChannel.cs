using ChatterBox.Client.Common.Communication.Signaling.Dto;

namespace ChatterBox.Client.Common.Communication.Signaling
{
    public interface ISignalingSocketChannel
    {
        ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner);
        ConnectionStatus GetConnectionStatus();
    }
}