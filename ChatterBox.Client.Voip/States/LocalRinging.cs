using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using webrtc_winrt_api;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_LocalRinging : BaseVoipState
    {
        public VoipState_LocalRinging(RelayMessage message)
        {
            _peerId = message.FromUserId;
        }

        private string _peerId;

        public override void OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);
            Context.InitializeWebRTC();

            // TODO: Visual ringing on the UI
        }

        public override void Answer()
        {
            Context.SendToPeer(_peerId, RelayMessageTags.VoipAnswer, "");
            Context.SwitchState(new VoipState_EstablishIncoming(_peerId));
        }

        public override void Reject(IncomingCallReject reason)
        {
            Context.SendToPeer(_peerId, RelayMessageTags.VoipReject, "Rejected");
            Context.SwitchState(new VoipState_Idle());
        }
    }
}
