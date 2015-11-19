using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Common.Communication.Voip
{
    public delegate void RenderFormatUpdateHandler(long swapChainHandle, uint width, uint height);

    public interface IVideoRenderHelper
    {
        event RenderFormatUpdateHandler RenderFormatUpdate;
        void SetupRenderer(uint foregroundProcessId, IMediaSource source);
        void Teardown();

        void SetMediaElement(CoreDispatcher dispatcher, MediaElement mediaElement);
    }
}