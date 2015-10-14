﻿using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Peers
{
    public sealed class PeerUpdate : IMessage
    {
        public PeerData PeerData { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset SentDateTimeUtc { get; set; }
    }
}