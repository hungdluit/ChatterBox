using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ChatterBox.Client.Presentation.Shared.ViewModels;

namespace ChatterBox.Client.Presentation.Shared.StyleSelectors
{
    public class InstantMessageStyleSelector : StyleSelector
    {
        public Style PeerMessageStyle { get; set; }
        public Style OwnMessageStyle { get; set; }

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            var message = (InstantMessageViewModel) item;
            return message.IsSender ? OwnMessageStyle : PeerMessageStyle;
        }
    }
}