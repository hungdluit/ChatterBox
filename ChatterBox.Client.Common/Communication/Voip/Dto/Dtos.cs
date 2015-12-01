namespace ChatterBox.Client.Common.Communication.Voip.Dto
{
    public sealed class OutgoingCallRequest
    {
        public string PeerUserId { get; set; }
        public bool Video { get; set; }
    }

    public sealed class IncomingCallReject
    {
        public string Reason { get; set; }
    }
}