using System;
using ChatterBox.Client.Common.Communication.Foreground.Dto;

namespace ChatterBox.Client.Presentation.Shared.Services
{
    public interface IForegroundUpdateService
    {
        event Action OnPeerDataUpdated;
        event Action OnRegistrationStatusUpdated;
        event Action OnRelayMessagesUpdated;
        event Action<VoipState> OnVoipStateUpdate;
        event Action<FrameFormat> OnFrameFormatUpdate;
        event Func<string> GetShownUser;
    }
}