using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using ChatterBox.Client.Notifications;
using ChatterBox.Client.Settings;
using ChatterBox.Client.Signaling.Shared;
using ChatterBox.Client.Signaling.Shared.Avatars;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Messages.Peers;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Standard;
using ChatterBox.Common.Communication.Shared.Messages.Registration;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Signaling
{
    public sealed class SignalingClient : IClientChannel, IServerChannel
    {
        private readonly ISignaledDataUpdateNotifier _signaledDataUpdateNotifier;
        private readonly ISignalingSocketService _signalingSocketService;

        public SignalingClient(ISignalingSocketService signalingSocketService,
            ISignaledDataUpdateNotifier signaledDataUpdateNotifier)
        {
            _signalingSocketService = signalingSocketService;
            _signaledDataUpdateNotifier = signaledDataUpdateNotifier;
            ServerChannelInvoker = new ChannelInvoker(this);
        }

        private ChannelWriteHelper ClientChannelWriteHelper { get; } = new ChannelWriteHelper(typeof (IClientChannel));
        private ChannelInvoker ServerChannelInvoker { get; }

        public async void ClientConfirmation(Confirmation confirmation)
        {
            await SendToServer(confirmation);
        }

        public async void ClientHeartBeat()
        {
            await SendToServer();
        }

        public async void GetPeerList(Message message)
        {
            await SendToServer(message);
        }

        public async void Register(Registration message)
        {
            await SendToServer(message);
        }

        public async void Relay(RelayMessage message)
        {
            await SendToServer(message);
        }

        public void OnPeerList(PeerList peerList)
        {
            ClientConfirmation(Confirmation.For(peerList));
            foreach (var peerStatus in peerList.Peers)
            {
                SignaledPeerData.AddOrUpdate(peerStatus);
            }
            _signaledDataUpdateNotifier?.RaiseSignaledDataUpdated();
        }

        public void OnPeerPresence(PeerUpdate peer)
        {
            ClientConfirmation(Confirmation.For(peer));
            GetPeerList(new Message());

            if (peer.SentDateTimeUtc.Subtract(DateTimeOffset.UtcNow).TotalSeconds < 10)
            {
                ToastNotificationService.ShowPresenceNotification(
                    peer.PeerData.Name,
                    AvatarLink.For(peer.PeerData.Avatar),
                    peer.PeerData.IsOnline);
            }
            _signaledDataUpdateNotifier?.RaiseSignaledDataUpdated();
        }

        public void OnRegistrationConfirmation(RegisteredReply reply)
        {
            ClientConfirmation(Confirmation.For(reply));
            SignalingStatus.IsRegistered = true;
            SignalingStatus.Avatar = reply.Avatar;
            GetPeerList(new Message());
            _signaledDataUpdateNotifier?.RaiseSignaledDataUpdated();
        }

        public void ServerConfirmation(Confirmation confirmation)
        {
        }

        public void ServerError(ErrorReply reply)
        {
        }

        public void ServerHeartBeat()
        {
            ClientHeartBeat();
        }

        public void ServerReceivedInvalidMessage(InvalidMessage reply)
        {
        }

        public void ServerRelay(RelayMessage message)
        {
            ClientConfirmation(Confirmation.For(message));
            SignaledRelayMessages.Add(message);
            if (message.Tag == RelayMessageTags.InstantMessage &&
                (message.SentDateTimeUtc.Subtract(DateTimeOffset.UtcNow).TotalMinutes < 10))
            {
                ToastNotificationService.ShowInstantMessageNotification(message.FromName,
                    AvatarLink.For(message.FromAvatar), message.Payload);
            }
            _signaledDataUpdateNotifier?.RaiseSignaledDataUpdated();
        }

        private IAsyncOperation<bool> BufferFileExists()
        {
            return
                Task.Run(
                    async () =>
                    {
                        return
                            (await ApplicationData.Current.LocalFolder.GetFilesAsync()).Any(s => s.Name == "BufferFile");
                    }).AsAsyncOperation();
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
            SignaledPeerData.Reset();
            SignalingStatus.Reset();
            SignaledRelayMessages.Reset();
            return _signalingSocketService.Connect(SignalingSettings.SignalingServerHost,
                int.Parse(SignalingSettings.SignalingServerPort));
        }

        private IAsyncOperation<StorageFile> GetBufferFile()
        {
            return ApplicationData.Current.LocalFolder.CreateFileAsync("BufferFile",
                CreationCollisionOption.OpenIfExists);
        }

        public void HandleRequest(string request)
        {
            List<string> requests;
            var fileTask = BufferFileExists().AsTask();
            fileTask.Wait();
            if (fileTask.Result)
            {
                var bufferFileTask = GetBufferFile().AsTask();
                bufferFileTask.Wait();
                var bufferFile = bufferFileTask.Result;

                var task = FileIO.AppendTextAsync(bufferFile, request).AsTask();
                task.Wait();

                var readLinesTask = FileIO.ReadLinesAsync(bufferFile).AsTask();
                readLinesTask.Wait();
                requests = (readLinesTask.Result).ToList();

                var deleteTask = bufferFile.DeleteAsync().AsTask();
                deleteTask.Wait();
            }
            else
            {
                requests = request.Split(new[] {Environment.NewLine},
                    StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            for (var i = 0; i < requests.Count; i++)
            {
                var invoked = ServerChannelInvoker.ProcessRequest(requests[i]);
                if (i != requests.Count - 1) continue;
                if (invoked) continue;
                var bufferFileTask = GetBufferFile().AsTask();
                bufferFileTask.Wait();
                var appendTask = FileIO.AppendTextAsync(bufferFileTask.Result, requests[i]).AsTask();
                appendTask.Wait();
            }
        }

        public async void RegisterUsingSettings()
        {
            var bufferFile = await GetBufferFile();
            await bufferFile.DeleteAsync();

            Register(new Registration
            {
                Name = RegistrationSettings.Name,
                Domain = RegistrationSettings.Domain,
                PushToken = RegistrationSettings.Name,
                UserId = RegistrationSettings.UserId
            });
        }

        private async Task SendToServer(object arg = null, [CallerMemberName] string method = null)
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
    }
}