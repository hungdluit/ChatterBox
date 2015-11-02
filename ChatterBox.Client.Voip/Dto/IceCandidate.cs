using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using webrtc_winrt_api;

namespace ChatterBox.Client.Voip.Dto
{
    [DataContract]
    internal sealed class DtoIceCandidate
    {
        public static DtoIceCandidate ToDto(RTCIceCandidate obj)
        {
            return new DtoIceCandidate
            {
                Candidate = obj.Candidate,
                SdpMid = obj.SdpMid,
                SdpMLineIndex = obj.SdpMLineIndex,
            };
        }

        public static RTCIceCandidate FromDto(DtoIceCandidate obj)
        {
            return new RTCIceCandidate
            {
                Candidate = obj.Candidate,
                SdpMid = obj.SdpMid,
                SdpMLineIndex = obj.SdpMLineIndex,
            };
        }

        [DataMember]
        public string Candidate { get; set; }
        [DataMember]
        public string SdpMid { get; set; }
        [DataMember]
        public ushort SdpMLineIndex { get; set; }
    }
}
