using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using System.Diagnostics;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Common.Communication.Serialization;
using System.Threading;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_LocalRinging : BaseVoipState
    {
        private readonly RelayMessage _message;
        private readonly OutgoingCallRequest _callRequest;
        private Timer _callTimeout;
        private const int _callDueTimeout = 1000 * 35; //35 seconds, should be bigger than RemoteRinging state timer

        public VoipState_LocalRinging(RelayMessage message)
        {
            _message = message;
            _callRequest = (OutgoingCallRequest)JsonConvert.Deserialize(message.Payload, typeof(OutgoingCallRequest));            
        }

        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.LocalRinging;
            }
        }

        public override async Task Answer()
        {
            StopTimer();

            Context.SendToPeer(RelayMessageTags.VoipAnswer, "");

            var establishIncomingState = new VoipState_EstablishIncoming(_message);
            await Context.SwitchState(establishIncomingState);
        }

        public override async Task Hangup()
        {
            StopTimer();

            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);
            Context.PeerId = _message.FromUserId;
            Context.IsVideoEnabled = _callRequest.VideoEnabled;

            Context.VoipCoordinator.StartIncomingCall(_message);

            _callTimeout = new Timer(CallTimeoutCallback, null, _callDueTimeout, Timeout.Infinite);
        }

        public override async Task OnRemoteHangup(RelayMessage message)
        {
            StopTimer();
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task Reject(IncomingCallReject reason)
        {
            StopTimer();
            Context.SendToPeer(RelayMessageTags.VoipReject, "Rejected");

            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        private async void CallTimeoutCallback(object state)
        {
            await Hangup();
            StopTimer();
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
