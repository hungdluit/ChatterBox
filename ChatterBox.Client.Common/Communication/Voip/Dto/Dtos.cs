using System.Runtime.Serialization;

namespace ChatterBox.Client.Common.Communication.Voip.Dto
{
    public sealed class OutgoingCallRequest
    {
        [DataMember]
        public string PeerUserId { get; set; }

        [DataMember]
        public bool VideoEnabled { get; set; }

    }

    public sealed class IncomingCallReject
    {
        public string Reason { get; set; }
    }
}