using System;
using Windows.ApplicationModel.Background;
using Windows.UI.Core;
using Windows.UI.Popups;
using ChatterBox.Client.Presentation.Shared.ViewModels;
using ChatterBox.Client.Settings;
using ChatterBox.Client.Signaling;
using ChatterBox.Client.Signaling.Shared;
using ChatterBox.Client.Tasks.Signaling.Universal;
using ChatterBox.Client.Universal.Helpers;
using ChatterBox.Client.Presentation.Shared.Models;

namespace ChatterBox.Client.Universal.ViewModels
{
    public class MainViewModel : MainViewModelBase
    {
        private SignalingClient _signalingClient;

        public MainViewModel(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
            WelcomeViewModel.Domain = RegistrationSettings.Domain;
            WelcomeViewModel.OnCompleted += WelcomeCompleted;

            ContactsViewModel.ContactSelected += ContactSelected;

            ChatViewModel.OnCompleted += ChatClosed;
            CallViewModel.OnCompleted += CallClosed;
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
            signalingTask.Completed -= SignalingTask_Completed;
            signalingTask.Completed += SignalingTask_Completed;


            var socketService = new SignalingSocketService(signalingTask.TaskId);
            _signalingClient = new SignalingClient(socketService);

            var isConnected = _signalingClient.CheckConnection();
            if (!isConnected)
            {
                var connected = _signalingClient.Connect();
                if (!connected)
                {
                    var dialog = new MessageDialog("Connecting to the server failed.");
                    await dialog.ShowAsync();
                    return;
                }
            }

            IsWelcomeOpened = true;
        }


        private void WelcomeCompleted()
        {
            _signalingClient.RegisterUsingSettings();
            IsWelcomeOpened = false;
            IsContactsOpened = true;
        }

        private void ContactSelected(ContactModel contact)
        {
            IsContactsOpened = false;
            IsChatOpened = true;
            ChatViewModel.OnNavigatedTo(contact);
        }

        private void ChatClosed()
        {
            IsContactsOpened = true;
            IsChatOpened = false;
        }

        private void CallClosed()
        {
            IsCallOpened = false;
        }

        private void SignalingTask_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            var peers = ContactService.Peers;
            RunOnUiThread(() =>
            {
                ContactsViewModel.Update(peers);
            });
        }
    }
}
