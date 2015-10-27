using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using webrtc_winrt_api;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_RemoteRinging : BaseVoipState
    {
        public VoipState_RemoteRinging(OutgoingCallRequest request)
        {
            _request = request;
        }

        private OutgoingCallRequest _request;

        public override void OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);

            Context.PeerId = _request.PeerUserId;

            Context.SendToPeer(RelayMessageTags.VoipCall, "");
            Context.InitializeWebRTC();
            // TODO: Feedback on the UI
        }


        public override void OnOutgoingCallAccepted(RelayMessage message)
        {
            Context.SwitchState(new VoipState_EstablishOutgoing(_request));
        }

        public override void OnOutgoingCallRejected(RelayMessage message)
        {
            Context.SwitchState(new VoipState_Idle());
        }

        public override void Hangup()
        {
            Context.SwitchState(new VoipState_HangingUp());
        }
    }
}
