using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Peers
{
    public sealed class PeerList : IMessageReply
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public PeerData[] Peers { get; set; }
        public string ReplyFor { get; set; }
        public DateTimeOffset SentDateTimeUtc { get; set; }
    }
}