using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Client.Universal.Background.Helpers
{
    public static class AppServiceChannelHelper
    {
        public static async void HandleRequest(AppServiceRequest request, object handler, string message)
        {
            var invoker = new ChannelInvoker(handler);
            var result = invoker.ProcessRequest(message);
            await SendResponse(request, result);
        }

        public static ValueSet InvokeChannel(this AppServiceConnection connection, Type contractType, object argument,
            string method)
        {
            var channelWriteHelper = new ChannelWriteHelper(contractType);
            var message = channelWriteHelper.FormatOutput(argument, method);
            var sendMessageTask = connection.SendMessageAsync(new ValueSet {{contractType.Name, message}}).AsTask();
            sendMessageTask.Wait();
            return sendMessageTask.Result.Status != AppServiceResponseStatus.Success
                ? null
                : sendMessageTask.Result.Message;
        }

        public static object InvokeChannel(this AppServiceConnection connection, Type contractType, object argument,
            string method, Type responseType)
        {
            var resultMessage = connection.InvokeChannel(contractType, argument, method);
            if (resultMessage == null) return null;
            if (!resultMessage.Values.Any()) return null;
            return JsonConvert.Deserialize(resultMessage.Values.Single().ToString(), responseType);
        }

        private static async Task SendResponse(AppServiceRequest request, InvocationResult result)
        {
            if (result.Result == null) return;
            await request.SendResponseAsync(new ValueSet
            {
                {Guid.NewGuid().ToString(), JsonConvert.Serialize(result.Result)}
            });
        }
    }
}