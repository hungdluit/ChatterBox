using System;
using System.Collections.Generic;
using System.Text;
using webrtc_winrt_api;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class BaseVoipState : IVoipChannel
    {
        public BaseVoipState()
        {
        }

        public void EnterState(VoipContext context)
        {
            Context = context;
            OnEnteringState();
        }

        public virtual void OnEnteringState() { }

        public void LeaveState()
        {
            OnLeavingState();
            Context = null;
        }

        public virtual void OnLeavingState() { }

        #region IVoipChannel implementation
        public virtual void Call(OutgoingCallRequest request) { }

        public virtual void OnOutgoingCallAccepted(RelayMessage message) { }

        public virtual void OnOutgoingCallRejected(RelayMessage message) { }

        public virtual void OnIncomingCall(RelayMessage message) { }

        public virtual void Answer() { }

        public virtual void Reject(IncomingCallReject reason) { }

        public virtual void Hangup() { }

        public virtual void OnRemoteHangup(RelayMessage message) { }

        public virtual void OnSdpAnswer(RelayMessage message) { }

        public virtual void OnSdpOffer(RelayMessage message) { }

        public virtual void OnIceCandidate(RelayMessage message) { }
        #endregion

        internal virtual void SendLocalIceCandidate(RTCIceCandidate candidate) { }

        public VoipContext Context { get; private set; }
    }
}
