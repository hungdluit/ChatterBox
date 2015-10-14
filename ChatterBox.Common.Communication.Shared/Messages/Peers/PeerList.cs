using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Peers
{
    public sealed class PeerList : IMessageReply
    {
        public PeerData[] Peers { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset SentDateTimeUtc { get; set; }
        public string ReplyFor { get; set; }
    }
}