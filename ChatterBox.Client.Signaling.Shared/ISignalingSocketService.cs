using Windows.Networking.Sockets;

namespace ChatterBox.Client.Signaling
{
    public interface ISignalingSocketService
    {
        bool Connect(string hostname, int port);
        StreamSocket GetSocket();
        void HandoffSocket(StreamSocket socket);
    }
}
