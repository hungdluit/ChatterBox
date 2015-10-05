using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using ChatterBox.Client.Settings;
using ChatterBox.Client.Signaling.Shared;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Messages.Peers;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Standard;

namespace ChatterBox.Client.Signaling
{
    public sealed class SignalingClient : IClientChannel, IServerChannel
    {
        private readonly ISignalingSocketService _signalingSocketService;
        private ChannelWriteHelper ClientChannelWriteHelper { get; } = new ChannelWriteHelper(typeof(IClientChannel));
        private ChannelInvoker ServerChannelInvoker { get; }


        public SignalingClient(ISignalingSocketService signalingSocketService)
        {
            _signalingSocketService = signalingSocketService;
            ServerChannelInvoker = new ChannelInvoker(this);
        }


        public async void Register(Registration message)
        {
            await SendToServer(message);
        }

        public async void ClientConfirmation(Confirmation confirmation)
        {
            await SendToServer(confirmation);
        }



        public async void GetPeerList(Message message)
        {
            await SendToServer(message);
        }

        public async void ClientHeartBeat()
        {
            await SendToServer();
        }




        public void ServerConfirmation(Confirmation confirmation)
        {
        }

        public void ServerReceivedInvalidMessage(InvalidMessage reply)
        {

        }

        public void ServerError(ErrorReply reply)
        {

        }

        public void OnPeerPresence(PeerInformation peer)
        {
            ClientConfirmation(Confirmation.For(peer));
            GetPeerList(new Message());
        }

        public void OnPeerList(PeerList peerList)
        {
            ClientConfirmation(Confirmation.For(peerList));
            foreach (var peerStatus in peerList.Peers.Select(peer => new Contact
            {
                UserId = peer.UserId,
                Name = peer.Name,
                IsOnline = peer.IsOnline
            }))
            {
                ContactService.AddOrUpdate(peerStatus);
            }
        }


        public void OnRegistrationConfirmation(OkReply reply)
        {
            ClientConfirmation(Confirmation.For(reply));
            GetPeerList(new Message());
        }



        public void ServerHeartBeat()
        {
            ClientHeartBeat();
        }



        private async Task SendToServer(object arg = null, [CallerMemberName]string method = null)
        {
            var message = ClientChannelWriteHelper.FormatOutput(arg, method);
            var socket = _signalingSocketService.GetSocket();
            if (socket != null)
            {

                using (var writer = new DataWriter(socket.OutputStream))
                {
                    writer.WriteString($"{message}{Environment.NewLine}");
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                }
                _signalingSocketService.HandoffSocket(socket);
            }
        }

        public async void RegisterUsingSettings()
        {
            var bufferFile = await GetCommunicationBufferFile();
            await bufferFile.DeleteAsync();

            Register(new Registration
            {
                Name = RegistrationSettings.Name,
                Domain = RegistrationSettings.Domain,
                PushToken = RegistrationSettings.Name,
                UserId = RegistrationSettings.UserId,
            });
        }


        public async void Read()
        {

            try
            {
                var socket = _signalingSocketService.GetSocket();
                string request;
                using (var reader = new DataReader(socket.InputStream)
                {
                    InputStreamOptions = InputStreamOptions.Partial
                })
                {
                    await reader.LoadAsync(8192);
                    request = reader.ReadString(reader.UnconsumedBufferLength);
                    _signalingSocketService.HandoffSocket(socket);
                }


                var bufferFile = await GetCommunicationBufferFile();
                FileIO.AppendTextAsync(bufferFile, request).AsTask().Wait();
                var requests = (await FileIO.ReadLinesAsync(bufferFile)).ToList();

                if (requests.Count == 1)
                {
                    var invoked = ServerChannelInvoker.ProcessRequest(requests.Single());
                    if (invoked)
                    {
                        await bufferFile.DeleteAsync();
                    }
                }
                else
                {
                    for (var i = 0; i < requests.Count; i++)
                    {
                        var invoked = ServerChannelInvoker.ProcessRequest(requests[i]);
                        if (i != requests.Count - 1) continue;
                        //If the last message failed the invocation, leave it in the buffer file (it might be a partial message)
                        await bufferFile.DeleteAsync();
                        if (invoked) continue;
                        bufferFile = await GetCommunicationBufferFile();
                        await FileIO.AppendTextAsync(bufferFile, requests[i]);
                    }
                }
            }
            catch (Exception exception)
            {

            }
        }





        private async Task<StorageFile> GetCommunicationBufferFile()
        {
            try
            {
                return await
                    ApplicationData.Current.LocalFolder.CreateFileAsync("CommunicationBufferFile",
                        CreationCollisionOption.OpenIfExists);
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public bool CheckConnection()
        {
            var socket = _signalingSocketService.GetSocket();
            var isConnected = socket != null;
            if (socket != null)
            {
                _signalingSocketService.HandoffSocket(socket);
            }
            return isConnected;
        }

        public bool Connect()
        {
            ContactService.Reset();
            return _signalingSocketService.Connect(SignalingSettings.SignalingServerHost,
                int.Parse(SignalingSettings.SignalingServerPort));
        }
    }
}
