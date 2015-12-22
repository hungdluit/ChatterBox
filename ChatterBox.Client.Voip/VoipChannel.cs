using System;
using System.Diagnostics;
using Windows.Graphics.Display;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using ChatterBox.Client.Voip;
using System.Threading.Tasks;

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipChannel : IVoipChannel
    {
        private readonly IHub _hub;

        // This variable should not be used outside of the getter below.

        private CoreDispatcher Dispatcher { get; }

        private VoipContext Context { get; }

        public VoipChannel(IHub hub, CoreDispatcher dispatcher,
                           VoipContext context)
        {
            _hub = hub;
            Dispatcher = dispatcher;
            Context = context;
        }

        #region IVoipChannel Members

        public void DisplayOrientationChanged(DisplayOrientations orientation)
        {
            Context.DisplayOrientation = orientation;
        }

        public void SetForegroundProcessId(uint processId)
        {
            Context.ForegroundProcessId = processId;
        }


        public void Answer()
        {
            Task.Run(() =>
            {
                Debug.WriteLine("VoipChannel.Answer");
                Context.WithState(st => st.Answer()).Wait();
            });
        }

        public void Call(OutgoingCallRequest request)
        {
            Task.Run(() =>
            {
                Debug.WriteLine("VoipChannel.Call");
                _hub.IsAppInsightsEnabled = ChatterBox.Client.Common.Settings.SignalingSettings.AppInsightsEnabled;
                Context.WithState(st => st.Call(request)).Wait();
            });
        }

        public VoipState GetVoipState()
        {
            return Context.GetVoipState();
        }

        // Hangup can happen on both sides
        public void Hangup()
        {
            Task.Run(() =>
            {

                Debug.WriteLine("VoipChannel.Hangup ");
                Context.WithState(st => st.Hangup()).Wait();
            });
        }

        public void OnIceCandidate(RelayMessage message)
        {
            Task.Run(() =>
            {
                Debug.WriteLine("VoipChannel.OnIceCandidate");
                Context.WithState(st => st.OnIceCandidate(message)).Wait();
            });
        }

        // Remotely initiated calls
        public void OnIncomingCall(RelayMessage message)
        {
            Task.Run(() =>
            {
                Debug.WriteLine("VoipChannel.OnIncomingCall");
                Context.WithState(st => st.OnIncomingCall(message)).Wait();
            });
        }

        public void OnOutgoingCallAccepted(RelayMessage message)
        {
            Task.Run(() =>
            {
                Debug.WriteLine("VoipChannel.OnOutgoingCallAccepted");
                Context.WithState(st => st.OnOutgoingCallAccepted(message)).Wait();
            });
        }

        public void OnOutgoingCallRejected(RelayMessage message)
        {
            Task.Run(() =>
            {
                Debug.WriteLine("VoipChannel.OnOutgoingCallRejected");
                Context.WithState(st => st.OnOutgoingCallRejected(message)).Wait();
            });
        }

        public void OnRemoteHangup(RelayMessage message)
        {
            Task.Run(() =>
            {
                Debug.WriteLine("VoipChannel.OnRemoteHangup");
                Context.WithState(st => st.OnRemoteHangup(message)).Wait();
            });
        }

        // WebRTC signaling
        public void OnSdpAnswer(RelayMessage message)
        {
            Task.Run(() =>
            {
                Debug.WriteLine("VoipChannel.OnSdpAnswer");
                Context.WithState(st => st.OnSdpAnswer(message)).Wait();
            });
        }

        public void OnSdpOffer(RelayMessage message)
        {
            Task.Run(() =>
            {
                Debug.WriteLine("VoipChannel.OnSdpOffer");
                Context.WithState(st => st.OnSdpOffer(message)).Wait();
            });
        }

        public void Reject(IncomingCallReject reason)
        {
            Task.Run(() =>
            {
                Debug.WriteLine("VoipChannel.Reject");
                Context.WithState(st => st.Reject(reason)).Wait();
            });
        }

        public void ConfigureMicrophone(MicrophoneConfig config)
        {
            Task.Run(() =>
            {
                Debug.WriteLine("VoipChannel.ConfigureMicrophone Muted=" + (config.Muted ? "yes" : "no"));
                Context.MicrophoneMuted = config.Muted;
            });
        }

        #endregion

        public void RegisterVideoElements(MediaElement self, MediaElement peer)
        {
            Context.LocalVideoRenderer.SetMediaElement(Dispatcher, self);
            Context.RemoteVideoRenderer.SetMediaElement(Dispatcher, peer);
        }
    }
}
