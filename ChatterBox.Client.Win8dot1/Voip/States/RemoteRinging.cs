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
    internal class VoipState_RemoteRinging : BaseVoipState, IRemoteRinging
    {
        private readonly OutgoingCallRequest _request;

        public VoipState_RemoteRinging(OutgoingCallRequest request)
        {
            _request = request;
        }

        public override void Hangup()
        {
            var hangingUpState = Context.UnityContainer.Resolve<IHangingUp>();
            Context.SwitchState((BaseVoipState)hangingUpState);
        }

        public override void OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);

            Context.PeerId = _request.PeerUserId;

            Context.SendToPeer(RelayMessageTags.VoipCall, "");
            Context.InitializeWebRTC();
        }

        public override void OnOutgoingCallAccepted(RelayMessage message)
        {
            Context.SwitchState(new VoipState_EstablishOutgoing(_request));
        }

        public override void OnOutgoingCallRejected(RelayMessage message)
        {
            var idleState = Context.UnityContainer.Resolve<IIdle>();
            Context.SwitchState((BaseVoipState)idleState);
        }
    }
}
