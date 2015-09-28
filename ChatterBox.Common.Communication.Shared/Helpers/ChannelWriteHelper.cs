using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ChatterBox.Common.Communication.Messages.Interfaces;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Common.Communication.Helpers
{
    public sealed class ChannelWriteHelper
    {
        private readonly Type _target;

        public ChannelWriteHelper(Type target)
        {
            _target = target;
        }

        public string FormatOutput(object argument, string method)
        {
            var message = argument as IMessage;
            if (message != null)
            {
                message.SentDateTimeUtc=DateTime.UtcNow;
            }


            if (method == null) return null;
            var methodDefinition = _target.GetRuntimeMethods().Single(s => s.Name == method);
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