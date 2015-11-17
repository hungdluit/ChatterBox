using System;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using webrtc_winrt_api;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class BaseVoipState : IVoipChannel
    {
        public VoipContext Context { get; private set; }

        #region IVoipChannel Members

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

        internal virtual void SendLocalIceCandidate(RTCIceCandidate candidate)
        {
        }
    }
}