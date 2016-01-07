using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using System.Threading.Tasks;

namespace ChatterBox.Client.Voip.States.Interfaces
{
    /// <summary>
    ///  Voip call coordinator only for win10 client.
    /// </summary>
    /// <remarks>
    /// On win8 the voip APIs are not available.
    /// </remarks>
    internal interface IVoipCoordinator
    {
        void StartOutgoingCall(OutgoingCallRequest request);

        void SetActiveIncomingCall(RelayMessage message, bool videoEnabled);

        void SetActiveCall(OutgoingCallRequest request);

        void StartIncomingCall(RelayMessage message);

        void StopVoip();

        Task StartVoipTask();
    }
}
