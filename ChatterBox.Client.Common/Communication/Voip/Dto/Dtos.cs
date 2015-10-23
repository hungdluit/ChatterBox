using System;
using System.Collections.Generic;
using System.Text;

namespace ChatterBox.Client.Common.Communication.Voip.Dto
{
    public sealed class OutgoingCallRequest
    {
        public string PeerUserId { get; set; }
    }

    public sealed class IncomingCallReject
    {
        public string Reason { get; set; }
    }
}
