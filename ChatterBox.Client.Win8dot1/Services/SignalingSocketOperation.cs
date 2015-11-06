using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Win8dot1.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace ChatterBox.Client.Win8dot1.Services
{
    internal class SignalingSocketOperation : ISignalingSocketOperation
    {
        private SignalingSocketChannel _signalingSocketChannel;

        public SignalingSocketOperation(SignalingSocketChannel signalingSocketService)
        {
            _signalingSocketChannel = signalingSocketService;              
        }

        public StreamSocket Socket
        {
            get
            {
                return _signalingSocketChannel.StreamSocket;
            }
        }

        public void Dispose()
        {
            //_signalingSocketChannel = null;
        }
    }
}
