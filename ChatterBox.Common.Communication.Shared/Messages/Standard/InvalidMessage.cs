using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Standard
{
    public sealed class InvalidMessage : IMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset SentDateTimeUtc { get; set; }
        public string OriginalMessage { get; set; }

        public static InvalidMessage For(string request)
        {
            return new InvalidMessage
            {
                OriginalMessage = request
            };
        }
    }
}