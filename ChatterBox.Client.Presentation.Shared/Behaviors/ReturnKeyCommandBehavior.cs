using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ChatterBox.Client.Presentation.Shared.Behaviors
{
    public class ReturnKeyCommandBehavior
    {
        private static bool _isControlPressed = false;
        private static ICommand _command;

        static ReturnKeyCommandBehavior()
        {
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
        }

        private static void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (!_isControlPressed) return;
            if (_command == null) return;
            if (args.VirtualKey != VirtualKey.Enter) return;

            if (args.EventType == CoreAcceleratorKeyEventType.KeyDown)
            {
                args.Handled = true;
                _command?.Execute(null);
            }
        }

        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ReturnKeyCommandBehavior), new PropertyMetadata(0, OnCommandUpdatePropertyChanged));

        private static void OnCommandUpdatePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = dp as UIElement;

            if (element == null)
            {
                return;
            }

            if (e.OldValue != null)
            {
                element.KeyDown -= HandleKeyDown;
                element.KeyUp -= HandleKeyUp;
            }

            if (e.NewValue != null)
            {
                element.KeyDown += HandleKeyDown;
                element.KeyUp += HandleKeyUp;
            }
        }

        static void HandleKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control)
            {
                _isControlPressed = true;
                var cmd = ((UIElement)sender).GetValue(CommandProperty) as ICommand;
                if (cmd != null && cmd.CanExecute(null))
                {
                    _command = cmd;
                }
            }
        }

        static void HandleKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control)
            {
                _isControlPressed = false;
                _command = null;
            }
        }
    }
}
