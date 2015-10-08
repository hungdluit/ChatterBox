using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Server
{
    public class RegisteredClientMessageQueueItem
    {
        public bool IsDelivered { get; set; }
        public bool IsSent { get; set; }
        public IMessage Message { get; set; }
        public string Method { get; set; }
        public string SerializedMessage { get; set; }
    }
}