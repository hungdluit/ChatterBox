using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace ChatterBox.Client.Presentation.Shared.Views
{
    public sealed partial class ChatView
    {
        public ChatView()
        {
            InitializeComponent();
            InstantMessagingHistory.Items.VectorChanged += HistoryChanged;

            InputPane inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Showing;
            inputPane.Hiding += InputPane_Hiding; ;
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (MainGrid.RowDefinitions != null && MainGrid.RowDefinitions.Count > 1)
            {
                MainGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                MainGrid.InvalidateArrange();
            }
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            Rect coveredArea = sender.OccludedRect;

            if (MainGrid.RowDefinitions != null && MainGrid.RowDefinitions.Count > 1)
            {
                MainGrid.RowDefinitions[1].Height = new GridLength(InstantMessagingHistory.ActualHeight - coveredArea.Height);
                MainGrid.InvalidateArrange();
            }
        }

        private void HistoryChanged(IObservableVector<object> sender, IVectorChangedEventArgs @event)
        {
            if (InstantMessagingHistory.Items.Count > 0)
            {
                var lastItem = InstantMessagingHistory.Items[InstantMessagingHistory.Items.Count - 1];
                InstantMessagingHistory.UpdateLayout();
                InstantMessagingHistory.ScrollIntoView(lastItem);
            }
        }

        private void PreventFocus(object sender, TappedRoutedEventArgs e)
        {
            Focus(FocusState.Programmatic);
        }
    }
}