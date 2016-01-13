using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using System.Threading.Tasks;

namespace ChatterBox.Client.Win8dot1.Voip
{
    internal class VoipCoordinator : IVoipCoordinator
    {
        public void StartOutgoingCall(OutgoingCallRequest request)
        {
        }

        public void StartIncomingCall(RelayMessage message)
        {
        }

        public void StopVoip()
        {
        }

        public async Task StartVoipTask()
        {
        }
    }
}
