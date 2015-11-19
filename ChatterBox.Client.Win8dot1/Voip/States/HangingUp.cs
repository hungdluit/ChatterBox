using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Common.Communication.Messages.Relay;
using Microsoft.Practices.Unity;

namespace ChatterBox.Client.Win8dot1.Voip.States
{
    internal class VoipState_HangingUp : BaseVoipState, IHangingUp
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

            var idleState = Context.UnityContainer.Resolve<IIdle>();
            Context.SwitchState((BaseVoipState)idleState);
        }
    }
}
