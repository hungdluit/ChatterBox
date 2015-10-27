using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Peers
{
    public sealed class PeerUpdate : IMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public PeerData PeerData { get; set; }
        public DateTimeOffset SentDateTimeUtc { get; set; }
    }
}