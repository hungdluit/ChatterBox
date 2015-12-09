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
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_LocalRinging : BaseVoipState
    {
        private readonly RelayMessage _message;
        private readonly OutgoingCallRequest _callRequest;

        public VoipState_LocalRinging(RelayMessage message)
        {
            _message = message;
            _callRequest = (OutgoingCallRequest)JsonConvert.Deserialize(message.Payload, typeof(OutgoingCallRequest));            
        }

        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.LocalRinging;
            }
        }

        public override async Task Answer()
        {
            Context.VoipCoordinator.NotifyCallActive();
            Context.SendToPeer(RelayMessageTags.VoipAnswer, "");

            var establishIncomingState = new VoipState_EstablishIncoming(_message);
            await Context.SwitchState(establishIncomingState);
        }

        public override async Task Hangup()
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);
            Context.PeerId = _message.FromUserId;
            Context.IsVideoEnabled = _callRequest.VideoEnabled;

            Context.VoipCoordinator.OnEnterLocalRinging(_message);
        }

        public override async Task OnRemoteHangup(RelayMessage message)
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task Reject(IncomingCallReject reason)
        {
            Context.SendToPeer(RelayMessageTags.VoipReject, "Rejected");
            Context.VoipCoordinator.NotifyCallEnded();

            var idleState = new VoipState_Idle();
            await Context.SwitchState(idleState);
        }
    }
}
