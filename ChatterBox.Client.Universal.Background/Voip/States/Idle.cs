using System;
using System.Diagnostics;
using Windows.ApplicationModel.Calls;
using Windows.Foundation.Metadata;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Universal.Background.Tasks;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Voip.States.Interfaces;
using Microsoft.Practices.Unity;
using ChatterBox.Common.Communication.Messages.Relay;

namespace ChatterBox.Client.Universal.Background.Voip.States
{
    internal class VoipState_Idle : BaseVoipState, IIdle
    {
        public override void Call(OutgoingCallRequest request)
        {
            var remoteRingingState = Context.UnityContainer.Resolve<IRemoteRinging>(new ParameterOverride("request", request));
            Context.SwitchState((BaseVoipState)remoteRingingState);
        }

        public override void OnEnteringState()
        {
            // Entering idle state.
            // Close the VoipTask so this process can end if needed.
            Hub.Instance.VoipTaskInstance?.CloseVoipTask();

            // Make sure the context is sane.
            Context.PeerConnection = null;
            Context.PeerId = null;
        }

        static private webrtc_winrt_api.MediaStream _stream;

        public override void OnIncomingCall(RelayMessage message)
        {
            var localRingingState = Context.UnityContainer.Resolve<ILocalRinging>(new ParameterOverride("message", message));
            Context.SwitchState((BaseVoipState)localRingingState);
        }

        public override async void OnLeavingState()
        {
            // Leaving the idle state means there's a call that's happening.
            // Trigger the VoipTask to prevent this background task from terminating.
#if true // VoipCallCoordinator support
            if (Hub.Instance.VoipTaskInstance == null &&
                ApiInformation.IsApiContractPresent("Windows.ApplicationModel.Calls.CallsVoipContract", 1))
            {
                var vcc = VoipCallCoordinator.GetDefault();
                var voipEntryPoint = typeof (VoipTask).FullName;
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
            var ret = await Hub.Instance.WebRtcTaskTrigger.RequestAsync();
            Debug.WriteLine($"VoipTask Trigger -> {ret}");
#endif
        }
    }
}
