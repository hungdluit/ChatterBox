using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;

namespace ChatterBox.Client.Universal.Background.Voip
{
    internal class VoipCallContext
    {
        public VoipPhoneCall VoipCall { get; set; }
    }
}
