using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Server
{
    public class RegisteredClientMessageQueueItem
    {
        public string SerializedMessage { get; set; }
        public string Method { get; set; }

        public IMessage Message { get; set; }

        public bool IsDelivered { get; set; }
        public bool IsSent { get; set; }
    }
}