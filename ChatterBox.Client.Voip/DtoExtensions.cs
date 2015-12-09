using ChatterBox.Client.Common.Signaling.Dto;
using webrtc_winrt_api;

namespace ChatterBox.Client.Voip
{
    internal static class DtoExtensions
    {
        public static RTCIceCandidate FromDto(DtoIceCandidate obj)
        {
            return new RTCIceCandidate
            {
                Candidate = obj.Candidate,
                SdpMid = obj.SdpMid,
                SdpMLineIndex = obj.SdpMLineIndex
            };
        }

        public static DtoIceCandidate ToDto(RTCIceCandidate obj)
        {
            return new DtoIceCandidate
            {
                Candidate = obj.Candidate,
                SdpMid = obj.SdpMid,
                SdpMLineIndex = obj.SdpMLineIndex
            };
        }
    }
}
