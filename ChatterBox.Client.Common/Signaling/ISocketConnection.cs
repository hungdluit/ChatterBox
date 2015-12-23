using Windows.Foundation;

namespace ChatterBox.Client.Common.Signaling
{
    public interface ISocketConnection
    {
        IAsyncOperation<bool> Connect();
        IAsyncOperation<bool> Disconnect();
        void Register();   
        bool IsConnected { get; }
    }
}
