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
    public sealed class ForegroundClient : IForegroundCommunicationChannel
    {
        public void OnSignaledDataUpdated()
        {
            SendToForeground();
        }

        public void OnVoipState(VoipState state)
        {
            SendToForeground(state);
        }

        private ValueSet SendToForeground(object arg = null, [CallerMemberName] string method = null)
        {
            var channelWriteHelper = new ChannelWriteHelper(typeof (IForegroundCommunicationChannel));
            var message = channelWriteHelper.FormatOutput(arg, method);
            var sendMessageTask = Hub.Instance.ForegroundConnection.SendMessageAsync(new ValueSet
            {
                {typeof (IForegroundCommunicationChannel).Name, message}
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