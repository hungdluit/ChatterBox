
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Standard;

namespace ChatterBox.Common.Communication.Contracts
{
    public interface IClientChannel
    {
        void Register(Registration message);
        void ClientConfirmation(Confirmation confirmation);
        void GetPeerList(Message message);


        void ClientHeartBeat();
    }
}
