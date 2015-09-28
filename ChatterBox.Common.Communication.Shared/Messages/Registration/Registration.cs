using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Registration
{
    public sealed class Registration : IMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset SentDateTimeUtc { get; set; }

        public string UserId { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        public string PushToken { get; set; }
    }
}
