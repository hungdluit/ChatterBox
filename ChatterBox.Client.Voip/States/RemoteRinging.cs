using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Common.Communication.Messages.Relay;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Signaling.Dto;
using ChatterBox.Common.Communication.Serialization;
using System.Threading;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_RemoteRinging : BaseVoipState
    {
        private readonly OutgoingCallRequest _request;
        private Timer _callTimeout;
        private const int _callDueTimeout = 1000 * 30; // 30 seconds

        public VoipState_RemoteRinging(OutgoingCallRequest request)
        {
            _request = request;
        }

        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.RemoteRinging;
            }
        }

        public override async Task Hangup()
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task OnRemoteHangup(RelayMessage message)
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);

            Context.PeerId = _request.PeerUserId;

            Context.IsVideoEnabled = _request.VideoEnabled;

            var payload = JsonConvert.Serialize(_request);

            Context.SendToPeer(RelayMessageTags.VoipCall, payload);

            Context.VoipCoordinator.StartOutgoingCall(_request);

            _callTimeout = new Timer(CallTimeoutCallback, null, 30000, Timeout.Infinite);
        }

        public override async Task OnOutgoingCallAccepted(RelayMessage message)
        {
            var establishOutgoingState = new VoipState_EstablishOutgoing(_request);
            await Context.SwitchState(establishOutgoingState);
        }

        public override async Task OnOutgoingCallRejected(RelayMessage message)
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        private async void CallTimeoutCallback(object state)
        {
            if (Context != null)
            {
                await Hangup();
            }
            else
            {
                StopTimer();
            }
        }

        public override Task OnLeavingState()
        {
            StopTimer();
            return base.OnLeavingState();
        }

        private void StopTimer()
        {
            if (_callTimeout != null)
            {
                _callTimeout.Dispose();
                _callTimeout = null;
            }
        }
    }
}
