using System;
using System.Linq;
using System.Reflection;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Common.Communication.Helpers
{
    public sealed class ChannelInvoker
    {

        private object Handler { get; }
        public ChannelInvoker(object handler)
        {
            Handler = handler;
        }


        public bool ProcessRequest(string request)
        {
            try
            {
                var methodName = !request.Contains(" ")
                    ? request
                    : request.Substring(0, request.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase));

                var method = Handler.GetType().GetRuntimeMethods().Single(s => s.Name == methodName);
                var parameters = method.GetParameters();

                object argument = null;

                if (parameters.Any())
                {
                    var serializedParameter =
                        request.Substring(request.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase) + 1);
                    argument = JsonConvert.Deserialize(serializedParameter, parameters.Single().ParameterType);
                }

                method.Invoke(Handler, argument == null ? null : new[] { argument });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    
}
