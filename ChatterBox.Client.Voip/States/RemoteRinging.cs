using System;
using System.Diagnostics;
using Windows.ApplicationModel.Calls;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_RemoteRinging : BaseVoipState
    {
        private readonly OutgoingCallRequest _request;

        public VoipState_RemoteRinging(OutgoingCallRequest request)
        {
            _request = request;
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
            Context.SwitchState(new VoipState_HangingUp());
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

                Context.VoipCall = call;
            }
        }

        public override void OnOutgoingCallAccepted(RelayMessage message)
        {
            Context.SwitchState(new VoipState_EstablishOutgoing(_request));
        }

        public override void OnOutgoingCallRejected(RelayMessage message)
        {
            Context.VoipCall.NotifyCallEnded();
            Context.SwitchState(new VoipState_Idle());
        }
    }
}