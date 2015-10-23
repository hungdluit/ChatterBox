using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using webrtc_winrt_api;

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipChannel : IVoipChannel
    {
        public VoipChannel()
        {
            _context = new VoipContext();
        }

        public void Initialize()
        {
            webrtc_winrt_api.WebRTC.Initialize(null);
        }

        // TODO: Check RelayMessage FromUserId is the peer we're connected to.

        #region IVoipChannel implementation
        public void Call(OutgoingCallRequest request)
        {
            _context.State.Call(request);
        }
        public void OnOutgoingCallAccepted(RelayMessage message)
        {
            _context.State.OnOutgoingCallAccepted(message);
        }
        public void OnOutgoingCallRejected(RelayMessage message)
        {
            _context.State.OnOutgoingCallRejected(message);
        }

        // Remotely initiated calls
        public void OnIncomingCall(RelayMessage message)
        {
            _context.State.OnIncomingCall(message);
        }
        public void Answer()
        {
            _context.State.Answer();
        }
        public void Reject(IncomingCallReject reason)
        {
            _context.State.Reject(reason);
        }

        // Hangup can happen on both sides
        public void Hangup()
        {
            _context.State.Hangup();
        }
        public void OnRemoteHangup(RelayMessage message)
        {
            _context.State.OnRemoteHangup(message);
        }

        // WebRTC signaling
        public void OnSdpAnswer(RelayMessage message)
        {
            _context.State.OnSdpAnswer(message);
        }
        public void OnSdpOffer(RelayMessage message)
        {
            _context.State.OnSdpOffer(message);
        }
        public void OnIceCandidate(RelayMessage message)
        {
            _context.State.OnIceCandidate(message);
        }

        #endregion

        VoipContext _context;
    }

}
