using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Standard
{
    public sealed class Confirmation : IMessageConfirmation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ConfirmationFor { get; set; }

        public static Confirmation For(IMessage message)
        {
            return new Confirmation
            {
                ConfirmationFor = message.Id
            };
        }
    }
}
