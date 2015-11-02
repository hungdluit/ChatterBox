using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using webrtc_winrt_api;
using Windows.ApplicationModel.Calls;

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

            var vCC = VoipCallCoordinator.GetDefault();
            VoipPhoneCall call = vCC.RequestNewOutgoingCall(_request.PeerUserId, _request.PeerUserId, "ChatterBox Universal", VoipPhoneCallMedia.Audio);
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

        private void Call_ResumeRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Call_RejectRequested(VoipPhoneCall sender, CallRejectEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Call_HoldRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Call_EndRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            Hangup();
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
