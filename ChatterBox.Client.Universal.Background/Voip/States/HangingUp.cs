using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Common.Communication.Messages.Relay;
using Microsoft.Practices.Unity;

namespace ChatterBox.Client.Universal.Background.Voip.States
{
    internal class VoipState_HangingUp : BaseVoipState, IHangingUp
    {
        private VoipCallContext _voipCallContext;

        public VoipState_HangingUp(VoipCallContext voipCallContext)
        {
            _voipCallContext = voipCallContext;
        }

        public override void OnEnteringState()
        {
            Context.SendToPeer(RelayMessageTags.VoipHangup, "");
            if (Context.PeerConnection != null)
            {
                Context.PeerConnection.Close();
                Context.PeerConnection = null;
                Context.PeerId = null;
            }

            if (_voipCallContext.VoipCall != null)
            {
                _voipCallContext.VoipCall.NotifyCallEnded();
                _voipCallContext.VoipCall = null;
            }
            var idleState = Context.UnityContainer.Resolve<IIdle>();
            Context.ResetRenderers();
            Context.SwitchState((BaseVoipState)idleState);
        }
    }
}