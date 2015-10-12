using ChatterBox.Client.Signaling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace ChatterBox.Client.Tasks.Signaling.Win8dot1
{
    public sealed class SignalingSocketService : ISignalingSocketService
    {
        ControlChannelTrigger _controlChannelTrigger;
        private bool _isConnected;

        public SignalingSocketService(ControlChannelTrigger streamSocket)
        {
            _controlChannelTrigger = streamSocket;
        }

        public bool Connect(string hostname, int port)
        {
            try
            {
                var streamSocket = (StreamSocket)_controlChannelTrigger.TransportObject;
                streamSocket.ConnectAsync(new HostName(hostname), port.ToString())
                    .AsTask().Wait();

                var status = _controlChannelTrigger.WaitForPushEnabled();
                if (status != ControlChannelTriggerStatus.HardwareSlotAllocated &&
                    status != ControlChannelTriggerStatus.SoftwareSlotAllocated)
                {
                    _isConnected = false;
                }
                else
                {
                    _isConnected = true;
                }
            }
            catch (Exception ex)
            {
                _isConnected = false;
            }
            return _isConnected;
        }

        public StreamSocket GetSocket()
        {
            return _isConnected ? (StreamSocket)_controlChannelTrigger.TransportObject : null;
        }

        public void HandoffSocket(StreamSocket socket)
        {

        }
    }
}
