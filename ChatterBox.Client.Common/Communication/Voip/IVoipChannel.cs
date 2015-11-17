using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip
{
    public interface IVoipChannel
    {
        void Answer();
        // Locally initiated calls
        void Call(OutgoingCallRequest request);
        VoipState GetVoipState();
        // Hangup can happen on both sides
        void Hangup();
        void OnIceCandidate(RelayMessage message);
        // Remotely initiated calls
        void OnIncomingCall(RelayMessage message);
        void OnOutgoingCallAccepted(RelayMessage message);
        void OnOutgoingCallRejected(RelayMessage message);
        void OnRemoteHangup(RelayMessage message);
        // WebRTC signaling
        void OnSdpAnswer(RelayMessage message);
        void OnSdpOffer(RelayMessage message);
        void Reject(IncomingCallReject reason);
    }
}