using System;
using System.Diagnostics;
using Windows.ApplicationModel.Calls;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Voip.States.Interfaces;
using Microsoft.Practices.Unity;
using ChatterBox.Client.Universal.Background.Voip;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_RemoteRinging : BaseVoipState, IRemoteRinging
    {
        private readonly OutgoingCallRequest _request;

        public VoipState_RemoteRinging(OutgoingCallRequest request)
        {
            _request = request;
			IsVideoEnabled = request.VideoEnabled;
        }

		public override VoipStateEnum VoipStateEnum
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
            Context.SwitchState(new VoipState_EstablishOutgoing(_request));
        }

        public override void OnOutgoingCallRejected(RelayMessage message)
        {
            _voipCallContext.VoipCall.NotifyCallEnded();
            var idleState = Context.UnityContainer.Resolve<IIdle>();
            Context.SwitchState((BaseVoipState)idleState);
        }
    }
}
