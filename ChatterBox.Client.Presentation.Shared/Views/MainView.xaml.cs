using Windows.UI.Xaml.Navigation;
using ChatterBox.Client.Presentation.Shared.ViewModels;

namespace ChatterBox.Client.Presentation.Shared.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var viewModel = (MainViewModelBase)e.Parameter;
            DataContext = viewModel;
            viewModel.OnNavigatedTo();
        }
    }
}
