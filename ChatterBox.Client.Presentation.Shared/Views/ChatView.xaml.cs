using Windows.Foundation.Collections;
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