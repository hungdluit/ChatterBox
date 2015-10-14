using ChatterBox.Common.Communication.Messages.Peers;
using ChatterBox.Common.Communication.Messages.Standard;
using ChatterBox.Common.Communication.Shared.Messages.Registration;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Common.Communication.Contracts
{
    public interface IServerChannel
    {
        void OnPeerList(PeerList peerList);
        void OnPeerPresence(PeerUpdate peer);
        void OnRegistrationConfirmation(RegisteredReply reply);
        void ServerConfirmation(Confirmation confirmation);
        void ServerError(ErrorReply reply);
        void ServerHeartBeat();
        void ServerReceivedInvalidMessage(InvalidMessage reply);
        void ServerRelay(RelayMessage message);
    }
}