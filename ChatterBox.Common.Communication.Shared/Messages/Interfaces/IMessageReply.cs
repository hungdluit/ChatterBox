using System;

namespace ChatterBox.Common.Communication.Messages.Interfaces
{
    public interface IMessageReply : IMessage
    {
        Guid ReplyFor { get; set; }
    }
}