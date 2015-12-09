using ChatterBox.Client.Common.Communication.Voip;
using System;
using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Win8dot1.Voip
{
    internal class VideoRenderHelper : IVideoRenderHelper
    {
        public event RenderFormatUpdateHandler RenderFormatUpdate;

        private CoreDispatcher _dispatcher;
        private MediaElement _mediaElement;

        public void SetMediaElement(CoreDispatcher dispatcher, MediaElement mediaElement)
        {
            _dispatcher = dispatcher;
            _mediaElement = mediaElement;
        }

        public void SetupRenderer(uint foregroundProcessId, IMediaSource source)
        {
            if (_mediaElement != null)
            {
                Action fn = (() =>
                {
                    _mediaElement.SetMediaStreamSource(source);
                });
                _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new DispatchedHandler(fn));
            }
        }

        public void Teardown()
        {
        }
    }
}
