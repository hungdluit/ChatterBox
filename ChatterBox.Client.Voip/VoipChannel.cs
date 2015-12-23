using System;
using System.Diagnostics;
using Windows.Graphics.Display;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using Windows.Networking.Connectivity;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using ChatterBox.Client.Voip;
using System.Threading.Tasks;

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipChannel : IVoipChannel
    {
        private readonly IHub _hub;
        DateTimeOffset _callStartDateTime;

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
                _hub.IsAppInsightsEnabled = ChatterBox.Client.Common.Settings.SignalingSettings.AppInsightsEnabled;
                Context.WithState(st => st.Answer()).Wait();
                TrackCallStarted();
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

                Debug.WriteLine("VoipChannel.Hangup");
                // don't log CallEnded if it is not started yet
                if (Context.GetVoipState().State != VoipStateEnum.RemoteRinging)
                {
                    TrackCallEnded();
                }
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
                TrackCallStarted();
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

        private void TrackCallStarted()
        {
            if (!_hub.IsAppInsightsEnabled) {
                return;
            }
            _callStartDateTime = DateTimeOffset.Now;
            var currentConnection = NetworkInformation.GetInternetConnectionProfile();
            string connType;
            switch (currentConnection.NetworkAdapter.IanaInterfaceType)
            {
                case 6:
                    connType = "Cable";
                    break;
                case 71:
                    connType = "WiFi";
                    break;
                case 243:
                    connType = "Mobile";
                    break;
                default:
                    connType = "Unknown";
                    break;
            }
            var properties = new Dictionary<string, string> { { "Connection Type", connType } };
            _hub.TrackStatsManagerEvent("CallStarted", properties);
            // start call watch to count duration for tracking as request
            _hub.StartStatsManagerCallWatch();
        }

        private void TrackCallEnded() {
            if (!_hub.IsAppInsightsEnabled)
            {
                return;
            }
            // log call duration as CallEnded event property
            string duration = DateTimeOffset.Now.Subtract(_callStartDateTime).Duration().ToString(@"hh\:mm\:ss");
            var properties = new Dictionary<string, string> { { "Call Duration", duration } };
            _hub.TrackStatsManagerEvent("CallEnded", properties);

            // stop call watch, so the duration will be calculated and tracked as request
            _hub.StopStatsManagerCallWatch();
        }

        public void RegisterVideoElements(MediaElement self, MediaElement peer)
        {
            Context.LocalVideoRenderer.SetMediaElement(Dispatcher, self);
            Context.RemoteVideoRenderer.SetMediaElement(Dispatcher, peer);
        }
    }
}
