using System;
using System.Linq;
using ChatterBox.Shared.Communication.Serialization;

namespace ChatterBox.Shared.Communication.Helpers
{
    public sealed class ChannelInvoker<T>
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
                    : request.Substring(0, request.IndexOf(" ", StringComparison.InvariantCulture));

                var method = typeof(T).GetMethod(methodName);
                var parameters = method.GetParameters();

                object argument = null;

                if (parameters.Any())
                {
                    var serializedParameter =
                        request.Substring(request.IndexOf(" ", StringComparison.InvariantCulture) + 1);
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
