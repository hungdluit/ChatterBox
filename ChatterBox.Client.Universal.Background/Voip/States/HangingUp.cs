﻿using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Common.Communication.Messages.Relay;
using Microsoft.Practices.Unity;

namespace ChatterBox.Client.Universal.Background.Voip.States
{
    internal class VoipState_HangingUp : BaseVoipState, IHangingUp
    {
        public override VoipStateEnum VoipStateEnum
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

            if (_voipCallContext.VoipCall != null)
            {
                _voipCallContext.VoipCall.NotifyCallEnded();
                _voipCallContext.VoipCall = null;
            }

            Context.LocalStream?.Stop();
            Context.LocalStream = null;

            Context.RemoteStream?.Stop();
            Context.RemoteStream = null;

			Context.VoipCoordinator.OnEnterHangingUp();
			
            var idleState = new VoipState_Idle();
            Context.SwitchState(idleState);
        }
    }
}