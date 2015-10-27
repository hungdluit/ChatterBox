using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_RemoteRinging : BaseVoipState
    {
        private readonly OutgoingCallRequest _request;

        public VoipState_RemoteRinging(OutgoingCallRequest request)
        {
            _request = request;
        }

        public override void Hangup()
        {
            Context.SwitchState(new VoipState_HangingUp(_request.PeerUserId));
        }

        public override void OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);

            Context.SendToPeer(_request.PeerUserId, RelayMessageTags.VoipCall, "");
            Context.InitializeWebRTC();
            // TODO: Feedback on the UI
        }

        public override void OnOutgoingCallAccepted(RelayMessage message)
        {
            Context.SwitchState(new VoipState_EstablishOutgoing(_request));
        }

        public override void OnOutgoingCallRejected(RelayMessage message)
        {
            Context.SwitchState(new VoipState_Idle());
        }
    }
}