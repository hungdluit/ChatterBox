using System;
using System.Linq;
using System.Reflection;
using ChatterBox.Shared.Communication.Serialization;

namespace ChatterBox.Shared.Communication.Helpers
{
    internal sealed class ChannelInvoker<T>
    {

        private T Handler { get; }
        public ChannelInvoker(T handler)
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

                var method = typeof(T).GetRuntimeMethods().Single(s => s.Name == methodName);
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
