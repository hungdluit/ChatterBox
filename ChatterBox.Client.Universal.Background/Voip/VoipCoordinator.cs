using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Universal.Background.Tasks;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Common.Communication.Messages.Relay;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Calls;
using Windows.Foundation.Metadata;

namespace ChatterBox.Client.Universal.Background.Voip
{
    internal class VoipCoordinator : IVoipCoordinator
    {
        private Hub _hub;

        public VoipCoordinator(Hub hub)
        {
            _hub = hub;
        }

        public void OnEnterRemoteRinging(BaseVoipState currentState,
                                    OutgoingCallRequest request)
        {
            _currentState = currentState;

            var vCC = VoipCallCoordinator.GetDefault();
            VoipCall = vCC.RequestNewOutgoingCall(request.PeerUserId, request.PeerUserId, "ChatterBox Universal",
                VoipPhoneCallMedia.Audio);
            if (VoipCall != null)
            {
                VoipCall.EndRequested += Call_EndRequested;
                VoipCall.HoldRequested += Call_HoldRequested;
                VoipCall.RejectRequested += Call_RejectRequested;
                VoipCall.ResumeRequested += Call_ResumeRequested;
                VoipCall.AnswerRequested += VoipCall_AnswerRequested;

                VoipCall.NotifyCallActive();
            }
        }

        public void OnEnterLocalRinging(BaseVoipState currentState,
                                   RelayMessage message)
        {
            Debug.Assert(Context.PeerConnection == null);
            _currentState = currentState;
            _message = message;
            Context.PeerId = _message.FromUserId;

#if true // Temporary workaround for loss of connection between FG/BG.  Don't use OS call prompt.

            // TODO: Detect if UI is visible, and use an outgoing call if it is
            //       so there's not popup and we can answer on the UI.
            var vCC = VoipCallCoordinator.GetDefault();
            VoipCall = vCC.RequestNewOutgoingCall(_message.FromUserId, _message.FromUserId, "ChatterBox Universal",
                VoipPhoneCallMedia.Audio);
            if (VoipCall != null)
            {
                VoipCall.EndRequested += Call_EndRequested;
                VoipCall.HoldRequested += Call_HoldRequested;
                VoipCall.RejectRequested += Call_RejectRequested;
                VoipCall.ResumeRequested += Call_ResumeRequested;

                VoipCall.NotifyCallActive();
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

        public void OnEnterIdle()
        {
            _hub.VoipTaskInstance?.CloseVoipTask();
        }

        public async void OnLeavingIdle()
        {
            // Leaving the idle state means there's a call that's happening.
            // Trigger the VoipTask to prevent this background task from terminating.
#if true // VoipCallCoordinator support
            if (_hub.VoipTaskInstance == null &&
                ApiInformation.IsApiContractPresent("Windows.ApplicationModel.Calls.CallsVoipContract", 1))
            {
                var vcc = VoipCallCoordinator.GetDefault();
                var voipEntryPoint = typeof(VoipTask).FullName;
                Debug.WriteLine($"ReserveCallResourcesAsync {voipEntryPoint}");
                try
                {
                    var status = await vcc.ReserveCallResourcesAsync(voipEntryPoint);
                    Debug.WriteLine($"ReserveCallResourcesAsync result -> {status}");
                }
                catch (Exception ex)
                {
                    const int RTcTaskAlreadyRuningErrorCode = -2147024713;
                    if (ex.HResult == RTcTaskAlreadyRuningErrorCode)
                    {
                        Debug.WriteLine("VoipTask already running");
                    }
                    else
                    {
                        Debug.WriteLine($"ReserveCallResourcesAsync error -> {ex.HResult} : {ex.Message}");
                    }
                }
            }
#else
            var ret = await _hub.WebRtcTaskTrigger.RequestAsync();
            Debug.WriteLine($"VoipTask Trigger -> {ret}");
#endif
        }

        public void OnEnterHangingUp()
        {
            if (VoipCall != null)
            {
                VoipCall.NotifyCallEnded();
                VoipCall = null;
            }
        }

        private void VoipCall_AnswerRequested(VoipPhoneCall sender, CallAnswerEventArgs args)
        {
            _currentState.Answer();
        }

        private void Call_EndRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            _currentState.Hangup();
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

        public void OnOutgoingCallRejected()
        {
            VoipCall.NotifyCallEnded();
        }

        public void NotifyCallActive()
        {
            VoipCall.NotifyCallActive();
        }

        public void NotifyCallEnded()
        {
            VoipCall.NotifyCallEnded();
        }

        public VoipContext Context { get; set; }
        public VoipPhoneCall VoipCall { get; set; }
        private BaseVoipState _currentState { get; set; }
        private RelayMessage _message { get; set; }
    }
}
