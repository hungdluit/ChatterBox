using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Standard
{
    public sealed class ErrorReply : IMessageReply
    {
        public string ErrorMessage { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset SentDateTimeUtc { get; set; }
        public string ReplyFor { get; set; }

        public static ErrorReply For(IMessage message)
        {
            return new ErrorReply
            {
                ReplyFor = message.Id
            };
        }
    }
}