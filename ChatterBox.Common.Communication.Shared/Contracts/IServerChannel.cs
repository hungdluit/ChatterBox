using ChatterBox.Common.Communication.Messages.Peers;
using ChatterBox.Common.Communication.Messages.Standard;

namespace ChatterBox.Common.Communication.Contracts
{
    public interface IServerChannel
    {
        void ServerConfirmation(Confirmation confirmation);
        void ServerReceivedInvalidMessage(InvalidMessage reply);
        void ServerError(ErrorReply reply);
        void ServerHeartBeat();

        void OnPeerPresence(PeerInformation peer);

        void OnPeerList(PeerList peerList);


        void OnRegistrationConfirmation(OkReply reply);
    }
}