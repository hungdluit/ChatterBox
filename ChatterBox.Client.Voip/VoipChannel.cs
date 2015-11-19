using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using Microsoft.Practices.Unity;
using ChatterBox.Common.Communication.Messages.Relay;

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipChannel : IVoipChannel
    {
        // Semaphore used to make sure only one call on the channel
        // is executed at any given time.
        private readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);
        // This variable should not be used outside of the getter below.
        private VoipContext _context;

        private IUnityContainer _container;

        private VoipContext Context
        {
            get
            {
                // Create on demand.
                if (_context == null)
                    _context = new VoipContext(_container);
                return _context;
            }
        }

        public VoipChannel(IUnityContainer container)
        {
            _container = container;
        }

        #region IVoipChannel Members

        public void Answer()
        {
            Debug.WriteLine("VoipChannel.Answer");
            Post(() => Context.WithState(st => st.Answer()));
        }

        public void Call(OutgoingCallRequest request)
        {
            Debug.WriteLine("VoipChannel.Call");
            Post(() => Context.WithState(st => st.Call(request)));
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
            Post(() => Context.WithState(st => st.Hangup()));
        }

        public void OnIceCandidate(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnIceCandidate");
            Post(() => Context.WithState(st => st.OnIceCandidate(message)));
        }

        // Remotely initiated calls
        public void OnIncomingCall(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnIncomingCall");
            Post(() => Context.WithState(st => st.OnIncomingCall(message)));
        }

        public void OnOutgoingCallAccepted(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnOutgoingCallAccepted");
            Post(() => Context.WithState(st => st.OnOutgoingCallAccepted(message)));
        }

        public void OnOutgoingCallRejected(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnOutgoingCallRejected");
            Post(() => Context.WithState(st => st.OnOutgoingCallRejected(message)));
        }

        public void OnRemoteHangup(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnRemoteHangup");
            Post(() => Context.WithState(st => st.OnRemoteHangup(message)));
        }

        // WebRTC signaling
        public void OnSdpAnswer(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnSdpAnswer");
            Post(() => Context.WithState(st => st.OnSdpAnswer(message)));
        }

        public void OnSdpOffer(RelayMessage message)
        {
            Debug.WriteLine("VoipChannel.OnSdpOffer");
            Post(() => Context.WithState(st => st.OnSdpOffer(message)));
        }

        public void Reject(IncomingCallReject reason)
        {
            Debug.WriteLine("VoipChannel.Reject");
            Post(() => Context.WithState(st => st.Reject(reason)));
        }

        #endregion

        private void Post(Action fn)
        {
            Task.Run(async () =>
            {
                await _sem.WaitAsync();
                try
                {
                    fn();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message} {Environment.NewLine}{ex.StackTrace}");
                }
                finally
                {
                    _sem.Release();
                }
            });
        }
    }
}
