using ChatterBox.Client.Presentation.Shared.ViewModels;
using ChatterBox.Client.Win8._1.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using Windows.UI.Core;

namespace ChatterBox.Client.Win8._1.ViewModels
{
    public class MainViewModel : MainViewModelBase
    {
        public MainViewModel(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {

        }

        public override void OnRegistrationCompleted()
        {
            
        }

        public override async void OnNavigatedTo()
        {
            var helper = new SignalingTaskHelper();
            var signalingTask = await helper.RegisterTask();   

            base.OnNavigatedTo();
        }
    }
}
