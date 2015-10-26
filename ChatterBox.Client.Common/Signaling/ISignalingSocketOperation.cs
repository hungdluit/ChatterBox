using System;
using Windows.Networking.Sockets;

namespace ChatterBox.Client.Common.Signaling
{
    public interface ISignalingSocketOperation : IDisposable
    {
        StreamSocket Socket { get; }
    }
}
