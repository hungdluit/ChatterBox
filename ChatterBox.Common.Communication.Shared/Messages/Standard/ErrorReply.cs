using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Standard
{
    public sealed class ErrorReply : IMessageReply
    {
        public string ErrorMessage { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset SentDateTimeUtc { get; set; }
        public Guid ReplyFor { get; set; }

        public static ErrorReply For(IMessage message)
        {
            return new ErrorReply
            {
                ReplyFor = message.Id
            };
        }
    }
}