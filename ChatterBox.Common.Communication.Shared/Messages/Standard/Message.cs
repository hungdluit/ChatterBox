using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Standard
{
    public sealed class Message : IMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset SentDateTimeUtc { get; set; }
    }
}