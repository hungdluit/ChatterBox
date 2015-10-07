using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Shared.Messages.Relay
{
    public sealed class RelayMessage : IMessage
    {
        public string FromUserId { get; set; }
        public string Payload { get; set; }
        public string Tag { get; set; }
        public string ToUserId { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset SentDateTimeUtc { get; set; }
    }
}