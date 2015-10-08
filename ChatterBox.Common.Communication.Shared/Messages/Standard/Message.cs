using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Standard
{
    public sealed class Message : IMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset SentDateTimeUtc { get; set; }
    }
}