using System;

namespace ChatterBox.Common.Communication.Messages.Interfaces
{
    public interface IMessage
    {
        string Id { get; set; }
        DateTimeOffset SentDateTimeUtc { get; set; }
    }
}