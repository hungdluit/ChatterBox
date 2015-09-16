using System;
using ChatterBox.Shared.Communication.Messages.Interfaces;

namespace ChatterBox.Shared.Communication.Messages.Registration
{
    public sealed class RegistrationMessage : IMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        public string PushToken { get; set; }
    }
}
