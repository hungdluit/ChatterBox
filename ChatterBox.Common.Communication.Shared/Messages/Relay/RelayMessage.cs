﻿using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Shared.Messages.Relay
{
    public sealed class RelayMessage : IMessage
    {
        public int FromAvatar { get; set; }
        public string FromName { get; set; }
        public string FromUserId { get; set; }
        public string Payload { get; set; }
        public string Tag { get; set; }
        public string ToName { get; set; }
        public string ToUserId { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset SentDateTimeUtc { get; set; }
    }
}