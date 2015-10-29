using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using webrtc_winrt_api;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_HangingUp : BaseVoipState
    {
        public VoipState_HangingUp()
        {
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

            if (Context.VoipCall != null)
            {
                Context.VoipCall.NotifyCallEnded();
            }
            Context.SwitchState(new VoipState_Idle());
        }
    }
}
