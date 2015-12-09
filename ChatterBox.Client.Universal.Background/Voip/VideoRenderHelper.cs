using ChatterBox.Client.Common.Communication.Voip;
using ChatterBoxClient.Universal.BackgroundRenderer;
using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

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

        public void SetMediaElement(CoreDispatcher dispatcher, MediaElement mediaElement)
        {
        }

        private Renderer _renderer = new Renderer();
    }
}
