using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Standard
{
    public sealed class Confirmation : IMessageConfirmation
    {
        public string ConfirmationFor { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public static Confirmation For(IMessage message)
        {
            return new Confirmation
            {
                ConfirmationFor = message.Id
            };
        }
    }
}