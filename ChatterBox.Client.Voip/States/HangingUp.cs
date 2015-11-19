using System;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Common.Communication.Messages.Relay;
using Microsoft.Practices.Unity;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_HangingUp : BaseVoipState
    {
        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.HangingUp;
            }
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

            Context.LocalStream?.Stop();
            Context.RemoteStream?.Stop();

            Context.VoipCoordinator.OnEnterHangingUp();

            var idleState = new VoipState_Idle();
            Context.SwitchState(idleState);
        }
    }
}
