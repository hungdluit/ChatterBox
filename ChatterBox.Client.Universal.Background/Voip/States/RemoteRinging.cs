using System;
using System.Diagnostics;
using Windows.ApplicationModel.Calls;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Voip.States.Interfaces;
using Microsoft.Practices.Unity;
using ChatterBox.Client.Universal.Background.Voip;
using ChatterBox.Common.Communication.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_RemoteRinging : BaseVoipState, IRemoteRinging
    {
        private readonly OutgoingCallRequest _request;
        private readonly VoipCallContext _voipCallContext;

        public VoipState_RemoteRinging(OutgoingCallRequest request, VoipCallContext voipCallContext)
        {
            _request = request;
            _voipCallContext = voipCallContext;
        }

        private void Call_EndRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            Hangup();
        }

        private void Call_HoldRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Call_RejectRequested(VoipPhoneCall sender, CallRejectEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Call_ResumeRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            throw new NotImplementedException();
        }

        public override void Hangup()
        {
            var hangingUpState = Context.UnityContainer.Resolve<IHangingUp>();
            Context.SwitchState((BaseVoipState)hangingUpState);
        }

        public override void OnRemoteHangup(RelayMessage message)
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

            var vCC = VoipCallCoordinator.GetDefault();
            var call = vCC.RequestNewOutgoingCall(_request.PeerUserId, _request.PeerUserId, "ChatterBox Universal",
                VoipPhoneCallMedia.Audio);
            if (call != null)
            {
                call.EndRequested += Call_EndRequested;
                call.HoldRequested += Call_HoldRequested;
                call.RejectRequested += Call_RejectRequested;
                call.ResumeRequested += Call_ResumeRequested;

                call.NotifyCallActive();

                _voipCallContext.VoipCall = call;
            }
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
