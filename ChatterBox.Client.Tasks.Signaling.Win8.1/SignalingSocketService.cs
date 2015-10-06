using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatterBox.Client.Signaling;
using Windows.Networking.Sockets;

namespace ChatterBox.Client.Tasks.Signaling.Win8._1
{
    public sealed class SignalingSocketService : ISignalingSocketService
    {
        public bool Connect(string hostname, int port)
        {
            throw new NotImplementedException();
        }

        public StreamSocket GetSocket()
        {
            return null;
        }

        public void HandoffSocket(StreamSocket socket)
        {
            
        }
    }
}
