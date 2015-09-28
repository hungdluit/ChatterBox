using System;
using ChatterBox.Shared.Communication.Messages.Interfaces;

namespace ChatterBox.Shared.Communication.Messages.Peers
{
    public sealed class PeerInformation : IMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime SentDateTimeUtc { get; set; }

        public string UserId { get; set; }
        public string Name { get; set; }
        public bool IsOnline { get; set; }
    }
}
