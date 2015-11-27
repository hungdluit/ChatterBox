using System;
using System.Diagnostics;
using Windows.ApplicationModel.Calls;
using ChatterBox.Client.Common.Avatars;
using Microsoft.Practices.Unity;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Client.Universal.Background.Voip;
using ChatterBox.Common.Communication.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_LocalRinging : BaseVoipState, ILocalRinging
    {
        private readonly RelayMessage _message;
        private readonly VoipCallContext _voipCallContext;

        public VoipState_LocalRinging(RelayMessage message, VoipCallContext voipCallContext)
        {
            _message = message;
            _voipCallContext = voipCallContext;
        }

        public override void Answer()
        {
            _voipCallContext.VoipCall.NotifyCallActive();
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
            var hangingUpState = Context.UnityContainer.Resolve<IHangingUp>();
            Context.SwitchState((BaseVoipState)hangingUpState);
        }

        public override void OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);
            Context.PeerId = _message.FromUserId;
            Context.InitializeWebRTC();

#if true // Temporary workaround for loss of connection between FG/BG.  Don't use OS call prompt.

            // TODO: Detect if UI is visible, and use an outgoing call if it is
            //       so there's not popup and we can answer on the UI.
            var vCC = VoipCallCoordinator.GetDefault();
            var call = vCC.RequestNewOutgoingCall(_message.FromUserId, _message.FromName, "ChatterBox Universal",
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
#else

            var vCC = VoipCallCoordinator.GetDefault();
            var call = vCC.RequestNewIncomingCall(
                _message.FromUserId, _message.FromName, _message.FromUserId,
                new Uri(AvatarLink.For(_message.FromAvatar), UriKind.RelativeOrAbsolute),
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
#endif
        }

        public override void OnRemoteHangup(RelayMessage message)
        {
            var hangingUpState = Context.UnityContainer.Resolve<IHangingUp>();
            Context.SwitchState((BaseVoipState)hangingUpState);
        }

        public override void Reject(IncomingCallReject reason)
        {
            Context.SendToPeer(RelayMessageTags.VoipReject, "Rejected");
            _voipCallContext.VoipCall.NotifyCallEnded();
            var idleState = Context.UnityContainer.Resolve<IIdle>();
            Context.SwitchState((BaseVoipState)idleState);
        }
    }
}
