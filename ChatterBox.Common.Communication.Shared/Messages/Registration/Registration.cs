using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Registration
{
    public sealed class Registration : IMessage
    {
        public string Domain { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string PushNotificationChannelURI { get; set; }
        public DateTimeOffset SentDateTimeUtc { get; set; }
        public string UserId { get; set; }
    }
}