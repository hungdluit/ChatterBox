
using ChatterBox.Shared.Communication.Messages.Registration;
using ChatterBox.Shared.Communication.Messages.Standard;

namespace ChatterBox.Shared.Communication.Contracts
{
    public interface IClientChannel
    {
        void Register(RegistrationMessage message);
        void ClientConfirmation(Confirmation confirmation);
        void ClientHeartBeat();

    }
}
