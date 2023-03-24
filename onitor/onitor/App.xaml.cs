using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.System.Profile;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;
using Windows.UI.StartScreen;

namespace Onitor
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary> 

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public App()
        {
            this.InitializeComponent();

            if (!localSettings.Values.ContainsKey("theme"))
            {
                localSettings.Values.Add("theme", "WD");
            }
            else
            {
                string theme = localSettings.Values["theme"].ToString();
                if (theme == "WD")
                {
                    UISettings DefaultTheme = new UISettings();
                    string uiTheme = DefaultTheme.GetColorValue(UIColorType.Background).ToString();
                    if (uiTheme == "#FF000000")
                    {
                        RequestedTheme = ApplicationTheme.Dark;
                    }
                    else if (uiTheme == "#FFFFFFFF")
                    {
                        RequestedTheme = ApplicationTheme.Light;
                    }
                }
                else if (theme == "Dark")
                {
                    RequestedTheme = ApplicationTheme.Dark;
                }
                else if (theme == "Light")
                {
                    RequestedTheme = ApplicationTheme.Light;
                }
            }

            if (!localSettings.Values.ContainsKey("transparency"))
            {
                localSettings.Values.Add("transparency", "1");
            }

            if (!localSettings.Values.ContainsKey("WebViewTheme"))
            {
                localSettings.Values.Add("WebViewTheme", "Default");
            }

            if (!localSettings.Values.ContainsKey("titleBarColor"))
            {
                localSettings.Values.Add("titleBarColor", "0");
            }

            if (!localSettings.Values.ContainsKey("TabBarPosition"))
            {
                localSettings.Values.Add("TabBarPosition", "0");
            }

            if (!localSettings.Values.ContainsKey("homePage"))
            {
                localSettings.Values.Add("homePage", "about:home");
            }

            if (!localSettings.Values.ContainsKey("SearchEngine"))
            {
                localSettings.Values.Add("SearchEngine", "Bing");
            }

            if (!localSettings.Values.ContainsKey("vibrate"))
            {
                localSettings.Values.Add("vibrate", "1");
            }

            if (!localSettings.Values.ContainsKey("DeviceVersion"))
            {
                if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                {
                    localSettings.Values.Add("DeviceVersion", "Mobile");
                }
                else
                {
                    localSettings.Values.Add("DeviceVersion", "Desktop");
                }
            }
            if (!localSettings.Values.ContainsKey("javaScript"))
            {
                localSettings.Values.Add("javaScript", "1");
            }
            
            if (!localSettings.Values.ContainsKey("WebNotificationPermission"))
            {
                localSettings.Values.Add("WebNotificationPermission", "1");
            }

            if (!localSettings.Values.ContainsKey("LocationPermission"))
            {
                localSettings.Values.Add("LocationPermission", "1");
            }

            if (!localSettings.Values.ContainsKey("MediaPermission"))
            {
                localSettings.Values.Add("MediaPermission", "1");
            }

            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    // Do not repeat app initialization when the Window already has content, 
                    // just ensure that the window is active 

                    // CoreApplication.EnablePrelaunch was introduced in Windows 10 version 1607
                    bool canEnablePrelaunch = ApiInformation.IsMethodPresent("Windows.ApplicationModel.Core.CoreApplication", "EnablePrelaunch");

                    if (!(Window.Current.Content is Frame rootFrame))
                    {
                        // Create a Frame to act as the navigation context and navigate to the first page 
                        rootFrame = new Frame();
                        // Associate the frame with a SuspensionManager key 
                        if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                        {
                            // Restore the saved session state only when appropriate 
                        }

                        // Place the frame in the current Window 
                        Window.Current.Content = rootFrame;
                    }

                    if (e.PrelaunchActivated == false)
                    {
                        // On Windows 10 version 1607 or later, this code signals that this app wants to participate in prelaunch
                        if (canEnablePrelaunch)
                        {
                            TryEnablePrelaunch();
                        }

                        // TODO: This is not a prelaunch activation. Perform operations which
                        // assume that the user explicitly launched the app such as updating
                        // the online presence of the user on a social network, updating a
                        // what's new feed, etc.

                        // When the navigation stack isn't restored navigate to the first page,
                        // configuring the new page by passing required information as a navigation
                        // parameter
                        //MainPage is always in rootFrame so we don't have to worry about restoring the navigation state on resume
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }

                    if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 2))
                    {
                        ClearRUL();
                    }

                    // Ensure the current window is active
                    Window.Current.Activate();
                });
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {

                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;

                    if (args.Kind == ActivationKind.Protocol)
                    {
                        // TODO: Handle URI activation
                        // The received URI is eventArgs.Uri.AbsoluteUri
                        Frame rootFrame = Window.Current.Content as Frame;
                        var uri = eventArgs.Uri.AbsoluteUri;
                        onitor.Classes.LaunchURIHelper.launchURI = uri;

                        // Do not repeat app initialization when the Window already has content,
                        // just ensure that the window is active
                        if (rootFrame == null)
                        {
                            // Create a Frame to act as the navigation context and navigate to the first page
                            rootFrame = new Frame();
                            rootFrame.NavigationFailed += OnNavigationFailed;
                            rootFrame.Navigate(typeof(MainPage), onitor.Classes.LaunchURIHelper.launchURI);
                            // Place the frame in the current Window
                            Window.Current.Content = rootFrame;
                        } else
                        {
                            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
                            
                            rootFrame.NavigationFailed += OnNavigationFailed;
                            rootFrame.Navigate(typeof(MainPage), onitor.Classes.LaunchURIHelper.launchURI);
                            // Place the frame in the current Window
                            Window.Current.Content = rootFrame;
                        }
                        // Do not repeat app initialization when the Window already has content, 
                        // just ensure that the window is active 
                        
                    }
                    Window.Current.Activate();
                });
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            // Do not repeat app initialization when the Window already has content, 
            // just ensure that the window is active 
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Create a Frame to act as the navigation context and navigate to the first page 
                rootFrame = new Frame();
                // Associate the frame with a SuspensionManager key 
                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate 
                }

                // Place the frame in the current Window 
                Window.Current.Content = rootFrame;
            }

            rootFrame.Navigate(typeof(MainPage), args);
            if (rootFrame.Content == null)
            {
                if (!rootFrame.Navigate(typeof(MainPage)))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active 
            Window.Current.Activate();
        }

        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            // Do not repeat app initialization when the Window already has content, 
            // just ensure that the window is active 
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Create a Frame to act as the navigation context and navigate to the first page 
                rootFrame = new Frame();
                // Associate the frame with a SuspensionManager key 
                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate 
                }

                // Place the frame in the current Window 
                Window.Current.Content = rootFrame;
            }

            rootFrame.Navigate(typeof(ShareTargetPage), args);
            if (rootFrame.Content == null)
            {
                if (!rootFrame.Navigate(typeof(ShareTargetPage)))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active 
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Encapsulates the call to CoreApplication.EnablePrelaunch() so that the JIT
        /// won't encounter that call (and prevent the app from running when it doesn't
        /// find it), unless this method gets called. This method should only
        /// be called when the caller determines that we are running on a system that
        /// supports CoreApplication.EnablePrelaunch().
        /// </summary>
        private void TryEnablePrelaunch()
        {
            Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);
        }

        private async void ClearRUL()
        {
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Clear();
            Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Clear();

            if(JumpList.IsSupported() == true)
            {
                // Get the app's jump list.
                JumpList jumpList = await JumpList.LoadCurrentAsync();

                // Disable the system-managed jump list group.
                jumpList.SystemGroupKind = JumpListSystemGroupKind.None;

                // Remove any previously added custom jump list items.
                jumpList.Items.Clear();

                // Save the changes to the app's jump list.
                await jumpList.SaveAsync();
            }
        }
    }
}
