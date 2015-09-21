using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatterBox.Common.Mvvm.ViewModels
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
