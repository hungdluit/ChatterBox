namespace ChatterBox.Client.Common.Mvvm.ViewModels
{
    internal class PeerViewModel
    {
        private string _displayName;

        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
