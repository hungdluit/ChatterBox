using System;

namespace ChatterBox.Client.Common.Signaling
{
    public interface ISocketConnection
    {
        void Connect();
        void Disconnect();
        void Register();
        bool IsConnecting {get; set;}
        bool IsConnectionFailed { get; set; }
        bool IsConnected { get; }

        event EventHandler<object> OnConnectionFailed;
        event EventHandler<object> OnConnecting;
        event EventHandler<object> OnRegistering;
    }
}
