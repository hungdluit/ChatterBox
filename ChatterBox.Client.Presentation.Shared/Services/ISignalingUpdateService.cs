using System;

namespace ChatterBox.Client.Presentation.Shared.Services
{
    public interface ISignalingUpdateService
    {
        event Action OnRegistrationStatusUpdated;
        event Action OnPeerDataUpdated;
        event Action OnRelayMessagesUpdated;
    }
}