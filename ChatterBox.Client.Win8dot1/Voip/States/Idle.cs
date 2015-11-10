using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Voip.States.Interfaces;
using System;
using Microsoft.Practices.Unity;
using ChatterBox.Common.Communication.Messages.Relay;

namespace ChatterBox.Client.Win8dot1.Voip.States
{
    internal class VoipState_Idle : BaseVoipState, IIdle
    {
        public override void Call(OutgoingCallRequest request)
        {
            var remoteRingingState = Context.UnityContainer.Resolve<IRemoteRinging>(new ParameterOverride("request", request));
            Context.SwitchState((BaseVoipState)remoteRingingState);
        }

        public override void OnEnteringState()
        {
            // Entering idle state.

            // Make sure the context is sane.
            Context.PeerConnection = null;
            Context.PeerId = null;
        }

        public override void OnIncomingCall(RelayMessage message)
        {
            var localRingingState = Context.UnityContainer.Resolve<ILocalRinging>(new ParameterOverride("message", message));
            Context.SwitchState((BaseVoipState)localRingingState);
        }

        public override async void OnLeavingState()
        {
        }
    }
}
