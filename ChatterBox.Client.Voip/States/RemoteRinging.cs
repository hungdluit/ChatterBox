using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Common.Communication.Messages.Relay;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Signaling.Dto;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_RemoteRinging : BaseVoipState
    {
        private readonly OutgoingCallRequest _request;

        public VoipState_RemoteRinging(OutgoingCallRequest request)
        {
            _request = request;
            IsVideoEnabled = request.VideoEnabled;
        }

        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.RemoteRinging;
            }
        }

        public override void Hangup()
        {
            var hangingUpState = new VoipState_HangingUp();
            Context.SwitchState(hangingUpState);
        }

        public override void OnRemoteHangup(RelayMessage message)
        {
            var hangingUpState = new VoipState_HangingUp();
            Context.SwitchState(hangingUpState);
        }

        public override void OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);

            Context.PeerId = _request.PeerUserId;

            var payload = JsonConvert.Serialize(_request);

            Context.SendToPeer(RelayMessageTags.VoipCall, payload);

            Context.VoipCoordinator.OnEnterRemoteRinging(this, _request);
        }

        public override void OnOutgoingCallAccepted(RelayMessage message)
        {
            var establishOutgoingState = new VoipState_EstablishOutgoing(_request);
            Context.SwitchState(establishOutgoingState);
        }

        public override void OnOutgoingCallRejected(RelayMessage message)
        {
            Context.VoipCoordinator.OnOutgoingCallRejected();

            var idleState = new VoipState_Idle();
            Context.SwitchState(idleState);
        }
    }
}
