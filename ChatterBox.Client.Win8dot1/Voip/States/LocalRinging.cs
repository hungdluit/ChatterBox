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

namespace ChatterBox.Client.Win8dot1.Voip.States
{
    internal class VoipState_LocalRinging : BaseVoipState, ILocalRinging
    {
        private readonly RelayMessage _message;

        public VoipState_LocalRinging(RelayMessage message)
        {
            _message = message;
        }

        public override void Answer()
        {
            Context.SendToPeer(RelayMessageTags.VoipAnswer, "");
            Context.SwitchState(new VoipState_EstablishIncoming());
        }

        public override void Hangup()
        {
            var hangingUpState = Context.UnityContainer.Resolve<IHangingUp>();
            Context.SwitchState((BaseVoipState)hangingUpState);
        }

        public override void OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);
            Context.PeerId = _message.FromUserId;
            Context.InitializeWebRTC();
        }

        public override void OnRemoteHangup(RelayMessage message)
        {
            var hangingUpState = Context.UnityContainer.Resolve<IHangingUp>();
            Context.SwitchState((BaseVoipState)hangingUpState);
        }

        public override void Reject(IncomingCallReject reason)
        {
            Context.SendToPeer(RelayMessageTags.VoipReject, "Rejected");
            var idleState = Context.UnityContainer.Resolve<IIdle>();
            Context.SwitchState((BaseVoipState)idleState);
        }
    }
}
