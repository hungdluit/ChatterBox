using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Standard
{
    public sealed class OkReply : IMessageReply
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset SentDateTimeUtc { get; set; }
        public string ReplyFor { get; set; }

        public static OkReply For(IMessage message)
        {
            return new OkReply
            {
                ReplyFor = message.Id
            };
        }
    }
}