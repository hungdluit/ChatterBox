using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip
{
    public interface IVoipChannel
    {
        // Locally initiated calls
        void Call(OutgoingCallRequest request);
        void OnOutgoingCallAccepted(RelayMessage message);
        void OnOutgoingCallRejected(RelayMessage message);

        // Remotely initiated calls
        void OnIncomingCall(RelayMessage message);
        void Answer();
        void Reject(IncomingCallReject reason);

        // Hangup can happen on both sides
        void Hangup();
        void OnRemoteHangup(RelayMessage message);

        // WebRTC signaling
        void OnSdpAnswer(RelayMessage message);
        void OnSdpOffer(RelayMessage message);
        void OnIceCandidate(RelayMessage message);
    }
}
