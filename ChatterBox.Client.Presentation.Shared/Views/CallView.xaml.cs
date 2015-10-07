using Windows.UI.Xaml;

namespace ChatterBox.Client.Presentation.Shared.Views
{
    public sealed partial class CallView
    {
        public CallView()
        {
            InitializeComponent();
        }

        private void VideoGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SelfPlaceholder.Width = SelfVideo.Width = e.NewSize.Width*0.25D;
            SelfPlaceholder.Height = SelfVideo.Height = e.NewSize.Height*0.25D;
        }
    }
}