using ChatterBox.Client.Presentation.Shared.MVVM;

namespace ChatterBox.Client.Presentation.Shared.Models
{
    public class ContactModel : BindableBase
    {
        private string _userId;
        private string _name;
        private bool _isOnline;

        public string UserId
        {
            get { return _userId; }
            set { SetProperty(ref _userId, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public bool IsOnline
        {
            get { return _isOnline; }
            set { SetProperty(ref _isOnline, value); }
        }

        public override string ToString()
        {
            return $"{Name}-{IsOnline}";
        }
    }
}
