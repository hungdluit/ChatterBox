using ChatterBox.Client.Common.Communication.Voip;
using ChatterBoxClient.Universal.BackgroundRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using webrtc_winrt_api;
using Windows.ApplicationModel.Calls;
using Windows.Media.Core;

namespace ChatterBox.Client.Universal.Background.Voip
{
    public sealed class VideoRenderHelper : IVideoRenderHelper
    {
        public VideoRenderHelper()
        {
            // Pipe the event
            _renderer.RenderFormatUpdate += (a, b, c) => RenderFormatUpdate(a, b, c);
        }

        public event ChatterBox.Client.Common.Communication.Voip.RenderFormatUpdateHandler RenderFormatUpdate;

        public void SetupRenderer(uint foregroundProcessId, IMediaSource source)
        {
            _renderer.SetupRenderer(foregroundProcessId, source);
        }

        public void Teardown()
        {
            _renderer.Teardown();
        }

        private Renderer _renderer = new Renderer();
    }
}
