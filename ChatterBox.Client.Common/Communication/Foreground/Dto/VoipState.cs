namespace ChatterBox.Client.Common.Communication.Foreground.Dto
{
    public enum VoipStateEnum
    {
        Idle,
        LocalRinging,
        RemoteRinging,
        EstablishOutgoing,
        EstablishIncoming,
        HangingUp,
        ActiveCall
    }

    public sealed class VoipState
    {
        public bool HasPeerConnection { get; set; }
        public string PeerId { get; set; }
        public VoipStateEnum State { get; set; }
    }
}