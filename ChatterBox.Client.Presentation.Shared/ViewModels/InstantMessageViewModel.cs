using System;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class InstantMessageViewModel
    {
        public DateTime DateTime { get; set; }
        public bool IsSender { get; set; }
        public string Message { get; set; }
        public string Sender { get; set; }

        public override string ToString()
        {
            return $"{Sender} - {Message}";
        }
    }
}