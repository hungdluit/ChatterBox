using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Common.Communication.Messages.Relay;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatterBox.Client.Voip.States.Interfaces
{
    /// <summary>
    ///  Voip call coordinator only for win10 client.
    /// </summary>
    /// <remarks>
    /// On win8 the voip APIs are not available.
    /// </remarks>
    internal interface IVoipCoordinator
    {
        void OnEnterRemoteRinging(BaseVoipState currentState,
                             OutgoingCallRequest request);

        void OnEnterLocalRinging(BaseVoipState currentState,
                            RelayMessage message);

        void OnEnterIdle();

        void OnEnterHangingUp();

        void OnLeavingIdle();

        void OnOutgoingCallRejected();

        void NotifyCallActive();

        void NotifyCallEnded();

        VoipContext Context { get; set; }
    }
}
