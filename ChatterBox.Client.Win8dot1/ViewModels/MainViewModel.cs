using ChatterBox.Client.Presentation.Shared.ViewModels;
using ChatterBox.Client.Signaling;
using ChatterBox.Client.Tasks.Signaling.Win8dot1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;

namespace ChatterBox.Client.Win8dot1.ViewModels
{
    public class MainViewModel : MainViewModelBase
    {
        private SignalingClient _signalingClient;

        public MainViewModel(CoreDispatcher dispatcher) : base(dispatcher)
        {

        }

        public async override void OnNavigatedTo()
        {
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            var signalingTaskHelper = new SignalingTaskHelper();
            var signalingTask = await signalingTaskHelper.RegisterTask();

            var socketService = new SignalingSocketService(signalingTaskHelper.ControlChannelTrigger);
            _signalingClient = new SignalingClient(socketService);

            var isConnected = _signalingClient.CheckConnection();
            if (!isConnected)
            {
                isConnected = _signalingClient.Connect();
                if (!isConnected)
                {
                    var dialog = new MessageDialog("Connecting to the server failed.");
                    await dialog.ShowAsync();
                    return;
                }
            }

            base.OnNavigatedTo();
        }

        public override void OnRegistrationCompleted()
        {
            _signalingClient.RegisterUsingSettings();   
        }
    }
}
