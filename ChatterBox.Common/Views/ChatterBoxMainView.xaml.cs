using ChatterBox.Common.Mvvm;
using ChatterBox.Common.Mvvm.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ChatterBox.Common.Views
{
    public sealed partial class ChatterBoxMainView : UserControl
    {
        private NavigationService _navigationService;

        public ChatterBoxMainView()
        {
            this.InitializeComponent();
            _navigationService = new NavigationService(LeftNavigationFrame, RightNavigationFrame);
            DataContext = new CompositeViewModel(Dispatcher, _navigationService);
            RightNavigationFrame.Navigated += RightNavigationFrame_Navigated;
            this.Loaded += ChatterBoxMainView_Loaded;
        }

        private void RightNavigationFrame_Navigated(object sender, NavigationEventArgs e)
        {
            
        }

        private void ChatterBoxMainView_Loaded(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateTo(typeof(FirstPanelView));
        }
    }
}
