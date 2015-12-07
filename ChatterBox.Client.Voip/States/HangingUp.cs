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

            StopVideoTracks(Context.LocalStream?.GetVideoTracks());
            StopAudioTracks(Context.LocalStream?.GetAudioTracks());
            Context.LocalStream?.Stop();

            StopVideoTracks(Context.RemoteStream?.GetVideoTracks());
            StopAudioTracks(Context.RemoteStream?.GetAudioTracks());
            Context.RemoteStream?.Stop();

            Context.VoipCoordinator.OnEnterHangingUp();

            var idleState = new VoipState_Idle();
            await Context.SwitchState(idleState);
        }

        private void StopVideoTracks(IList<MediaVideoTrack> tracks)
        {
            if (tracks == null) return;
            foreach (var track in tracks)
            {
                track.Stop();
            }
        }

        private void StopAudioTracks(IList<MediaAudioTrack> tracks)
        {
            if (tracks == null) return;
            foreach (var track in tracks)
            {
                track.Stop();
            }
        }
    }
}
