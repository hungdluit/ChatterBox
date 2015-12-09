using System;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Common.Communication.Messages.Relay;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using webrtc_winrt_api;
using System.Threading.Tasks;

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_HangingUp : BaseVoipState
    {
        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.HangingUp;
            }
        }

        public override async Task OnEnteringState()
        {
            Context.SendToPeer(RelayMessageTags.VoipHangup, "");
            if (Context.PeerConnection != null)
            {
                Context.PeerConnection.Close();
                Context.PeerConnection = null;
                Context.PeerId = null;
            }

            StopTracks(Context.LocalStream?.GetTracks());
            Context.LocalStream?.Stop();
            Context.LocalStream = null;

            StopTracks(Context.RemoteStream?.GetTracks());
            Context.RemoteStream?.Stop();
            Context.RemoteStream = null;

            var idleState = new VoipState_Idle();
            await Context.SwitchState(idleState);
        }

        private void StopTracks(IList<IMediaStreamTrack> tracks)
        {
            if (tracks == null) return;
            foreach (var track in tracks)
            {
                track.Stop();
            }
        }
    }
}
