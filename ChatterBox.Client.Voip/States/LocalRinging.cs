using System;
using System.Diagnostics;
using Windows.ApplicationModel.Calls;
using ChatterBox.Client.Common.Avatars;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_LocalRinging : BaseVoipState
    {
        private readonly RelayMessage _message;

        public VoipState_LocalRinging(RelayMessage message)
        {
            _message = message;
        }

        public override void Answer()
        {
            Context.VoipCall.NotifyCallActive();
            Context.SendToPeer(RelayMessageTags.VoipAnswer, "");
            Context.SwitchState(new VoipState_EstablishIncoming());
        }

        private void Call_AnswerRequested(VoipPhoneCall sender, CallAnswerEventArgs args)
        {
            Answer();
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
            Reject(new IncomingCallReject {Reason = args.RejectReason.ToString()});
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
            Context.PeerId = _message.FromUserId;
            Context.InitializeWebRTC();

            // TODO: Detect if UI is visible, and use an outgoing call if it is
            //       so there's not popup and we can answer on the UI.

            var vCC = VoipCallCoordinator.GetDefault();
            var call = vCC.RequestNewIncomingCall(
                _message.FromUserId, _message.FromName, _message.FromUserId,
                new Uri(AvatarLink.For(_message.FromAvatar), UriKind.RelativeOrAbsolute	),
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

        public override void OnRemoteHangup(RelayMessage message)
        {
            Context.SwitchState(new VoipState_HangingUp());
        }

        public override void Reject(IncomingCallReject reason)
        {
            Context.SendToPeer(RelayMessageTags.VoipReject, "Rejected");
            Context.SwitchState(new VoipState_Idle());
        }
    }
}