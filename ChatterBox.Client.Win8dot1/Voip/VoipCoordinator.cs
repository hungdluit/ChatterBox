using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using System.Threading.Tasks;

namespace ChatterBox.Client.Win8dot1.Voip
{
    internal class VoipCoordinator : IVoipCoordinator
    {
        public void NotifyCallActive()
        {
        }

        public void NotifyCallEnded()
        {
        }

        public void OnEnterIdle()
        {
        }

        public void OnEnterLocalRinging(RelayMessage message)
        {
        }

        public void OnEnterRemoteRinging(OutgoingCallRequest request)
        {
        }

        public async Task OnLeavingIdle()
        {
        }

        public void OnOutgoingCallRejected()
        {
        }
    }
}
