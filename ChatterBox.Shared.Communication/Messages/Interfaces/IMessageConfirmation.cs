using System;

namespace ChatterBox.Shared.Communication.Messages.Interfaces
{
    public interface IMessageConfirmation
    {
        Guid ConfirmationFor { get; set; }
    }
}
