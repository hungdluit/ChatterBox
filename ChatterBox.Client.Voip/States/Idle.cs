using System;
using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Universal.Background;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_Idle : BaseVoipState
    {
        public override void Call(OutgoingCallRequest request)
        {
            Context.SwitchState(new VoipState_RemoteRinging(request));
        }

        public override void OnEnteringState()
        {
            // Entering idle state.
            // Close the VoipTask so this process can end if needed.
            Hub.Instance.VoipTaskInstance?.CloseVoipTask();
        }

        public override void OnIncomingCall(RelayMessage message)
        {
            Context.SwitchState(new VoipState_LocalRinging(message));
        }

        public override async void OnLeavingState()
        {
            // Leaving the idle state means there's a call that's happening.
            // Trigger the VoipTask to prevent this background task from terminating.
            if (Hub.Instance.VoipTaskInstance == null)
            {
                var ret = await Hub.Instance.WebRtcTaskTrigger.RequestAsync();
                Debug.WriteLine($"VoipTask Trigger -> {ret}");
            }
        }
    }
}