using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Universal.Background.Tasks;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Common.Communication.Messages.Relay;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Foundation.Metadata;
using ChatterBox.Client.Common.Avatars;

namespace ChatterBox.Client.Universal.Background.Voip
{
    internal class VoipCoordinator : IVoipCoordinator
    {
        public void StartOutgoingCall(OutgoingCallRequest request)
        {
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

        public void StartIncomingCall(RelayMessage message)
        {

            Debug.WriteLine("GetForegroundState");
            var foregroundIsVisible = false;
            var state = Hub.Instance.ForegroundClient.GetForegroundState();
            if (state != null) foregroundIsVisible = state.IsForegroundVisible;

            var voipCallCoordinatorCc = VoipCallCoordinator.GetDefault();

            if (foregroundIsVisible)
            {
                VoipCall = voipCallCoordinatorCc.RequestNewOutgoingCall(message.FromUserId, message.FromName, "ChatterBox Universal",
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
                VoipCall = voipCallCoordinatorCc.RequestNewIncomingCall(message.FromUserId, message.FromName, message.FromName,
                    new Uri(AvatarLink.For(message.FromAvatar), UriKind.RelativeOrAbsolute),
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

        public void StopVoip()
        {
            if (VoipCall != null)
            {
                VoipCall.NotifyCallEnded();
                VoipCall = null;
            }

            Hub.Instance.VoipTaskInstance?.CloseVoipTask();
        }

        public async Task StartVoipTask()
        {
            // Leaving the idle state means there's a call that's happening.
            // Trigger the VoipTask to prevent this background task from terminating.
#if true // VoipCallCoordinator support
            if (Hub.Instance.VoipTaskInstance == null &&
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

        private void Call_AnswerRequested(VoipPhoneCall sender, CallAnswerEventArgs args)
        {
            VoipCall.NotifyCallActive();
            Hub.Instance.VoipChannel.Answer();
        }

        private void Call_EndRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            Hub.Instance.VoipChannel.Hangup();
        }

        private void Call_HoldRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Call_RejectRequested(VoipPhoneCall sender, CallRejectEventArgs args)
        {
            Hub.Instance.VoipChannel.Reject(new IncomingCallReject() {
                Reason = "Rejected"
            });
        }

        private void Call_ResumeRequested(VoipPhoneCall sender, CallStateChangeEventArgs args)
        {
            throw new NotImplementedException();
        }

        public VoipPhoneCall VoipCall { get; set; }
    }
}
