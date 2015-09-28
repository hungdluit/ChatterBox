using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using ChatterBox.Client.Signaling;

namespace ChatterBox.Client.Tasks.Signaling.Universal
{
    public sealed class SignalingSocketService : ISignalingSocketService
    {
        public bool Connect(string hostname, int port)
        {
            throw new NotImplementedException();
        }

        public StreamSocket GetSocket()
        {
            throw new NotImplementedException();
        }

        public void HandleSocket(StreamSocket socket)
        {
            throw new NotImplementedException();
        }
    }
}
