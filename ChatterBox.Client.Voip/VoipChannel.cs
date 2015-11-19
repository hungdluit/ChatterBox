﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using Microsoft.Practices.Unity;
using ChatterBox.Common.Communication.Messages.Relay;
using Windows.Networking.Connectivity;
using System.Collections.Generic;
using ChatterBox.Client.Voip;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using ChatterBox.Client.Voip.States.Interfaces;

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipChannel : IVoipChannel
    {
        // Semaphore used to make sure only one call on the channel
        // is executed at any given time.
        private readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);

        DateTimeOffset _callStartDateTime;

        // This variable should not be used outside of the getter below.
        private VoipContext _context;

        private UInt32 _foregroundProcessId;
        private IHub _hub;
        private CoreDispatcher _dispatcher;
        private IVoipCoordinator _coordinator;

        private VoipContext Context
        {
            get
            {
                return _context;
            }
        }

        public VoipChannel(IHub hub, 
                           CoreDispatcher dispatcher,
                           IVoipCoordinator coordinator,
                           VoipContext context)
        {
            _hub = hub;
            _dispatcher = dispatcher;
            _coordinator = coordinator;
            _context = context;
        }

        #region IVoipChannel Members

        public void DisplayOrientationChanged(DisplayOrientations orientation)
        {
            Context.DisplayOrientation = orientation;
        }

        public void SetForegroundProcessId(uint processId)
        {
            _foregroundProcessId = processId;
            _context.ForegroundProcessId = processId;
        }


        public void Answer()
        {
            Debug.WriteLine("VoipChannel.Answer");
            Post(() => Context.WithState(st => st.Answer()));
            TrackCallStarted();
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
            // don't log CallEnded if it is not started yet
            if (Context.GetVoipState().State != VoipStateEnum.RemoteRinging)
            {
                TrackCallEnded();
            }
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
            TrackCallStarted();
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

        private void TrackCallStarted()
        {
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
            // log call duration as CallEnded event property
            string duration = DateTimeOffset.Now.Subtract(_callStartDateTime).Duration().ToString(@"hh\:mm\:ss");
            var properties = new Dictionary<string, string> { { "Call Duration", duration } };
            _hub.TrackStatsManagerEvent("CallEnded", properties);

            // stop call watch, so the duration will be calculated and tracked as request
            _hub.StopStatsManagerCallWatch();
        }

        public void RegisterVideoElements(MediaElement self, MediaElement peer)
        {
            Context.LocalVideoRenderer.SetMediaElement(_dispatcher, self);
            Context.RemoteVideoRenderer.SetMediaElement(_dispatcher, peer);
        }
    }
}
