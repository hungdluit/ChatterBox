
using ChatterBox.Shared.Communication.Messages.Registration;
using ChatterBox.Shared.Communication.Messages.Standard;

namespace ChatterBox.Shared.Communication.Contracts
{
    public interface IClientChannel
    {
        void Register(Registration message);
        void ClientConfirmation(Confirmation confirmation);
        void GetPeerList(Message message);


        void ClientHeartBeat();
    }
}
