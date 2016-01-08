using ChatterBox.Client.Presentation.Shared.MVVM;
using System;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class InstantMessageViewModel : BindableBase
    {
        public DateTime DateTime { get; set; }
        public bool IsSender { get; set; }
        public string Message { get; set; }
        public string Sender { get; set; }

        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set { SetProperty(ref _isHighlighted, value); }
        }

        public override string ToString()
        {
            return $"{Sender} - {Message}";
        }
    }
}