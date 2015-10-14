using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Standard
{
    public sealed class InvalidMessage : IMessage
    {
        public string OriginalMessage { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset SentDateTimeUtc { get; set; }

        public static InvalidMessage For(string request)
        {
            return new InvalidMessage
            {
                OriginalMessage = request
            };
        }
    }
}