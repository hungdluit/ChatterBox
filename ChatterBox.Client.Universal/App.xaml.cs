using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Presentation.Shared.ViewModels;
using ChatterBox.Client.Presentation.Shared.Views;
using ChatterBox.Client.Settings;
using ChatterBox.Client.Signaling;
using ChatterBox.Client.Signaling.Shared;
using ChatterBox.Client.Universal.Common;
using ChatterBox.Client.Universal.Helpers;
using ChatterBox.Client.Universal.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Unity;

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
                WindowsCollectors.Session);
            InitializeComponent();
            Suspending += OnSuspending;
        }

        public UnityContainer Container { get; } = new UnityContainer();

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Container.RegisterInstance(CoreApplication.MainView.CoreWindow.Dispatcher);

            var registerAgain = false;
            if (e.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                e.PreviousExecutionState == ApplicationExecutionState.NotRunning ||
                e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                RegistrationSettings.Reset();
                registerAgain = true;
            }

            var helper = new SignalingTaskHelper();
            var signalingTask = await helper.GetSignalingTaskRegistration(registerAgain);
            if (signalingTask == null)
            {
                var message = new MessageDialog("The signaling task is required.");
                await message.ShowAsync();
                return;
            }

            Container
                .RegisterType<ForegroundAppServiceClient>(new ContainerControlledLifetimeManager())
                .RegisterType<ISignaledDataUpdateNotifier, NoActionSignaledDataUpdateNotifier>()
                .RegisterType<ISignalingSocketService, SignalingSocketService>(
                    new ContainerControlledLifetimeManager(), new InjectionConstructor(signalingTask.TaskId))
                .RegisterType<ISignalingUpdateService, SignalingUpdateService>(new ContainerControlledLifetimeManager());


            var client = Container.Resolve<ForegroundAppServiceClient>();
            await client.Connect();

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
                rootFrame.Navigate(typeof (MainView), Container.Resolve<MainViewModel>());
            }
            // Ensure the current window is active
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
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            LayoutService.Instance.LayoutRoot = args.Window;
            base.OnWindowCreated(args);
        }
    }
}