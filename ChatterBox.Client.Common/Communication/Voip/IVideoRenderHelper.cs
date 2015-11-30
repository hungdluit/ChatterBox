using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using System;
using Windows.Media.Core;

namespace ChatterBox.Client.Common.Communication.Voip
{
    public delegate void RenderFormatUpdateHandler(long swapChainHandle, uint width, uint height);

    public interface IVideoRenderHelper
    {
        event RenderFormatUpdateHandler RenderFormatUpdate;
        void SetupRenderer(uint foregroundProcessId, IMediaSource source);
        void Teardown();
    }
}