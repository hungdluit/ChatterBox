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
    internal class VoipState_LocalRinging : BaseVoipState
    {
        public VoipState_LocalRinging(RelayMessage message)
        {
            _message = message;
        }

        RelayMessage _message;

        public override void OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);
            Context.PeerId = _message.FromUserId;
            Context.InitializeWebRTC();

            // TODO: Detect if UI is visible, and use an outgoing call if it is
            //       so there's not popup and we can answer on the UI.

            var vCC = VoipCallCoordinator.GetDefault();
            VoipPhoneCall call = vCC.RequestNewIncomingCall(
                _message.FromUserId, _message.FromName, _message.FromUserId,
                null,
                "ChatterBox Universal",
                null,
                "",
                null,
                VoipPhoneCallMedia.Audio,
                new TimeSpan(0, 1, 20));

            if (call != null)
            {
                call.AnswerRequested += Call_AnswerRequested;
                call.EndRequested += Call_EndRequested;
                call.HoldRequested += Call_HoldRequested;
                call.RejectRequested += Call_RejectRequested;
                call.ResumeRequested += Call_ResumeRequested;

                Context.VoipCall = call;
            }
        }

        private void Call_ResumeRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Call_RejectRequested(VoipPhoneCall sender, CallRejectEventArgs args)
        {
            Reject(new IncomingCallReject { Reason = args.RejectReason.ToString() });
        }

        private void Call_HoldRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Call_EndRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            Hangup();
        }

        private void Call_AnswerRequested(VoipPhoneCall sender, CallAnswerEventArgs args)
        {
            Answer();
        }

        public override void Answer()
        {
            Context.VoipCall.NotifyCallActive();
            Context.SendToPeer(RelayMessageTags.VoipAnswer, "");
            Context.SwitchState(new VoipState_EstablishIncoming());
        }

        public override void Reject(IncomingCallReject reason)
        {
            Context.SendToPeer(RelayMessageTags.VoipReject, "Rejected");
            Context.SwitchState(new VoipState_Idle());
        }

        public override void Hangup()
        {
            Context.SwitchState(new VoipState_HangingUp());
        }

        public override void OnRemoteHangup(RelayMessage message)
        {
            Context.SwitchState(new VoipState_HangingUp());
        }
    }
}
