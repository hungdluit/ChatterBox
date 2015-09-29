using System;
using Windows.UI.Core;
using Windows.UI.Popups;
using ChatterBox.Client.Settings;
using ChatterBox.Client.Signaling;
using ChatterBox.Client.Tasks.Signaling.Universal;
using ChatterBox.Client.Universal.Helpers;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class MainViewModel : MainViewModelBase
    {
        private readonly CoreDispatcher _uiDispatcher;
        private SignalingClient _signalingClient;

        public MainViewModel(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
            _uiDispatcher = uiDispatcher;
        }

        public override async void OnNavigatedTo()
        {
            var helper = new SignalingTaskHelper();
            var signalingTask = await helper.RegisterTask();
            if (signalingTask == null)
            {
                var message = new MessageDialog("The signaling service is required.");
                await message.ShowAsync();
                return;
            }

            var socketService = new SignalingSocketService(signalingTask.TaskId);
            var connected = socketService.Connect(SignalingSettings.SignalingServerHost, int.Parse(SignalingSettings.SignalingServerPort));

            _signalingClient = new SignalingClient(new SignalingSocketService(signalingTask.TaskId));
            RegistrationViewModel = new RegistrationViewModel(_uiDispatcher, _signalingClient);
            IsRegistrationCompleted = (RegistrationSettings.Name == null);
            if (!IsRegistrationCompleted) return;
        }
    }
}
