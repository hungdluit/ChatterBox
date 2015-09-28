using System;
using ChatterBox.Shared.Communication.Messages.Interfaces;

namespace ChatterBox.Shared.Communication.Messages.Registration
{
    public sealed class Registration : IMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime SentDateTimeUtc { get; set; }

        public string UserId { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        public string PushToken { get; set; }
    }
}
