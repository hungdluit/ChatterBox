using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Peers
{
    public sealed class PeerInformation : IMessage
    {
        public bool IsOnline { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset SentDateTimeUtc { get; set; }
    }
}