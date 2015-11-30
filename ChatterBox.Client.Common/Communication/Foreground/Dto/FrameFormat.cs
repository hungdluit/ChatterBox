using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatterBox.Client.Common.Communication.Foreground.Dto
{
  public sealed class FrameFormat
  {
    public bool IsLocal { get; set; }
    
    public Int64 SwapChainHandle { get; set; }

    public UInt32 Width { get; set; }

    public UInt32 Height { get; set; }
  }
}
