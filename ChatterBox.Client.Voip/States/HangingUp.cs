using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_HangingUp : BaseVoipState
    {
        public override void OnEnteringState()
        {
            Context.SendToPeer(RelayMessageTags.VoipHangup, "");
            if (Context.PeerConnection != null)
            {
                Context.PeerConnection.Close();
                Context.PeerConnection = null;
                Context.PeerId = null;
            }

            if (Context.VoipCall != null)
            {
                Context.VoipCall.NotifyCallEnded();
            }
            Context.SwitchState(new VoipState_Idle());
        }
    }
}