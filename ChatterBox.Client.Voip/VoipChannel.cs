using System;
using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using System.Threading.Tasks;
using System.Threading;

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
            Post(() => Context.State.Answer());
        }

        public void Call(OutgoingCallRequest request)
        {
            Debug.WriteLine("VoipChannel.Call");
            Post(() => Context.State.Call(request));
        }

        public VoipState GetVoipState()
        {
            _sem.Wait();
            try
            {
                return Context.GetVoipState();
            }
            finally
            {
                _sem.Release();
            }
        }

        // Hangup can happen on both sides
        public void Hangup()
        {
            Debug.WriteLine("VoipChannel.Hangup");
            Post(() => Context.State.Hangup());
        }

        public void OnIceCandidate(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnIceCandidate");
            Post(() => Context.State.OnIceCandidate(message));
        }

        // Remotely initiated calls
        public void OnIncomingCall(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnIncomingCall");
            Post(() => Context.State.OnIncomingCall(message));
        }

        public void OnOutgoingCallAccepted(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnOutgoingCallAccepted");
            Post(() => Context.State.OnOutgoingCallAccepted(message));
        }

        public void OnOutgoingCallRejected(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnOutgoingCallRejected");
            Post(() => Context.State.OnOutgoingCallRejected(message));
        }

        public void OnRemoteHangup(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnRemoteHangup");
            Post(() => Context.State.OnRemoteHangup(message));
        }

        // WebRTC signaling
        public void OnSdpAnswer(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnSdpAnswer");
            Post(() => Context.State.OnSdpAnswer(message));
        }

        public void OnSdpOffer(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnSdpOffer");
            Post(() => Context.State.OnSdpOffer(message));
        }

        public void Reject(IncomingCallReject reason)
        {
            Debug.WriteLine("VoipChannel.Reject");
            Post(() => Context.State.Reject(reason));
        }

        #endregion

        // Semaphore used to make sure only one call on the channel
        // is executed at any given time.
        private SemaphoreSlim _sem = new SemaphoreSlim(1, 1);

        private void Post(Action fn)
        {
            Task.Run(async () =>
            {
                await _sem.WaitAsync();
                try
                {
                    fn();
                }
                finally
                {
                    _sem.Release();
                }
            });
        }
    }
}