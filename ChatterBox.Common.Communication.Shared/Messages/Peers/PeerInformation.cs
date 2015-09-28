using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Peers
{
    public sealed class PeerInformation : IMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset SentDateTimeUtc { get; set; }

        public string UserId { get; set; }
        public string Name { get; set; }
        public bool IsOnline { get; set; }
    }
}
