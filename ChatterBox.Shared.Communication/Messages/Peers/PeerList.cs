using System;
using System.Collections.Generic;
using ChatterBox.Shared.Communication.Messages.Interfaces;

namespace ChatterBox.Shared.Communication.Messages.Peers
{
    public sealed class PeerList : IMessageReply
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime SentDateTimeUtc { get; set; }
        public Guid ReplyFor { get; set; }

        public List<PeerInformation> Peers { get; set; } = new List<PeerInformation>();
    }

}
