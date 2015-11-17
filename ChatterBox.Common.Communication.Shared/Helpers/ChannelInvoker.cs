using System;
using System.Linq;
using System.Reflection;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Common.Communication.Helpers
{
    public sealed class ChannelInvoker
    {
        public ChannelInvoker(object handler)
        {
            Handler = handler;
        }

        private object Handler { get; }

        public InvocationResult ProcessRequest(string request)
        {
            try
            {
                var methodName = !request.Contains(" ")
                    ? request
                    : request.Substring(0, request.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase));

                var methods = Handler.GetType().GetRuntimeMethods();
                var method = methods.Single(s => s.Name == methodName);
                var parameters = method.GetParameters();

                object argument = null;

                if (parameters.Any())
                {
                    var serializedParameter =
                        request.Substring(request.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase) + 1);
                    argument = JsonConvert.Deserialize(serializedParameter, parameters.Single().ParameterType);
                }

                var result = method.Invoke(Handler, argument == null ? null : new[] {argument});
                return new InvocationResult
                {
                    Invoked = true,
                    Result = result
                };
            }
            catch(Exception exception)
            {
                return new InvocationResult
                {
                    Invoked = false,
                  ErrorMessage = exception.ToString()
                 };
            }
        }
    }
}