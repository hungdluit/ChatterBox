using ChatterBox.Client.Common.Mvvm.Utils;
using ChatterBox.Client.Common.Mvvm.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ChatterBox.Client.Common.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class CallView : Page
    {
        public CallView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = new CallViewModel(Dispatcher, e.Parameter as NavigationService);
        }
    }
}
