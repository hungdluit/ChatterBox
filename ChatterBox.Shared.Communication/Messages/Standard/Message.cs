using System;
using ChatterBox.Shared.Communication.Messages.Interfaces;

namespace ChatterBox.Shared.Communication.Messages.Standard
{
    public sealed class Message:IMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime SentDateTimeUtc { get; set; }
    }
}
