﻿using ChatterBox.Client.Common.Communication.Voip.Dto;
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
        }

        // TODO: Check RelayMessage FromUserId is the peer we're connected to.

        #region IVoipChannel implementation
        public void Call(OutgoingCallRequest request)
        {
            Debug.WriteLine("VoipChannel.Call");
            Context.State.Call(request);
        }
        public void OnOutgoingCallAccepted(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnOutgoingCallAccepted");
            Context.State.OnOutgoingCallAccepted(message);
        }
        public void OnOutgoingCallRejected(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnOutgoingCallRejected");
            Context.State.OnOutgoingCallRejected(message);
        }

        // Remotely initiated calls
        public void OnIncomingCall(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnIncomingCall");
            Context.State.OnIncomingCall(message);
        }
        public void Answer()
        {
            Debug.WriteLine("VoipChannel.Answer");
            Context.State.Answer();
        }
        public void Reject(IncomingCallReject reason)
        {
            Debug.WriteLine("VoipChannel.Reject");
            Context.State.Reject(reason);
        }

        // Hangup can happen on both sides
        public void Hangup()
        {
            Debug.WriteLine("VoipChannel.Hangup");
            Context.State.Hangup();
        }
        public void OnRemoteHangup(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnRemoteHangup");
            Context.State.OnRemoteHangup(message);
        }

        // WebRTC signaling
        public void OnSdpAnswer(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnSdpAnswer");
            Context.State.OnSdpAnswer(message);
        }
        public void OnSdpOffer(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnSdpOffer");
            Context.State.OnSdpOffer(message);
        }
        public void OnIceCandidate(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnIceCandidate");
            Context.State.OnIceCandidate(message);
        }

        #endregion

        // This variable should not be used outside of the getter below.
        VoipContext _context;
        VoipContext Context
        {
            get
            {
                // Create on demand.
                if (_context == null)
                    _context = new VoipContext();
                return _context;
            }
        }

    }

}