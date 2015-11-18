using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using ChatterBox.Client.Common.Avatars;
using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Notifications;
using ChatterBox.Client.Common.Signaling.PersistedData;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Messages.Peers;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Messages.Standard;

namespace ChatterBox.Client.Common.Signaling
{
    public sealed class SignalingClient : IClientChannel, IServerChannel
    {
        private readonly IForegroundChannel _foregroundChannel;
        private readonly ISignalingSocketService _signalingSocketService;
        private readonly IVoipChannel _voipChannel;

        public SignalingClient(ISignalingSocketService signalingSocketService,
            IForegroundChannel foregroundChannel,
            IVoipChannel voipChannel)
        {
            _signalingSocketService = signalingSocketService;
            _voipChannel = voipChannel;
            _foregroundChannel = foregroundChannel;
            ServerChannelInvoker = new ChannelInvoker(this);
        }

        private ChannelWriteHelper ClientChannelWriteHelper { get; } = new ChannelWriteHelper(typeof(IClientChannel));
        private ChannelInvoker ServerChannelInvoker { get; }

        #region IClientChannel Members

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
            var bufferFile = await GetBufferFile();
            await bufferFile.DeleteAsync();
            await SendToServer(message);
        }

        public async void Relay(RelayMessage message)
        {
            await SendToServer(message);
        }

        #endregion

        #region IServerChannel Members

        public void OnPeerList(PeerList peerList)
        {
            ClientConfirmation(Confirmation.For(peerList));
            foreach (var peerStatus in peerList.Peers)
            {
                SignaledPeerData.AddOrUpdate(peerStatus);
            }
            _foregroundChannel?.OnSignaledPeerDataUpdated();
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
            _foregroundChannel?.OnSignaledPeerDataUpdated();
        }

        public void OnRegistrationConfirmation(RegisteredReply reply)
        {
            ClientConfirmation(Confirmation.For(reply));
            SignalingStatus.IsRegistered = true;
            SignalingStatus.Avatar = reply.Avatar;
            GetPeerList(new Message());
            _foregroundChannel?.OnSignaledRegistrationStatusUpdated();
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
            if (message.Tag == RelayMessageTags.InstantMessage && !SignaledRelayMessages.IsPushNotificationReceived(message.Id) &&
                (message.SentDateTimeUtc.Subtract(DateTimeOffset.UtcNow).TotalMinutes < 10))
            {
                ToastNotificationService.ShowInstantMessageNotification(message.FromName,
                    AvatarLink.For(message.FromAvatar), message.Payload);
            }
            _foregroundChannel?.OnSignaledRelayMessagesUpdated();

            // Handle Voip tags
            if (message.Tag == RelayMessageTags.VoipCall)
            {
                _voipChannel.OnIncomingCall(message);
            }
            else if (message.Tag == RelayMessageTags.VoipAnswer)
            {
                _voipChannel.OnOutgoingCallAccepted(message);
            }
            else if (message.Tag == RelayMessageTags.VoipReject)
            {
                _voipChannel.OnOutgoingCallRejected(message);
            }
            else if (message.Tag == RelayMessageTags.SdpOffer)
            {
                _voipChannel.OnSdpOffer(message);
            }
            else if (message.Tag == RelayMessageTags.SdpAnswer)
            {
                _voipChannel.OnSdpAnswer(message);
            }
            else if (message.Tag == RelayMessageTags.IceCandidate)
            {
                _voipChannel.OnIceCandidate(message);
            }
            else if (message.Tag == RelayMessageTags.VoipHangup)
            {
                _voipChannel.OnRemoteHangup(message);
            }
        }

        #endregion

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
                requests = request.Split(new[] { Environment.NewLine },
                    StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            for (var i = 0; i < requests.Count; i++)
            {
                var invoked = ServerChannelInvoker.ProcessRequest(requests[i]);
                if (i != requests.Count - 1) continue;
                if (invoked.Invoked) continue;
                var bufferFileTask = GetBufferFile().AsTask();
                bufferFileTask.Wait();
                var appendTask = FileIO.AppendTextAsync(bufferFileTask.Result, requests[i]).AsTask();
                appendTask.Wait();
            }
        }

        private async Task SendToServer(object arg = null, [CallerMemberName] string method = null)
        {
            var message = ClientChannelWriteHelper.FormatOutput(arg, method);

            using (var socketOperation = _signalingSocketService.SocketOperation)
            {
                var socket = socketOperation.Socket;
                if (socket != null)
                {
                    using (var writer = new DataWriter(socket.OutputStream))
                    {
                        writer.WriteString($"{message}{Environment.NewLine}");
                        await writer.StoreAsync();
                        await writer.FlushAsync();
                        writer.DetachStream();
                    }
                }
            }
        }
    }
}