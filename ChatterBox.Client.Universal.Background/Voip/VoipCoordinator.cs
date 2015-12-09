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
using ChatterBox.Client.Common.Avatars;

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
                VoipCall.AnswerRequested += Call_AnswerRequested;

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

            var foregroundIsVisible = false;
            var state = Hub.Instance.ForegroundClient.GetForegroundState();
            if (state != null) foregroundIsVisible = state.IsForegroundVisible;

            var voipCallCoordinatorCc = VoipCallCoordinator.GetDefault();

            if (foregroundIsVisible)
            {
                VoipCall = voipCallCoordinatorCc.RequestNewOutgoingCall(_message.FromUserId, _message.FromName, "ChatterBox Universal",
                    VoipPhoneCallMedia.Audio);
                if (VoipCall != null)
                {
                    VoipCall.EndRequested += Call_EndRequested;
                    VoipCall.HoldRequested += Call_HoldRequested;
                    VoipCall.RejectRequested += Call_RejectRequested;
                    VoipCall.ResumeRequested += Call_ResumeRequested;

                    VoipCall.NotifyCallActive();
                }
            }
            else
            {
                VoipCall = voipCallCoordinatorCc.RequestNewIncomingCall(_message.FromUserId, _message.FromName, _message.FromName,
                    new Uri(AvatarLink.For(_message.FromAvatar), UriKind.RelativeOrAbsolute),
                    "ChatterBox Universal",
                    null,
                    "",
                    null,
                    VoipPhoneCallMedia.Audio,
                    new TimeSpan(0, 1, 20));

                if (VoipCall != null)
                {
                    VoipCall.AnswerRequested += Call_AnswerRequested;
                    VoipCall.EndRequested += Call_EndRequested;
                    VoipCall.HoldRequested += Call_HoldRequested;
                    VoipCall.RejectRequested += Call_RejectRequested;
                    VoipCall.ResumeRequested += Call_ResumeRequested;
                }
            }


        }

        public void OnEnterIdle()
        {
            if (VoipCall != null)
            {
                VoipCall.NotifyCallEnded();
                VoipCall = null;
            }

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
                    const int RTcTaskAlreadyRunningErrorCode = -2147024713;
                    if (ex.HResult == RTcTaskAlreadyRunningErrorCode)
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

        private async void Call_AnswerRequested(VoipPhoneCall sender, CallAnswerEventArgs args)
        {
            // TODO: Pass through VoipContext.WithState()
            await _currentState.Answer();
        }

        private async void Call_EndRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            // TODO: Pass through VoipContext.WithState()
            await _currentState.Hangup();
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
