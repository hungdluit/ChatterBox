using System;

namespace ChatterBox.Shared.Communication.Messages.Interfaces
{
    public interface IMessageReply: IMessage
    {
        Guid ReplyFor { get; set; }
    }
}