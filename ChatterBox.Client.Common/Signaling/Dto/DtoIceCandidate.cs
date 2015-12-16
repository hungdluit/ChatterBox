using System.Runtime.Serialization;

namespace ChatterBox.Client.Common.Signaling.Dto
{
    public sealed class DtoIceCandidate
    {
        [DataMember]
        public string Candidate { get; set; }

        [DataMember]
        public string SdpMid { get; set; }

        [DataMember]
        public ushort SdpMLineIndex { get; set; }
    }

    public sealed class DtoIceCandidates
    {
        [DataMember]
        public DtoIceCandidate[] Candidates { get; set; }
    }
}