using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Messages.Standard;

namespace ChatterBox.Common.Communication.Contracts
{
    public interface IClientChannel
    {
        void ClientConfirmation(Confirmation confirmation);
        void ClientHeartBeat();
        void GetPeerList(Message message);
        void Register(Registration message);
        void Relay(RelayMessage message);
    }
}