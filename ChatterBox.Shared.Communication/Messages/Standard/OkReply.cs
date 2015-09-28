using System;
using ChatterBox.Shared.Communication.Messages.Interfaces;

namespace ChatterBox.Shared.Communication.Messages.Standard
{
    public sealed class OkReply : IMessageReply
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime SentDateTimeUtc { get; set; }
        public Guid ReplyFor { get; set; }

        public static OkReply For(IMessage message)
        {
            return new OkReply
            {
                ReplyFor = message.Id
            };
        }

        public static OkReply For(Guid id)
        {
            return new OkReply
            {
                ReplyFor = id
            };
        }
    }
}