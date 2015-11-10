using ChatterBox.Client.Common.Communication.Voip;
using System;
using System.Collections.Generic;
using System.Text;
using webrtc_winrt_api;

namespace ChatterBox.Client.Voip.States.Interfaces
{
    internal interface IIdle
    {
        void EnterState(VoipContext context);

        void LeaveState();

        void OnEnteringState();

        void OnLeavingState();

        void SendLocalIceCandidate(RTCIceCandidate candidate);
    }
}
