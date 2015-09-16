using System;
using ChatterBox.Shared.Communication.Messages.Interfaces;

namespace ChatterBox.Shared.Communication.Messages.Standard
{
    public sealed class ErrorReply : IMessageReply
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ReplyFor { get; set; }

        public string ErrorMessage { get; set; }

        public static ErrorReply For(IMessage message)
        {
            return new ErrorReply
            {
                ReplyFor = message.Id
            };
        }

        public static ErrorReply For(Guid id)
        {
            return new ErrorReply
            {
                ReplyFor = id
            };
        }
    }
}
