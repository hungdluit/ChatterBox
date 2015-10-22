using System;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Presentation.Shared.Services;
using Buffer = Windows.Storage.Streams.Buffer;

namespace ChatterBox.Client.Win8dot1.Helpers
{
    public sealed class SignalingSocketService : ISignalingSocketService
    {
        private readonly SignalingClient _signalingClient;
        private readonly ISignalingUpdateService _signalingUpdateService;
        private bool _isConnected;
        private StreamSocket _streamSocket;

        public SignalingSocketService(ISignalingUpdateService signalingUpdateService)
        {
            _signalingClient = new SignalingClient(this, null, null);
            _signalingUpdateService = signalingUpdateService;
        }

        public StreamSocket GetSocket()
        {
            return _streamSocket;
        }

        public void HandoffSocket(StreamSocket socket)
        {
        }


        public bool Connect(string hostname, int port)
        {
            try
            {
                _streamSocket = new StreamSocket();
                _streamSocket.ConnectAsync(new HostName(hostname), port.ToString(), SocketProtectionLevel.PlainSocket)
                    .AsTask().Wait();
                _isConnected = true;
                var readTask = new Task(Read);
                readTask.Start();
                Connected?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public event Action Connected;

        private async void Read()
        {
            try
            {
                while (_isConnected)
                {
                    const uint length = 65536;
                    var readBuf = new Buffer(length);
                    var localBuffer =
                        await _streamSocket.InputStream.ReadAsync(readBuf, length, InputStreamOptions.Partial);
                    var dataReader = DataReader.FromBuffer(localBuffer);
                    var request = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                    dataReader.DetachStream();
                    _signalingClient.HandleRequest(request);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}