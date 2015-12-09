using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Client.Common.Communication.Voip;
using System;

namespace ChatterBox.Client.Win8dot1.Voip
{
    internal class VoipCoordinator : IVoipCoordinator
    {
        public VoipContext Context { get; set; }

        public void NotifyCallActive()
        {
        }

        public void NotifyCallEnded()
        {
        }

        public void OnEnterHangingUp()
        {
        }

        public void OnEnterIdle()
        {
        }

        public void OnEnterLocalRinging(BaseVoipState currentState, RelayMessage message)
        {
        }

        public void OnEnterRemoteRinging(BaseVoipState currentState, OutgoingCallRequest request)
        {
        }

        public void OnLeavingIdle()
        {
        }

        public void OnOutgoingCallRejected()
        {
        }
    }
}
