using System;

namespace ChatterBox.Common.Communication.Messages.Interfaces
{
    public interface IMessageConfirmation
    {
        Guid ConfirmationFor { get; set; }
    }
}
