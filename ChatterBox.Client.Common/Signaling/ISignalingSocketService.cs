using Windows.Networking.Sockets;

namespace ChatterBox.Client.Common.Signaling
{
    public interface ISignalingSocketService
    {
        bool Connect();
        StreamSocket GetSocket();
        void HandoffSocket(StreamSocket socket);
    }
}