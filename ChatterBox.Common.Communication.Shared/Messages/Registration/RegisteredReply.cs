using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Shared.Messages.Registration
{
    public sealed class RegisteredReply : IMessageReply
    {
        public int Avatar { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset SentDateTimeUtc { get; set; }
        public string ReplyFor { get; set; }
    }
}