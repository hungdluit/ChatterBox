using ChatterBox.Client.Common.Background;
using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Notifications;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Presentation.Shared.ViewModels;
using ChatterBox.Client.Presentation.Shared.Views;
using ChatterBox.Client.Voip;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Client.Win8dot1.Channels;
using ChatterBox.Client.Win8dot1.Services;
using ChatterBox.Client.Win8dot1.Voip;
using ChatterBox.Common.Communication.Contracts;
using Microsoft.Practices.Unity;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Win8dot1
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        ///     Initializes the singleton Application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        public UnityContainer Container { get; } = new UnityContainer();

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used when the application is launched to open a specific file, to display
        ///     search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            ToastNotificationLaunchArguments launchArg = null;
            if (args.Arguments != null && args.Arguments != String.Empty)
            {
                launchArg = ToastNotificationLaunchArguments.FromXmlString(args.Arguments);
            }

            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                Resume();
                ProcessLaunchArgument(launchArg);

                return;
            }

            Container.RegisterInstance(CoreApplication.MainView.CoreWindow.Dispatcher);

            if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                args.PreviousExecutionState == ApplicationExecutionState.NotRunning ||
                args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                RegistrationSettings.Reset();
            }

            Container
                .RegisterType<ISignalingSocketChannel, SignalingSocketChannel>(new ContainerControlledLifetimeManager())
                .RegisterType<ISignalingSocketOperation, SignalingSocketOperation>(new ContainerControlledLifetimeManager())
                .RegisterType<ISignalingSocketService, SignalingSocketService>(new ContainerControlledLifetimeManager())
                .RegisterType<SignalingClient>(new ContainerControlledLifetimeManager())
                .RegisterType<IVideoRenderHelper, VideoRenderHelper>()
                .RegisterType<IForegroundChannel, ForegroundSignalingUpdateService>(new ContainerControlledLifetimeManager())
                .RegisterType<IForegroundUpdateService, ForegroundSignalingUpdateService>(new ContainerControlledLifetimeManager())
                .RegisterType<IClientChannel, ClientChannel>(new ContainerControlledLifetimeManager())
                .RegisterType<ISocketConnection, SocketConnection>(new ContainerControlledLifetimeManager())
                .RegisterType<IVoipCoordinator, VoipCoordinator>()
                .RegisterType<IHub, Voip.Hub>(new ContainerControlledLifetimeManager())
                .RegisterType<VoipContext>(new ContainerControlledLifetimeManager())
                .RegisterType<IVoipChannel, VoipChannel>(new ContainerControlledLifetimeManager())
                .RegisterType<ISocketConnection, SocketConnection>(new ContainerControlledLifetimeManager())
                .RegisterType<IWebRTCSettingsService, WebRTCSettingsService>()
                .RegisterInstance<MainViewModel>(Container.Resolve<MainViewModel>(), new ContainerControlledLifetimeManager());

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored or there are launch arguments
                // indicating an alternate launch (e.g.: via toast or secondary tile), 
                // navigate to the appropriate page, configuring the new page by passing required 
                // information as a navigation parameter
                if (!rootFrame.Navigate(typeof(MainView), Container.Resolve<MainViewModel>()))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            var currentStatus = BackgroundExecutionManager.GetAccessStatus();
            if (currentStatus == BackgroundAccessStatus.Unspecified ||
                currentStatus == BackgroundAccessStatus.Denied)
            {
                ShowMessageForMissingLockScreen();
            }
            else
            {
                await RegisterForPush();
            }

            ProcessLaunchArgument(launchArg);
            // Ensure the current window is active
            Window.Current.Activate();
        }

        private async void ShowMessageForMissingLockScreen()
        {
            var msgDialog = new MessageDialog("Please add ChatterBox application to Lock Screen (PC Settings -> PC and devices -> Lock screen -> Lock screen apps)",
                                                            "Need lock screen presence");
            msgDialog.Commands.Add(new UICommand("OK", (cmd) => Current.Exit()) { Id = 0 });
            msgDialog.DefaultCommandIndex = 0;
            msgDialog.CancelCommandIndex = 0;
            Debug.WriteLine("Message dialog for missing lock screen showed");
            await msgDialog.ShowAsync();

        }

        private static async System.Threading.Tasks.Task RegisterForPush(bool registerAgain = true)
        {
            PushNotificationHelper.RegisterPushNotificationChannel();

            var helper = new TaskHelper();
            var pushNotificationTask = await helper.RegisterTask(nameof(PushNotificationTask), typeof(PushNotificationTask).FullName, new PushNotificationTrigger(), registerAgain);
            if (pushNotificationTask == null)
            {
                Debug.WriteLine("Push notification background task is not started");
            }
        }

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            LayoutService.Instance.LayoutRoot = args.Window;
            base.OnWindowCreated(args);
        }

        private void Resume()
        {
            if (Container.IsRegistered(typeof(ISocketConnection)))
            {
                if (!Container.Resolve<ISocketConnection>().IsConnected)
                    Container.Resolve<ISocketConnection>().Connect();
            }
            Window.Current.Activate();
        }

        private void ProcessLaunchArgument(ToastNotificationLaunchArguments launchArg)
        {
            if (launchArg != null)
            {
                switch (launchArg.type)
                {
                    case NotificationType.InstantMessage:
                        Container.Resolve<MainViewModel>().ContactsViewModel.SelectConversation(
                            (string)launchArg.arguments[ArgumentType.FromId]);
                        break;
                }
            }
        }
    }
}