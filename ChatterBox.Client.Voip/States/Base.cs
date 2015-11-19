using System;
using Windows.Graphics.Display;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using webrtc_winrt_api;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal abstract class BaseVoipState : IVoipChannel
    {
        public VoipContext Context { get; private set; }

        public abstract VoipStateEnum VoipState { get; }

        #region IVoipChannel Members

        public virtual void SetForegroundProcessId(uint processId)
        {
        }

        public virtual void DisplayOrientationChanged(DisplayOrientations orientation)
        {
        }

        public virtual void Answer()
        {
        }

        public virtual void Call(OutgoingCallRequest request)
        {
        }

        public VoipState GetVoipState()
        {
            // Should never be called.  VoipContext handles it.
            throw new NotImplementedException();
        }

        public virtual void Hangup()
        {
        }

        public virtual void OnIceCandidate(RelayMessage message)
        {
        }

        public virtual void OnIncomingCall(RelayMessage message)
        {
        }

        public virtual void OnOutgoingCallAccepted(RelayMessage message)
        {
        }

        public virtual void OnOutgoingCallRejected(RelayMessage message)
        {
        }

        public virtual void OnRemoteHangup(RelayMessage message)
        {
        }

        public virtual void OnSdpAnswer(RelayMessage message)
        {
        }

        public virtual void OnSdpOffer(RelayMessage message)
        {
        }

        public virtual void Reject(IncomingCallReject reason)
        {
        }

        #endregion

        public void EnterState(VoipContext context)
        {
            Context = context;
            OnEnteringState();
        }

        public void LeaveState()
        {
            OnLeavingState();
            Context = null;
        }

        public virtual void OnEnteringState()
        {
        }

        public virtual void OnLeavingState()
        {
        }

        public virtual void SendLocalIceCandidate(RTCIceCandidate candidate)
        {
        }

        internal virtual void OnAddStream(MediaStream stream)
        {
        }

        //internal virtual void UpdateSwapChainHandle(long handle, bool local)
        //{
        //}
		
        public void RegisterVideoElements(MediaElement self, MediaElement peer)
        {
            throw new NotImplementedException();
        }

        public bool IsVideoEnabled { get; set; }
    }
}