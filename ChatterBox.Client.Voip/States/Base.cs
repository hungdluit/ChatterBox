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

        public virtual async Task SendLocalIceCandidates(RTCIceCandidate[] candidates)
        {
        }

        internal virtual async Task OnAddStream(MediaStream stream)
        {
        }

        internal async Task SuspendVoipVideo()
        {
            Context.ResetRenderers();
            // Don't send RenderFormatUpdate here.  The UI is suspending
            // and may not get the message.
            if (Context.LocalStream != null)
            {
                foreach (var track in Context.LocalStream.GetVideoTracks())
                {
                    track.Suspended = true;
                }
            }
        }

        internal async Task ResumeVoipVideo()
        {
            Context.ResetRenderers();
            // Setup remote before local as it's more important.
            if (Context.RemoteStream != null)
            {
                var tracks = Context.RemoteStream.GetVideoTracks();
                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, "PEER");
                Context.RemoteVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source);
            }
            // TODO: Delay here prevents a crash in the MF media engine when setting up the second
            //       renderer.  Investigate why this is happening.  Occurred 100% of the time.
            await Task.Delay(3000);
            if (Context.LocalStream != null)
            {
                var tracks = Context.LocalStream.GetVideoTracks();
                foreach (var track in tracks)
                {
                    track.Suspended = false;
                }

                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, "LOCAL");
                Context.LocalVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source);
            }
        }
    }
}