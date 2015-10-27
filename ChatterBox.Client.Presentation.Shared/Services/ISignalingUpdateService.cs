using System;

namespace ChatterBox.Client.Presentation.Shared.Services
{
    public interface ISignalingUpdateService
    {
        event Action OnPeerDataUpdated;
        event Action OnRegistrationStatusUpdated;
        event Action OnRelayMessagesUpdated;
    }
}