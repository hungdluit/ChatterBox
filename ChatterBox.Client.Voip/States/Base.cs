using System;
using Windows.Graphics.Display;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using webrtc_winrt_api;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

#pragma warning disable 1998

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal abstract class BaseVoipState
    {
        public VoipContext Context { get; private set; }

        public abstract VoipStateEnum VoipState { get; }

        #region IVoipChannel Members

        public virtual async Task SetForegroundProcessId(uint processId)
        {
        }

        public virtual void DisplayOrientationChanged(DisplayOrientations orientation)
        {
        }

        public virtual async Task Answer()
        {
        }

        public virtual async Task Call(OutgoingCallRequest request)
        {
        }

        public Task<VoipState> GetVoipState()
        {
            // Should never be called.  VoipContext handles it.
            throw new NotImplementedException();
        }

        public virtual async Task Hangup()
        {
        }

        public virtual async Task OnIceCandidate(RelayMessage message)
        {
        }

        public virtual async Task OnIncomingCall(RelayMessage message)
        {
        }

        public virtual async Task OnOutgoingCallAccepted(RelayMessage message)
        {
        }

        public virtual async Task OnOutgoingCallRejected(RelayMessage message)
        {
        }

        public virtual async Task OnRemoteHangup(RelayMessage message)
        {
        }

        public virtual async Task OnSdpAnswer(RelayMessage message)
        {
        }

        public virtual async Task OnSdpOffer(RelayMessage message)
        {
        }

        public virtual async Task Reject(IncomingCallReject reason)
        {
        }

        #endregion

        public async Task EnterState(VoipContext context)
        {
            Context = context;
            await OnEnteringState();
        }

        public async Task LeaveState()
        {
            await OnLeavingState();
            Context = null;
        }

        public virtual async Task OnEnteringState()
        {
        }

        public virtual async Task OnLeavingState()
        {
        }

        public virtual async Task SendLocalIceCandidate(RTCIceCandidate candidate)
        {
        }

        internal virtual async Task OnAddStream(MediaStream stream)
        {
        }
    }
}