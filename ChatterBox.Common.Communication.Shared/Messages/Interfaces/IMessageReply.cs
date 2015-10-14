namespace ChatterBox.Common.Communication.Messages.Interfaces
{
    public interface IMessageReply : IMessage
    {
        string ReplyFor { get; set; }
    }
}