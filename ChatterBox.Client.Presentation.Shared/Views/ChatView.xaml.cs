

using System.Collections.Specialized;
using Windows.Foundation.Collections;

namespace ChatterBox.Client.Presentation.Shared.Views
{
    public sealed partial class ChatView
    {
        public ChatView()
        {
            InitializeComponent();
            ((IObservableVector<object>)InstantMessagingHistory.Items).VectorChanged += HistoryChanged;
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
    }
}