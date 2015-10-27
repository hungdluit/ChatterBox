using System;
using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using System.Threading.Tasks;

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipChannel : IVoipChannel
    {
        // This variable should not be used outside of the getter below.
        private VoipContext _context;

        private VoipContext Context
        {
            get
            {
                // Create on demand.
                if (_context == null)
                    _context = new VoipContext();
                return _context;
            }
        }

        #region IVoipChannel Members

        public void Answer()
        {
            Debug.WriteLine("VoipChannel.Answer");
            Task.Run(() => Context.State.Answer());
        }

        public void Call(OutgoingCallRequest request)
        {
            Debug.WriteLine("VoipChannel.Call");
            Task.Run(() => Context.State.Call(request));
        }

        public VoipState GetVoipState()
        {
            return Context.GetVoipState();
        }

        // Hangup can happen on both sides
        public void Hangup()
        {
            Debug.WriteLine("VoipChannel.Hangup");
            Task.Run(() => Context.State.Hangup());
        }

        public void OnIceCandidate(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnIceCandidate");
            Task.Run(() => Context.State.OnIceCandidate(message));
        }

        // Remotely initiated calls
        public void OnIncomingCall(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnIncomingCall");
            Task.Run(() => Context.State.OnIncomingCall(message));
        }

        public void OnOutgoingCallAccepted(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnOutgoingCallAccepted");
            Task.Run(() => Context.State.OnOutgoingCallAccepted(message));
        }

        public void OnOutgoingCallRejected(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnOutgoingCallRejected");
            Task.Run(() => Context.State.OnOutgoingCallRejected(message));
        }

        public void OnRemoteHangup(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnRemoteHangup");
            Task.Run(() => Context.State.OnRemoteHangup(message));
        }

        // WebRTC signaling
        public void OnSdpAnswer(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnSdpAnswer");
            Task.Run(() => Context.State.OnSdpAnswer(message));
        }

        public void OnSdpOffer(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnSdpOffer");
            Task.Run(() => Context.State.OnSdpOffer(message));
        }

        public void Reject(IncomingCallReject reason)
        {
            Debug.WriteLine("VoipChannel.Reject");
            Task.Run(() => Context.State.Reject(reason));
        }

        #endregion
    }
}