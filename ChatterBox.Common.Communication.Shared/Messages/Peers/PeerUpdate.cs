using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Peers
{
    public sealed class PeerUpdate : IMessage
    {
        public PeerData PeerData { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset SentDateTimeUtc { get; set; }
    }
}