using System;
using ChatterBox.Shared.Communication.Messages.Interfaces;

namespace ChatterBox.Shared.Communication.Messages.Standard
{
    public sealed class InvalidMessage : IMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
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