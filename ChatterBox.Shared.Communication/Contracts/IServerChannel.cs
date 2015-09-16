using ChatterBox.Shared.Communication.Messages.Standard;

namespace ChatterBox.Shared.Communication.Contracts
{
    public interface IServerChannel
    {
        void ServerConfirmation(Confirmation confirmation);
        void ServerReceivedInvalidMessage(InvalidMessage reply);
        void ServerError(ErrorReply reply);
        void ServerHeartBeat();


        void RegistrationConfirmation(OkReply reply);
    }
}