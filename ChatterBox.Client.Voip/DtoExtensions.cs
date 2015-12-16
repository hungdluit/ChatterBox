using System.Linq;
using ChatterBox.Client.Common.Signaling.Dto;
using webrtc_winrt_api;

namespace ChatterBox.Client.Voip
{
    internal static class DtoExtensions
    {
        public static RTCIceCandidate FromDto(this DtoIceCandidate obj)
        {
            return new RTCIceCandidate
            {
                Candidate = obj.Candidate,
                SdpMid = obj.SdpMid,
                SdpMLineIndex = obj.SdpMLineIndex
            };
        }

        public static DtoIceCandidate ToDto(this RTCIceCandidate obj)
        {
            return new DtoIceCandidate
            {
                Candidate = obj.Candidate,
                SdpMid = obj.SdpMid,
                SdpMLineIndex = obj.SdpMLineIndex
            };
        }

        public static RTCIceCandidate[] FromDto(this DtoIceCandidates obj)
        {
            return obj.Candidates.Select(FromDto).ToArray();
        }

        public static DtoIceCandidates ToDto(this RTCIceCandidate[] obj)
        {
            return new DtoIceCandidates
            {
                Candidates = obj.Select(ToDto).ToArray()
            };
        }
    }
}
