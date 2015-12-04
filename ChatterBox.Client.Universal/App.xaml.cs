using System;
using System.Windows;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Presentation.Shared.ViewModels;
using ChatterBox.Client.Presentation.Shared.Views;
using ChatterBox.Client.Universal.Services;
using ChatterBox.Common.Communication.Contracts;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Unity;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Common.Notifications;
using ChatterBox.Client.Common.Background;
using Windows.ApplicationModel.Background;
using ChatterBox.Client.Universal.Background.Tasks;
using Microsoft.ApplicationInsights.DataContracts;

namespace ChatterBox.Client.Universal
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            WindowsAppInitializer.InitializeAsync(
                WindowsCollectors.Metadata |
                WindowsCollectors.PageView |
                WindowsCollectors.Session);
            UnhandledException += CurrentDomain_UnhandledException;
            InitializeComponent();
            Suspending += OnSuspending;
            Resuming += OnResuming;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ExceptionTelemetry excTelemetry = new ExceptionTelemetry((Exception)e.Exception);
            excTelemetry.SeverityLevel = SeverityLevel.Critical;
            excTelemetry.HandledAt = ExceptionHandledAt.Unhandled;
            var telemetry = new TelemetryClient();
            telemetry.TrackException(excTelemetry);

            telemetry.Flush();
        }

        public UnityContainer Container { get; } = new UnityContainer();

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
			if (e.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                Resume();
                return;
            }

            //Register IoC types
            if (!Container.IsRegistered<HubClient>())
            {
                Container.RegisterInstance(CoreApplication.MainView.CoreWindow.Dispatcher);
                Container.RegisterType<TaskHelper>(new ContainerControlledLifetimeManager());
                Container.RegisterType<HubClient>(new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IForegroundUpdateService>(Container.Resolve<HubClient>(),new ContainerControlledLifetimeManager());
                Container.RegisterInstance<ISignalingSocketChannel>(Container.Resolve<HubClient>(),new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IClientChannel>(Container.Resolve<HubClient>(),new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IVoipChannel>(Container.Resolve<HubClient>(),new ContainerControlledLifetimeManager());
                Container.RegisterType<ISocketConnection, SocketConnection>(new ContainerControlledLifetimeManager());
            }

            if (!Container.Resolve<HubClient>().IsConnected)
            {
                var client = Container.Resolve<HubClient>();
                await client.Connect().ContinueWith(connected =>
                {
                    if (connected.Result)
                    {
                        client.SetForegroundProcessId(
                            ChatterBox.Client.WebRTCSwapChainPanel.WebRTCSwapChainPanel.CurrentProcessId);
                    }
                });
            }

            await RegisterForPush(Container.Resolve<TaskHelper>());

            var signalingTask = await RegisterSignalingTask(Container.Resolve<TaskHelper>(), false);
            if (signalingTask == null)
            {
                var message = new MessageDialog("The signaling task is required.");
                await message.ShowAsync();
                return;
            }

            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainView), Container.Resolve<MainViewModel>());
            }

            // Ensure the current window is active
            Window.Current.Activate();
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

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
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
            //ShowDialog("OnSuspending");
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private void OnResuming(object sender, object e)
        {
            Resume();
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            LayoutService.Instance.LayoutRoot = args.Window;
            base.OnWindowCreated(args);
        }

        private static async System.Threading.Tasks.Task RegisterForPush(TaskHelper helper, bool registerAgain = true)
        {
            try
            {
                PushNotificationHelper.RegisterPushNotificationChannel();

                var pushNotificationTask = await helper.RegisterTask(nameof(PushNotificationTask), typeof(PushNotificationTask).FullName, new PushNotificationTrigger(), registerAgain).AsTask();
                if (pushNotificationTask == null)
                {
                    Debug.WriteLine("Push notification background task is not started");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to register for push notification. Error: {e.Message}");
            }
        }

        private static async System.Threading.Tasks.Task<IBackgroundTaskRegistration> RegisterSignalingTask(TaskHelper helper, bool registerAgain = true)
        {
            if (helper.GetTask(nameof(SignalingTask)) == null)
            {
                return await helper.RegisterTask(nameof(SignalingTask), typeof(SignalingTask).FullName, new SocketActivityTrigger(), registerAgain).AsTask();
            }

            return null;
        }

        public async void ShowDialog(string message)
        {
            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }
    }
}