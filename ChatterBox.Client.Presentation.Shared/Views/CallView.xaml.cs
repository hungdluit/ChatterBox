using ChatterBox.Client.Presentation.Shared.Converters;
using ChatterBox.Client.Presentation.Shared.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.Views
{
    public sealed partial class CallView
    {
        private MediaElement _selfMediaElement;
        private MediaElement _peerMediaElement;

        public CallView()
        {
            InitializeComponent();
            SetVideoPresenters();
            DataContextChanged += CallView_DataContextChanged;
        }

        private void CallView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var conversationViewModel = DataContext as ConversationViewModel;
            conversationViewModel.RegisterAudioElement(SoundPlayElement);
            conversationViewModel.RegisterVideoElements(_selfMediaElement, _peerMediaElement);
        }

        private void SetVideoPresenters()
        {
            var boolToVisConverter = new BooleanToVisibilityConverter();

#if WIN10
            var peerSwapChainPanel = new WebRTCSwapChainPanel.WebRTCSwapChainPanel();

            var peerHandleBinding = new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("RemoteSwapChainPanelHandle"),
                Mode = BindingMode.OneWay
            };
            peerSwapChainPanel.SetBinding(
                WebRTCSwapChainPanel.WebRTCSwapChainPanel.SwapChainPanelHandleProperty,
                peerHandleBinding);

            PeerVideoPresenter.Content = peerSwapChainPanel;

            var selfSwapChainPanel = new WebRTCSwapChainPanel.WebRTCSwapChainPanel();

            var selfHandleBinding = new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("LocalSwapChainPanelHandle"),
                Mode = BindingMode.OneWay
            };
            selfSwapChainPanel.SetBinding(
                WebRTCSwapChainPanel.WebRTCSwapChainPanel.SwapChainPanelHandleProperty,
                selfHandleBinding);

            var selfSizeBinding = new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("LocalNativeVideoSize"),
            };
            selfSwapChainPanel.SetBinding(
                WebRTCSwapChainPanel.WebRTCSwapChainPanel.SizeProperty,
                selfSizeBinding);

            SelfVideoPresenter.Content = selfSwapChainPanel;
#endif

#if WIN81
            _peerMediaElement = new MediaElement
            {
                RealTimePlayback = true
            };
            PeerVideoPresenter.Content = _peerMediaElement;

            _selfMediaElement = new MediaElement
            {
                RealTimePlayback = true
            };
            SelfVideoPresenter.Content = _selfMediaElement;            
#endif
        }


        private void VideoGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SelfPlaceholder.Width = SelfVideoPresenter.Width = e.NewSize.Width * 0.25D;
            SelfPlaceholder.Height = SelfVideoPresenter.Height = e.NewSize.Height * 0.25D;
        }
    }
}