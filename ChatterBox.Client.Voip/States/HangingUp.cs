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
        public VoipState_HangingUp(string peerId)
        {
            _peerId = peerId;
        }

        private string _peerId;

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
