using ChatterBox.Client.Presentation.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public sealed class CallViewModel : BindableBase
    {


        public event Action OnCompleted;
    }
}
