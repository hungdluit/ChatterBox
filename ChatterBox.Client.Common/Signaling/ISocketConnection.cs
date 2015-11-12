using System;

namespace ChatterBox.Client.Common.Signaling
{
    public interface ISocketConnection
    {
        bool Connect();
        void Disconnect();
        void Register();
        bool IsConnecting {get;}
        bool IsConnectingFailed { get;}
        bool IsConnected { get; }

        event EventHandler<object> OnConnectingStarted;
        event EventHandler<object> OnConnectingFinished;
        event EventHandler<object> OnRegistering;
    }
}
