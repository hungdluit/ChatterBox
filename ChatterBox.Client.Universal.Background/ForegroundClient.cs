using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Client.Universal.Background
{
    public sealed class ForegroundClient : IForegroundChannel
    {
        #region IForegroundChannel Members

        public void OnSignaledPeerDataUpdated()
        {
            SendToForeground();
        }

        public void OnSignaledRegistrationStatusUpdated()
        {
            SendToForeground();
        }

        public void OnSignaledRelayMessagesUpdated()
        {
            SendToForeground();
        }

        public void OnVoipState(VoipState state)
        {
            SendToForeground(state);
        }

        #endregion

        private ValueSet SendToForeground(object arg = null, [CallerMemberName] string method = null)
        {
            var channelWriteHelper = new ChannelWriteHelper(typeof (IForegroundChannel));
            var message = channelWriteHelper.FormatOutput(arg, method);
            var sendMessageTask = Hub.Instance.ForegroundConnection.SendMessageAsync(new ValueSet
            {
                {typeof (IForegroundChannel).Name, message}
            }).AsTask();
            sendMessageTask.Wait();
            return sendMessageTask.Result.Status != AppServiceResponseStatus.Success
                ? null
                : sendMessageTask.Result.Message;
        }

        private TResult SendToForeground<TResult>(object arg = null, [CallerMemberName] string method = null)
            where TResult : class
        {
            var resultMessage = SendToForeground(arg, method);
            if (resultMessage == null) return null;
            if (!resultMessage.Values.Any()) return null;
            return (TResult) JsonConvert.Deserialize(resultMessage.Values.Single().ToString(), typeof (TResult));
        }
    }
}