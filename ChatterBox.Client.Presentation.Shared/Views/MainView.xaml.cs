using Windows.UI.Xaml.Navigation;
using ChatterBox.Client.Presentation.Shared.ViewModels;

namespace ChatterBox.Client.Presentation.Shared.Views
{
    public sealed partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var viewModel = (MainViewModel) e.Parameter;
            DataContext = viewModel;
            viewModel.OnNavigatedTo();
        }
    }
}