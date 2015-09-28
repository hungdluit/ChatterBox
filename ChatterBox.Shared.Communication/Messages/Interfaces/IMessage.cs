using System;

namespace ChatterBox.Shared.Communication.Messages.Interfaces
{
    public interface IMessage
    {
        Guid Id { get; set; }

        DateTimeOffset SentDateTimeUtc { get; set; }
    }
}