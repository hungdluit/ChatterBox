using System.Runtime.CompilerServices;
using System.Text;
using ChatterBox.Shared.Communication.Serialization;

namespace ChatterBox.Shared.Communication.Helpers
{
    public sealed class ChannelWriteHelper<T>
    {
        public static string FormatOutput(object argument = null, [CallerMemberName]string method = null)
        {
            if (method == null) return null;

            var methodDefinition = typeof(T).GetMethod(method);
            if (methodDefinition == null) return null;

            var messageBuilder = new StringBuilder();
            messageBuilder.Append(method);
            if (argument == null) return messageBuilder.ToString();
            var serializedArgument = JsonConvert.Serialize(argument);
            messageBuilder.Append(" ");
            messageBuilder.Append(serializedArgument);
            return messageBuilder.ToString();
        }
    }
}