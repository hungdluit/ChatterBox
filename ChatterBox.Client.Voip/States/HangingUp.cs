using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_HangingUp : BaseVoipState
    {
        private readonly string _peerId;

        public VoipState_HangingUp(string peerId)
        {
            _peerId = peerId;
        }

        public override void OnEnteringState()
        {
            Context.SendToPeer(_peerId, RelayMessageTags.VoipHangup, "");
            if (Context.PeerConnection != null)
            {
                Context.PeerConnection.Close();
                Context.PeerConnection = null;
            }
            Context.SwitchState(new VoipState_Idle());
        }
    }
}