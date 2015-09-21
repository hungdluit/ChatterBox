using ChatterBox.Common.Mvvm.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ChatterBox.Common.Views
{
    public sealed partial class SettingsView : Page
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = new SettingsViewModel(Dispatcher, e.Parameter as Frame);
        }
    }
}
