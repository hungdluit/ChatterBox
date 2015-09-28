using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Standard
{
    public sealed class OkReply : IMessageReply
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset SentDateTimeUtc { get; set; }
        public Guid ReplyFor { get; set; }

        public static OkReply For(IMessage message)
        {
            return new OkReply
            {
                ReplyFor = message.Id
            };
        }
    }
}