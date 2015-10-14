namespace ChatterBox.Common.Communication.Messages.Peers
{
    public sealed class PeerData
    {
        public int Avatar { get; set; }
        public bool IsOnline { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
    }
}