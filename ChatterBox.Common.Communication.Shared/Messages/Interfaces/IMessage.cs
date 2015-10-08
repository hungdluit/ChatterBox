using System;

namespace ChatterBox.Common.Communication.Messages.Interfaces
{
    public interface IMessage
    {
        Guid Id { get; set; }
        DateTimeOffset SentDateTimeUtc { get; set; }
    }
}