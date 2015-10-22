using Windows.Networking.Sockets;

namespace ChatterBox.Client.Common.Signaling
{
    public interface ISignalingSocketService
    {
        StreamSocket GetSocket();
        void HandoffSocket(StreamSocket socket);
    }
}