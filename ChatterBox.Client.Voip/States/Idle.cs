using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Voip.States.Interfaces;
using System;
using Microsoft.Practices.Unity;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Client.Common.Communication.Foreground.Dto;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_Idle : BaseVoipState
    {
        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.Idle;
            }
        }

        public override void Call(OutgoingCallRequest request)
        {
            var remoteRingingState = new VoipState_RemoteRinging(request);
            Context.SwitchState(remoteRingingState);
        }

        public override void OnEnteringState()
        {
            // Entering idle state.
            Context.VoipCoordinator.OnEnterIdle();

            // Make sure the context is sane.
            Context.PeerConnection = null;
            Context.PeerId = null;
        }

        public override void OnIncomingCall(RelayMessage message)
        {
            var localRingingState = new VoipState_LocalRinging(message);
            Context.SwitchState(localRingingState);
        }

        public override async void OnLeavingState()
        {
            Context.VoipCoordinator.OnLeavingIdle();
        }
    }
}
