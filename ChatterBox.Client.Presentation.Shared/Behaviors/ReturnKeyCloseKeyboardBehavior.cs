using System;
using System.Collections.Generic;
using System.Text;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace ChatterBox.Client.Presentation.Shared.Behaviors
{
    public static class ReturnKeyCloseKeyboardBehavior
    {
        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(ReturnKeyCloseKeyboardBehavior), 
                                                new PropertyMetadata(0, new PropertyChangedCallback(OnEnabledPropertyChanged)));

        private static void OnEnabledPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var element = dp as UIElement;

            if (element == null) return;

            if (e.OldValue != null)
            {
                element.KeyDown += Element_KeyDown;
            }

            if (e.NewValue != null)
            {
                element.KeyDown += Element_KeyDown;
            }
        }

        private static void Element_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var dp = sender as DependencyObject;
            if (!GetEnabled(dp)) return;
#if WIN10
            if (e.Key == VirtualKey.Enter)
            {
                InputPane.GetForCurrentView().TryHide();
            }
#endif
        }
    }
}
