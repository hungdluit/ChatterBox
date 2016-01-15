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

    public sealed class MicrophoneConfig
    {
        public bool Muted { get; set; }
    }


    public sealed class TraceServerConfig
    {
        [DataMember]
        public string Ip { get; set; }

        [DataMember]
        public int Port { get; set; }
  }
   public sealed class VideoConfig
   {
        public bool On { get; set; }
   }
}