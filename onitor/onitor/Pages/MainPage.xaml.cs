using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.Storage;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Display;
using UnitedCodebase.Brushes;
using Windows.UI.Xaml.Printing;
using Windows.Graphics.Printing;
using UnitedCodebase.WinRT;
using Windows.ApplicationModel.Activation;
using System.Text.RegularExpressions;
using UnitedCodebase.Classes;
using Windows.System.Power;
using Windows.Media;
using Windows.Devices.Geolocation;
using Windows.Networking.Connectivity;
using Windows.Storage.Pickers;
using System.Runtime.Serialization.Json;
using System.IO;
using Windows.Phone.UI.Input;
using Windows.UI.Input;
using Windows.System.Profile;
using Windows.Storage.Streams;
using System.Diagnostics;
using System.Collections.Generic;
using DataManager;
using Windows.UI.Popups;
using Windows.Web.Http;
using onitor.Classes;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using MemUsageHelper;
using FileSize;
using WebViewComponents;
using SharedLibrary;
using Windows.ApplicationModel.ExtendedExecution.Foreground;

namespace Onitor
{
    public sealed partial class MainPage : Page
    {
        SystemNavigationManager currentView =
            SystemNavigationManager.GetForCurrentView();

        ApplicationView appView = ApplicationView.GetForCurrentView();
        CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

        public static WebView currentWebView;
        private WebieHandler webieHandlerUI = new WebieHandler();

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private Type NavigatedPageType;

        string[] CommonSites = { "youtube.com", "google.com", "facebook.com", "bing.com", "twitter.com", "instagram.com" };
        ObservableCollection<Item> dataList = new ObservableCollection<Item>();

        ObservableCollection<Bookmark> favs = new ObservableCollection<Bookmark>();

        // SystemMediaTransportControls MediaControls = SystemMediaTransportControls.GetForCurrentView();

        private DispatcherTimer timer;

        private PrintManager printMan;
        private PrintDocument printDoc;
        private IPrintDocumentSource printDocSource;

        public static ObservableCollection<Bookmark> historyList;
        static List<string> historyListURLs = new List<string>();



        public static int TotalAdsBlocked;
        public static int CurrentSessionAdsBlocked;

        string UserSelectedUserAgent { get; set; }
        ObservableCollection<string> LanguageList;
        public static double FilePercent = 100;
        public static string FileInfo = string.Empty;


        public MainPage()
        {
            try
            {
                this.InitializeComponent();

                ExtendedExecution();


                StaticHandlers.Notify += CheckNotifyData;
                StaticHandlers.XEvents += async (s, e) =>
                {
                    try
                    {
                        var xEventData = (XEventData)e;
                        if (xEventData != null)
                        {
                            if (xEventData.type == "loaded")
                            {
                                /* await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async() =>
                                 {
                                     await Task.Delay(5000);
                                     if (currentWebView.IsLoaded == false)
                                     {

                                         currentWebView.Stop();

                                     }
                                 }); */
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                };
                StaticHandlers.AddToConsole += async (s, e) =>
                {
                    var consoleOutput = (ConsoleOutput)e;
                    Debug.WriteLine($"[Type] {consoleOutput.type}\n[Message] {consoleOutput.message}\n\n");
                    if (GlobalLocalSettings.JSConsole != null)
                    {
                        if (GlobalLocalSettings.JSConsole == "enabled")
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                JSConsoleOutput.Text += $"[Type] {consoleOutput.type}\n[Message] {consoleOutput.message}\n\n";
                            });
                        }
                    }

                };

                coreTitleBar.ExtendViewIntoTitleBar = true;

                Window.Current.CoreWindow.Activated += CoreWindow_Activated;
                Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;

                InputPane pane = InputPane.GetForCurrentView();
                pane.Showing += Pane_Showing;
                pane.Hiding += Pane_Hiding;

                // Mobile customization
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    StatusBar statusBar = StatusBar.GetForCurrentView();
                    if (statusBar != null)
                    {
                        statusBar.BackgroundOpacity = 0;

                        if (GlobalLocalSettings.theme != "WD")
                        {
                            if (GlobalLocalSettings.theme == "Dark")
                            {
                                statusBar.ForegroundColor = Colors.LightGray;
                            }
                            else if (GlobalLocalSettings.theme == "Light")
                            {
                                statusBar.ForegroundColor = Colors.DarkSlateGray;
                            }
                        }

                        /* TitleTextBlock.FontSize = 13;
                         TitleTextBlock.Margin = new Thickness(0, 0.6, 1, 0);
                         MiddleAppTitleBar.Margin = new Thickness(0, -statusBar.OccludedRect.Height, 0, 0);
                         MiddleAppTitleBar.Height = statusBar.OccludedRect.Height; */

                        ContentGrid.Margin = new Thickness(0, statusBar.OccludedRect.Top, 0, 0);
                    }
                }


                if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                {
                    HardwareButtons.CameraHalfPressed += HardwareButtons_CameraHalfPressed;
                }

                this.NavigationCacheMode = NavigationCacheMode.Required;

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                {
                    if (titleBar != null)
                    {
                        ContentGrid.Margin = new Thickness(0, coreTitleBar.Height, 0, 0);
                        FindName(nameof(LeftAppTitleBar));
                        // Window.Current.SetTitleBar(MiddleAppTitleBar);
                    }
                }

                coreTitleBar.LayoutMetricsChanged += coreTitleBar_LayoutMetricsChanged;


                appView.VisibleBoundsChanged += appView_VisibleBoundsChanged;

                // TitleTextBlock.Text = ApplicationView.GetForCurrentView().Title;





                // timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
                // timer.Tick += CheckMedia;

                PowerManager.EnergySaverStatusChanged += PowerManager_EnergySaverStatusChanged;
                MemoryManager.AppMemoryUsageIncreased += MemoryManager_AppMemoryUsageIncreased;
                // MediaControls.ButtonPressed += MediaControls_ButtonPressed;

                MakeDesign();
                MakeKeyAccelerators();
                RetrieveHistory();

                PopulateTranslateComboBoxes();



                PopulatePageSettings();


            }
            catch (Exception ex)
            {


                var CustErr = new MessageDialog($"{ex.Message}\n\n{ex.StackTrace}\n\n{ex.Source}\n\n{LaunchURIHelper.launchURI}");
                CustErr.Commands.Add(new UICommand("Close"));
                CustErr.ShowAsync();

            }
        }

        ExtendedExecutionForegroundSession extendedExecution = new ExtendedExecutionForegroundSession();
        private async void ExtendedExecution()
        {
            Application.Current.Suspending += Current_Suspending;
            
            extendedExecution.Reason = ExtendedExecutionForegroundReason.Unconstrained;
            extendedExecution.Description = "Browser tasks";
            var result = await extendedExecution.RequestExtensionAsync();
            if (result == ExtendedExecutionForegroundResult.Allowed)
            {

            } else
            {
                var CustErr = new MessageDialog($"Extended execution not allowed");
                CustErr.Commands.Add(new UICommand("Close"));
                await CustErr.ShowAsync();
            }
        }

        private void Current_Suspending(object sender, SuspendingEventArgs e)
        {
            
        }

        private void MemoryManager_AppMemoryUsageIncreased(object sender, object e)
        {
            GC.Collect();
        }

        /*        private async void MediaControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
                {
                    if (args.Button == SystemMediaTransportControlsButton.Play)
                    {
                        await Task.Run(async () =>
                        {
                            await currentWebView.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                            {
                                foreach (WebViewPivotItem item in PivotMain.Items)
                                {
                                    if (item.WebViewCore.IsPageHaveMedia)
                                    {
                                        item.WebViewCore.WebView.PlayMedia();
                                        MediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                                        break;
                                    }
                                }
                            });
                        });
                    }
                    else if (args.Button == SystemMediaTransportControlsButton.Pause)
                    {
                        await Task.Run(async () =>
                        {
                            await currentWebView.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                            {
                                foreach (WebViewPivotItem item in PivotMain.Items)
                                {
                                    if (item.WebViewCore.IsPageHaveMedia)
                                    {
                                        item.WebViewCore.WebView.PauseMedia();
                                        MediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                                        break;
                                    }
                                }
                            });
                        });
                    }
                } */

        private async void PowerManager_EnergySaverStatusChanged(object sender, object e)
        {
            CoreWindow window = CoreWindow.GetForCurrentThread();
            await Task.Run(async () =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (PowerManager.EnergySaverStatus == EnergySaverStatus.On)
                    {
                        timer.Interval = TimeSpan.FromSeconds(4);
                    }
                    else
                    {
                        timer.Interval = TimeSpan.FromSeconds(2);
                    }
                });
            });
        }

        private void coreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (!appView.IsFullScreenMode)
            {
                ContentGrid.Margin = new Thickness(0, sender.Height, 0, 0);
                Debug.Write(sender.Height);
            }

            /*  TitleTextBlock.Margin = new Thickness(0, 5.5, 32, 0);

              MiddleAppTitleBar.Margin = new Thickness(32, 0, 0, 0);
              MiddleAppTitleBar.Height = sender.Height; */
        }




        private void appView_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            MainCommandBar.IsOpen = false;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar") && !sender.IsFullScreenMode)
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();

                /* MiddleAppTitleBar.Height = statusBar.OccludedRect.Height;
                 MiddleAppTitleBar.Width = statusBar.OccludedRect.Width;

                 TitleTextBlock.Visibility = Visibility.Collapsed;

                 DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
                 if (displayInformation.CurrentOrientation == DisplayOrientations.Landscape)
                 {
                     MiddleAppTitleBar.HorizontalAlignment = HorizontalAlignment.Left;
                     MiddleAppTitleBar.Margin = new Thickness(-MiddleAppTitleBar.Width, 0, 0, -sender.VisibleBounds.Bottom);
                 }
                 else if (displayInformation.CurrentOrientation == DisplayOrientations.LandscapeFlipped)
                 {
                     MiddleAppTitleBar.HorizontalAlignment = HorizontalAlignment.Right;
                     MiddleAppTitleBar.Margin = new Thickness(0, 0, -MiddleAppTitleBar.Width, -sender.VisibleBounds.Bottom);
                 }
                 else
                 {
                     MiddleAppTitleBar.HorizontalAlignment = HorizontalAlignment.Stretch;
                     MiddleAppTitleBar.Margin = new Thickness(0, 0, 0, 0);
                     TitleTextBlock.Visibility = Visibility.Visible;
                 } */

                ContentGrid.Margin = new Thickness(0, statusBar.OccludedRect.Top, 0, 0);
            }

            GC.Collect(0, GCCollectionMode.Optimized);
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            ShareButton.IsEnabled = true;

            if (currentWebView != null)
            {
                if (!appView.IsFullScreenMode)
                {
                    PivotMain.SelectedWebViewItem.Margin = new Thickness(
                        0, 0, 0, 0);

                    currentWebView.Focus(FocusState.Programmatic);
                }
            }

            AddressAutoSuggestBox.IsSuggestionListOpen = false;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }

        private async void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs e)
        {
            if (appView.IsFullScreenMode)
            {
                if (PivotMain.Style == null)
                {
                    ContentGrid.Margin = new Thickness(0, -100, 0, 0);
                }
                else
                {
                    ContentGrid.Margin = new Thickness(0, 0, 0, -48);
                }
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }
            else
            {
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                {
                    if (titleBar != null)
                    {
                        ContentGrid.Margin = new Thickness(0, coreTitleBar.Height, 0, 0);
                    }
                }

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    StatusBar statusBar = StatusBar.GetForCurrentView();
                    if (statusBar != null)
                    {
                        ContentGrid.Margin = new Thickness(0, statusBar.OccludedRect.Top, 0, 0);
                    }
                }
                if (currentWebView != null)
                {
                    AlignTopBar();
                }


                if (currentWebView != null && currentWebView.ContainsFullScreenElement)
                {
                    await currentWebView.InvokeScriptAsync("eval", new string[] { @"if (document.webkitExitFullscreen) { document.webkitExitFullscreen(); }" });
                }
               
                e.Handled = true;

                // MiddleAppTitleBar.Opacity = 1;
            }

            GC.Collect();
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;

            if (appView.IsFullScreenMode)
            {
                appView.ExitFullScreenMode();
            }
            else
            {
                Frame frame = Window.Current.Content as Frame;
                if (!frame.CanGoBack && currentWebView.CanGoBack)
                {
                    currentWebView.GoBack();
                }
                else
                {
                    if (PivotMain.Items.Count > 1)
                    {
                        CloseOneTab(PivotMain.SelectedWebViewItem);
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
            }
        }

        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            if (args.CurrentPoint.Properties.IsXButton1Pressed)
            {
                args.Handled = true;

                if (currentWebView.CanGoBack) currentWebView.GoBack();
            }
            else if (args.CurrentPoint.Properties.IsXButton2Pressed)
            {
                args.Handled = true;

                if (currentWebView.CanGoForward) currentWebView.GoForward();
            }
        }

        private void HardwareButtons_CameraHalfPressed(object sender, CameraEventArgs e)
        {
            InputPane pane = InputPane.GetForCurrentView();
            if (appView.IsFullScreenMode)
            {
                appView.ExitFullScreenMode();
            }
        }

        private void Pane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (currentWebView != null)
            {
                MainGrid.Margin = new Thickness(0);
                PivotMain.Margin = new Thickness(0, 0, 0, args.OccludedRect.Height);

                if (FocusManager.GetFocusedElement() == currentWebView)
                {
                    AlignTopBar();
                }
            }
        }

        private void Pane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (currentWebView != null)
            {
                MainGrid.Margin = new Thickness(0);
                PivotMain.Margin = new Thickness(0, 0, 0, args.OccludedRect.Height);
                AlignTopBar();
            }
        }

        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;

            request.Data.Properties.Title = "URL Share";
            request.Data.Properties.Description = "Share URL from site";

            request.Data.SetWebLink(PivotMain.SelectedWebViewItem.WebViewCore.URL);

            if (PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri.EndsWith(".txt"))
            {
                Windows.Web.Http.HttpClient http = new Windows.Web.Http.HttpClient();
                Windows.Web.Http.HttpResponseMessage response = await http.GetAsync(currentWebView.Source);
                string source = await response.Content.ReadAsStringAsync();
                request.Data.SetText(source);
            }
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            CoreWindow window = CoreWindow.GetForCurrentThread();
            await Task.Run(async () =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
                    if (profile != null && profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
                    {
                        ConnectionCost cost = profile.GetConnectionCost();
                        if ((cost.NetworkCostType == NetworkCostType.Unknown || cost.NetworkCostType == NetworkCostType.Unrestricted)
                            && ((currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NoInternet.html#") ||
                            currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NotFound.html#"))
                            && !string.IsNullOrEmpty(currentWebView.Source.AbsoluteUri.Split('#')[1])))
                        {
                            Refresh();
                        }
                    }
                });
            });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Mobile customization
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 0;
                }
            }

            if (await ApplicationData.Current.TemporaryFolder.TryGetItemAsync("LocalFiles") == null)
                await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("LocalFiles");

           
            
            if (GlobalLocalSettings.DeviceVersion == "Mobile")
            {
                // UserAgentManager.ChangeUserAgent(UserAgentManager.DeviceMode.Mobile);
                if (GlobalLocalSettings.SavedUserAgent != null)
                {

                    UserSelectedUserAgent = UserAgent.ModifyUserAgent(false, GlobalLocalSettings.SavedUserAgent);
                }
                else
                {
                    UserSelectedUserAgent = UserAgent.ModifyUserAgent(false, GlobalLocalSettings.SavedUserAgent);
                }
                NewWindowButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (GlobalLocalSettings.SavedUserAgent != null)
                {

                    UserSelectedUserAgent = UserAgent.ModifyUserAgent(true, GlobalLocalSettings.SavedUserAgent);
                }
                else
                {
                    UserSelectedUserAgent = UserAgent.ModifyUserAgent(true, GlobalLocalSettings.SavedUserAgent);
                }
            }
            Debug.WriteLine(UserSelectedUserAgent);
            UserAgent.SetUserAgent(UserSelectedUserAgent);





            //gets preferred theme for supported websites
            //also the enablement of JavaScript
            foreach (WebViewPivotItem item in PivotMain.Items)
            {
                if (GlobalLocalSettings.WebViewTheme == "Default")
                {
                    UISettings DefaultTheme = new UISettings();
                    string uiTheme = DefaultTheme.GetColorValue(UIColorType.Background).ToString();
                    if (uiTheme == "#FF000000")
                    {
                        item.WebViewCore.Theme = WebViewCore.WebViewTheme.Dark;
                    }
                    else if (uiTheme == "#FFFFFFFF")
                    {
                        item.WebViewCore.Theme = WebViewCore.WebViewTheme.Light;
                    }
                }
                else if (GlobalLocalSettings.WebViewTheme == "Dark")
                {
                    item.WebViewCore.Theme = WebViewCore.WebViewTheme.Dark;
                }
                else if (GlobalLocalSettings.WebViewTheme == "Light")
                {
                    item.WebViewCore.Theme = WebViewCore.WebViewTheme.Light;
                }

                if (GlobalLocalSettings.javaScript == "1")
                {
                    item.WebViewCore.WebView.Settings.IsJavaScriptEnabled = true;
                }
                else
                {
                    item.WebViewCore.WebView.Settings.IsJavaScriptEnabled = false;
                }
            }

            // timer.Start();

            //parses bookmarks
            if ((await ApplicationData.Current.LocalFolder.TryGetItemAsync("favorites.json")) == null)
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("favorites.json");
                await FileIO.WriteTextAsync(file, "[]");
            }

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(favs.GetType());
            using (Stream stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(
                "favorites.json"))
            {
                favs = serializer.ReadObject(stream) as ObservableCollection<Bookmark>;
            }

            favs.CollectionChanged += Favs_CollectionChanged;

            currentView.BackRequested += CurrentView_BackRequested;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

            SettingsButton.IsEnabled = true;

            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            try
            {
                // Register for PrintTaskRequested event
                printMan = PrintManager.GetForCurrentView();
                printMan.PrintTaskRequested += PrintTaskRequested;

                // Build a PrintDocument and register for callbacks
                printDoc = new PrintDocument();
                printDocSource = printDoc.DocumentSource;
                printDoc.Paginate += Paginate;
                printDoc.GetPreviewPage += GetPreviewPage;
                printDoc.AddPages += AddPages;
            }
            catch (Exception)
            {
            }

            if (GlobalLocalSettings.DebugStats != null)
            {
                DispatcherTimer AppMemTimer = new DispatcherTimer();
                if (GlobalLocalSettings.DebugStats == "enabled")
                {
                    AppMemoryUsage.Visibility = Visibility.Visible;

                    AppMemTimer.Interval = new TimeSpan(500);
                    AppMemTimer.Tick += AppMemTimer_Tick;
                    AppMemTimer.Start();
                }
                else
                {
                    if (AppMemTimer.IsEnabled)
                    {
                        AppMemTimer.Stop();
                    }
                }
            }

            if (GlobalLocalSettings.JSConsole != null)
            {
                if (GlobalLocalSettings.JSConsole == "enabled")
                {
                    ToolsFlyoutButton.Visibility = Visibility.Visible;
                    ShowJSConsoleButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ToolsFlyoutButton.Visibility = Visibility.Collapsed;
                    ShowJSConsoleButton.Visibility = Visibility.Collapsed;
                }
            }

            StorageFolder localFilesFolder = await ApplicationData.Current.TemporaryFolder.GetFolderAsync("LocalFiles");
            if (e.Parameter != null)
            {
                string activationArgs = e.Parameter.ToString();
                if (NavigatedPageType != typeof(SettingsPage))
                {
                    if (Uri.IsWellFormedUriString(activationArgs, UriKind.Absolute))
                    {
                        Navigate(activationArgs, true);

                        if ((activationArgs.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NoInternet.html#")
                            || activationArgs.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NotFound.html#"))
                            && !string.IsNullOrEmpty(activationArgs.Split('#')[1]))
                        {
                            Refresh();
                        }
                    }
                }

                if (PivotMain.Items.Count == 0)
                {
                    if (GlobalLocalSettings.homePage == null)
                    {
                        GlobalLocalSettings.homePage = localSettings.Values["homePage"].ToString();
                    }
                    Navigate(GlobalLocalSettings.homePage, true);
                }
            }

            if (e.Parameter is IActivatedEventArgs args && NavigatedPageType != typeof(SettingsPage))
            {
                if (args.Kind == ActivationKind.File)
                {
                    FileActivatedEventArgs fileArgs = args as FileActivatedEventArgs;
                    foreach (StorageFile file in fileArgs.Files)
                    {
                        await Task.Delay(360);
                        try
                        {
                            await file.CopyAsync(localFilesFolder, file.Name, NameCollisionOption.ReplaceExisting);
                            Navigate("ms-appdata:///temp/LocalFiles/" + file.Name, true);
                        }
                        catch (Exception) { }

                        await Task.Delay(200);
                    }
                }

                if (args.Kind == ActivationKind.Protocol)
                {
                    await Task.Delay(360);

                    ProtocolActivatedEventArgs protocolArgs = args as ProtocolActivatedEventArgs;
                    if (protocolArgs.Uri.AbsoluteUri == "onitor:" && protocolArgs.Data != null)
                    {
                        if (protocolArgs.Data.ContainsKey("link"))
                        {
                            Navigate(protocolArgs.Data["link"].ToString(), true);
                        }
                        else if (protocolArgs.Data.ContainsKey("filePath"))
                        {
                            try
                            {
                                StorageFile file = await StorageFile.GetFileFromPathAsync(protocolArgs.Data["filePath"].ToString());
                                try
                                {
                                    await file.CopyAsync(localFilesFolder, file.Name, NameCollisionOption.ReplaceExisting);
                                    Navigate("ms-appdata:///temp/LocalFiles/" + file.Name, true);
                                }
                                catch (Exception) { }
                            }
                            catch (Exception)
                            {
                                ContentDialog unauthorizedAccessDialog = new ContentDialog()
                                {
                                    Title = "Can't access to file",
                                    Content = "\r\nOnitor can't access the file at " + "\"" + protocolArgs.Data["filePath"].ToString() + "\".",
                                    PrimaryButtonText = "OK"
                                };

                                await unauthorizedAccessDialog.ShowAsync();
                            }
                        }
                        else if (protocolArgs.Data.ContainsKey("html"))
                        {
                            Navigate(protocolArgs.Data["html"].ToString(), true);
                        }
                    }
                    else if (protocolArgs.Uri.AbsoluteUri.StartsWith("onitor:Go"))
                    {
                        Navigate(protocolArgs.Uri.AbsoluteUri.Split(new char[] { '=' }, 2)[1], true);
                    }
                    else if (LaunchURIHelper.launchURI != null)
                    {

                        Navigate(LaunchURIHelper.launchURI, false);

                    }
                }

                if (args.Kind == ActivationKind.ToastNotification)
                {
                    ToastNotificationActivatedEventArgs protocolArgs = args as ToastNotificationActivatedEventArgs;

                    if (ApiResources.GetQueryVariable(protocolArgs.Argument, "action") == "selecttab")
                    {
                        foreach (WebViewPivotItem item in PivotMain.Items)
                        {
                            if (item.Header.ToString() == ApiResources.GetQueryVariable(protocolArgs.Argument, "tab").Replace("_", " "))
                            {
                                await Task.Delay(100);
                                PivotMain.SelectedItem = item;
                                break;
                            }
                        }
                    }
                    else if (ApiResources.GetQueryVariable(protocolArgs.Argument, "action") == "opensettings")
                    {
                        await Launcher.LaunchUriAsync(new Uri("ms-settings:" + ApiResources.GetQueryVariable(protocolArgs.Argument, "section")));
                    }
                }
            }

        }

        private async void AppMemTimer_Tick(object sender, object e)
        {
            long usage = (long)MemUsage.GetCurrentMemUsage();
            //Debug.WriteLine("Usage: " + usage);
            var total = usage.ToFileSize();
            if (usage < 104857600)
            {
                AppMemoryUsage.Foreground = new SolidColorBrush(Colors.Green);
            }
            else if (usage > 104857600 && usage < 209715200)
            {
                AppMemoryUsage.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else if (usage > 209715200)
            {
                AppMemoryUsage.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (usage > 314572800)
            {
 
                if (GlobalLocalSettings.AutoCacheClear == "enabled")
                {
                    await WebView.ClearTemporaryWebDataAsync();
                    //GC.Collect(0, GCCollectionMode.Forced);
                }

            }
            AppMemoryUsage.Text = "Memory Usage: " + total;
        }

        private async void Favs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(favs.GetType());
                using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(
                  "favorites.json",
                  CreationCollisionOption.ReplaceExisting))
                {
                    serializer.WriteObject(stream, favs);
                }
            }
            catch (FileLoadException ex)
            {

            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            NavigatedPageType = e.SourcePageType;

            MainCommandBar.IsOpen = false;

            currentView.BackRequested -= CurrentView_BackRequested;

            //timer.Stop();

            try
            {
                printMan.PrintTaskRequested -= PrintTaskRequested;

                printDoc.Paginate -= Paginate;
                printDoc.GetPreviewPage -= GetPreviewPage;
                printDoc.AddPages -= AddPages;

                printMan = null;
                printDoc = null;
                printDocSource = null;
            }
            catch (Exception)
            {

            }
        }

        #region "Grids"

        private void MainGrid_Loading(FrameworkElement sender, object args)
        {
            AddTabButton.Label = "";
            CloseTabButton.Label = "";
            BackButton.Label = "";
            ForwardButton.Label = "";
            RefreshAppBarButton.Label = "";
            StopAppBarButton.Label = "";

            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            string AllTabsText = loader.GetString("AllTabsButton/Label");
            ToolTipService.SetToolTip(AllTabsButton, AllTabsText);

            string AddTabText = loader.GetString("AddTabButton/Label");
            ToolTipService.SetToolTip(AddTabButton, AddTabText + " (Ctrl + T)");

            string CloseTabText = loader.GetString("CloseTabButton/Label");
            ToolTipService.SetToolTip(CloseTabButton, CloseTabText + " (Ctrl + W)");

            string BackText = loader.GetString("BackButton/Label");
            ToolTipService.SetToolTip(BackButton, BackText + " (Alt + Left arrow)");

            string ForwardText = loader.GetString("ForwardButton/Label");
            ToolTipService.SetToolTip(ForwardButton, ForwardText + " (Alt + Right arrow)");

            string RefreshText = loader.GetString("RefreshButton/Label");
            ToolTipService.SetToolTip(RefreshAppBarButton, RefreshText + " (F5)");

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3)
                && PrintManager.IsSupported())
            {
                PrintButton.Visibility = Visibility.Visible;
            }
            else
            {
                PrintButton.Visibility = Visibility.Collapsed;
            }

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                BackButton.Visibility = Visibility.Collapsed;
            }

            AddressAutoSuggestBox.IsSuggestionListOpen = false;
        }

        private void ContentGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.F11)
            {
                if (appView.IsFullScreenMode)
                {
                    appView.ExitFullScreenMode();
                }
                else
                {
                    ShowFullScreen();
                }
            }
            else if (e.Key == VirtualKey.F5)
            {
                Refresh();
            }

            CoreVirtualKeyStates ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            if (ctrl.HasFlag(CoreVirtualKeyStates.Down) && e.Key == VirtualKey.Subtract)
            {
                ZoomOut();
            }

            if (ctrl.HasFlag(CoreVirtualKeyStates.Down) && e.Key == VirtualKey.Add)
            {
                ZoomIn();
            }
        }
        #endregion

        #region "Events"

        #region "Address bar"
        private async void SiteInfoFlyout_Opened(object sender, object e)
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView();
            var currentAgent = UserAgent.GetUserAgent();
            TrustTextBlock.Text = loader.GetString("UntrustedSiteState");
            ViewCertButton.Visibility = Visibility.Collapsed;

            if (PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri.StartsWith("https://"))
            {
                try
                {
                    Windows.Web.Http.HttpRequestMessage aReq = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, currentWebView.Source);
                    Windows.Web.Http.HttpResponseMessage aResp = await PivotMain.SelectedWebViewItem.ListViewItem.appHttpClient.SendRequestAsync(aReq);
                    // hit here if no exceptions!
                    if (aResp.IsSuccessStatusCode)
                    {
                        if (aReq.TransportInformation.ServerCertificate != null)
                        {
                            TrustTextBlock.Text = string.Format("{0} {1}\nUser Agent: {2}", loader.GetString("TrustedSiteState"), aReq.TransportInformation.ServerCertificate.Issuer, currentAgent);
                            ViewCertButton.Visibility = Visibility.Visible;
                        }
                    }
                }
                catch
                {
                    TrustTextBlock.Text = $"Issue finding certificate\n\n{currentAgent}";
                }
            }
            else if (PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML"))
            {
                TrustTextBlock.Text = loader.GetString("InternalSiteState");
            }
        }

        private void SiteInfoPresenter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UIElement se = sender as UIElement;
            FlyoutBase.ShowAttachedFlyout(se as FrameworkElement);
        }

        private async void ViewCertButton_Click(object sender, RoutedEventArgs e)
        {
            ViewCertButton.IsEnabled = false;

            ContentDialogResult? dialog = await PivotMain.SelectedWebViewItem.ListViewItem.ShowCertInfoDialog();
            if (dialog != null && (dialog == ContentDialogResult.None || dialog == ContentDialogResult.Primary))
            {
                ViewCertButton.IsEnabled = true;
            }
        }

        private void AddressAutoSuggestBox_TextBoxLoaded(object sender, RoutedEventArgs e)
        {
            // Add if for Desktop builds


            /* AddressAutoSuggestBox.TextBox.ContextMenuOpening += AddressAutoSuggestBox_ContextMenuOpening;
             if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
             {
                 AddressAutoSuggestBox.TextBox.ContextFlyout = EditMenu.ContextFlyout;
             }
             else
             {
                 AddressAutoSuggestBox.Holding += AddressAutoSuggestBox_Holding;
             } */
        }

        private void AddressAutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {


            if (AddressAutoSuggestBox.IsSuggestionListOpen)
            {
                AddressAutoSuggestBox.ItemsSource = CommonSites;
            }
        }

        private void AddressAutoSuggestBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = !ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7);


            UIElement senderUI = sender as UIElement;

        }

        private void AddressAutoSuggestBox_Holding(object sender, HoldingRoutedEventArgs e)
        {
            /* UIElement senderUI = sender as UIElement;
             if (e.HoldingState == HoldingState.Started)
             {
                 EditMenu.Core = AddressAutoSuggestBox;

                 EditMenu.ContextFlyout.ShowAt(AddressAutoSuggestBox, e.GetPosition(senderUI));
             } */
        }

        private void AddressAutoSuggestBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var search_term = AddressAutoSuggestBox.Text;

                List<string> commonResults = CommonSites.Where(i => i.StartsWith(search_term)).ToList();
                List<string> historyResults = historyListURLs.Where(i => i.StartsWith(search_term)).Distinct().ToList();

                commonResults.AddRange(historyResults);

                AddressAutoSuggestBox.ItemsSource = commonResults;
            }
        }

        private void AddressAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                Navigate(args.ChosenSuggestion.ToString());
                AddressAutoSuggestBox.IsSuggestionListOpen = false;
                AddressAutoSuggestBox.ItemsSource = null;
            }
            else if (args.QueryText.Length > 0)
            {
                Navigate(args.QueryText);
                AddressAutoSuggestBox.IsSuggestionListOpen = false;
                AddressAutoSuggestBox.ItemsSource = null;
            }
        }
        #endregion

        #region "Command bar"

        bool CommandBarPanelOpened = false;
        private void MainCommandBar_Opening(object sender, object e)
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
            {
                SecondaryCommands.OverlayInputPassThroughElement = PivotMain;
            }

            Button focusedButton = FocusManager.GetFocusedElement() as Button;
            if (CommandBarPanelOpened == false)
            {
                if (focusedButton != null && focusedButton.Name == "MoreButton")
                {
                    SecondaryCommands.ShowAt(focusedButton);
                }
                InputPane.GetForCurrentView().TryHide();
            }
        }

        private void MainCommandBar_Closing(object sender, object e)
        {
            if (CommandBarPanelOpened == true)
            {
                if (appView.IsFullScreenMode)
                {
                    currentWebView.Focus(FocusState.Programmatic);
                }

                SecondaryCommands.Hide();
            }
        }

        private void SecondaryCommands_Opened(object sender, object e)
        {
            CommandBarPanelOpened = true;
        }

        private void SecondaryCommands_Closed(object sender, object e)
        {
            CommandBarPanelOpened = false;
            if (!(FocusManager.GetFocusedElement() is Button focusedButton) ||
                (focusedButton != null && (focusedButton.Name != "MoreButton" ||
                focusedButton.Name == "MoreButton" && !focusedButton.IsPressed)))
            {
                MainCommandBar.IsOpen = false;
            }
        }

        #region "Command bar buttons"
        private void HomePageButton_Click(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine(homepage);
            if (GlobalLocalSettings.homePage != null)
            {
                if (!GlobalLocalSettings.homePage.Contains("https") && !GlobalLocalSettings.homePage.Contains("about:"))
                {

                    currentWebView.Navigate(new Uri("https://" + GlobalLocalSettings.homePage));
                }
                else
                {
                    currentWebView.Navigate(new Uri(GlobalLocalSettings.homePage));
                }
            }
            else
            {
                currentWebView.Navigate(new Uri("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/Home.html"));

            }
        }

        private void NewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            NewWindow();
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddTab();
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            CloseOneTab(PivotMain.SelectedWebViewItem);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            currentWebView.GoBack();
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            currentWebView.GoForward();
        }

        private void RefreshAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void StopAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            currentWebView.Stop();
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            InsertSelection.ShowAt(MainCommandBar);
        }

        private void FavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            Bookmarks.IsPaneOpen = true;
            BookmarksList.ItemsSource = favs;

            SecondaryCommands.Hide();
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SavePageAs(PivotMain.SelectedWebViewItem);
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            ShareButton.IsEnabled = false;

            ShareURL();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            Print();
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void ToolsFlyoutButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void ShowJSConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            JSConsolePane.ShowAt(PivotMain);
            SecondaryCommands.Hide();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFullScreen();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }

        #endregion

        #endregion

        #region "Items context flyout"

        private void HeaderTextBlock_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void AddTabItem_Click(object sender, RoutedEventArgs e)
        {
            AddTab();
        }

        private void CloseTabItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            foreach (WebViewPivotItem item in PivotMain.Items)
            {
                if (item.Header.ToString() == button.DataContext.ToString())
                {
                    CloseOneTab(item);
                    break;
                }
            }
        }

        private void CloseAllTabsItem_Click(object sender, RoutedEventArgs e)
        {
            ClearTabs();
        }

        #endregion

        #region "All Tabs flyout"

        private void AllTabsButton_Click(object sender, RoutedEventArgs e)
        {
            AllTabsFlyout.ShowAt(AllTabsButton);
        }

        private void AllTabsFlyout_Opening(object sender, object e)
        {
            FindName(nameof(AllTabsList));

            AllTabsList.ItemsSource = dataList;
            AllTabsList.SelectedIndex = PivotMain.SelectedIndex;
        }

        private void AllTabsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AllTabsList.SelectedIndex != -1)
            {
                Item item = e.AddedItems[0] as Item;

                PivotMain.SelectedItem = item.PivotItem;

                AllTabsList.SelectedIndex = PivotMain.SelectedIndex;
            }
        }

        private void TextBlock_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            TextBlock senderText = sender as TextBlock;
            senderText.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void TextBlock_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            TextBlock senderText = sender as TextBlock;
            senderText.Foreground = Resources["SystemControlHighlightAltBaseHighBrush"] as SolidColorBrush;
        }

        Item senderItem;
        private async void ListViewItemGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Completed)
            {
                await Task.Delay(100);
                FlyoutBase.GetAttachedFlyout(sender as FrameworkElement).ShowAt(AllTabsButton);
                senderItem = (sender as Grid).DataContext as Item;
                ApiResources.Vibrate(50);
            }
        }

        private void ListViewItemGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
            e.Handled = true;
        }

        private void CloseTabAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton button = sender as AppBarButton;
            Item item = button.DataContext as Item;
            CloseOneTab(item.PivotItem);
        }

        private void RefreshSiteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            if (!(button.DataContext is Item item))
            {
                item = senderItem;
            }

            item.PivotItem.WebViewCore.WebView.Refresh();
        }

        private void CloseTabMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            if (!(button.DataContext is Item item))
            {
                item = senderItem;
            }

            CloseOneTab(item.PivotItem);
        }

        #endregion

        #region "Favorites bar"

        private void BookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            AddOrRemoveBookmark();
        }

        private void RemoveBookmarkAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton button = sender as AppBarButton;
            Bookmark bm = button.DataContext as Bookmark;
            favs.Remove(bm);
            BookmarksList.ItemsSource = favs;

            if (bm.SiteURL == currentWebView.Source.AbsoluteUri)
            {
                BookamrkState.Glyph = "\uE734;";
                BookmarkHyperlinkButton.Content = ResourceLoader.GetForCurrentView().GetString("BookmarkHyperlinkText");
            }
        }

        private void BookmarksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BookmarksList.SelectedIndex != -1)
            {
                Bookmark bm = e.AddedItems[0] as Bookmark;
                Navigate(bm.SiteURL);
                BookmarksList.SelectedIndex = -1;
            }
        }

        #endregion

        private async void HomeItem_Click(object sender, RoutedEventArgs e)
        {
            await currentWebView.InvokeScriptAsync("eval", new string[] { @" window.scrollTo(0,0); " });
        }

        private async void EndItem_Click(object sender, RoutedEventArgs e)
        {
            await currentWebView.InvokeScriptAsync("eval", new string[] { @" window.scrollTo(0,document.documentElement.scrollHeight); " });
        }

        private void PinSiteStartMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PinSiteToStartMenu();
        }

        private async void PivotMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (appView.IsFullScreenMode)
            {
                appView.ExitFullScreenMode();
            }

            if (PivotMain.SelectedWebViewItem != null)
            {
                currentWebView = PivotMain.SelectedWebView;



                //the following code needs to 'install' the WebView properly, otherwise it won't be shown
                if (!PivotMain.SelectedWebViewItem.WebViewCore.IsWebViewLoaded)
                {
                    PivotMain.UpdateLayout();
                    MainGrid.Margin = new Thickness(1, -100, 1, 1);
                    await Task.Delay(120);
                    MainGrid.Margin = new Thickness(0, 0, 0, 0);
                }

                string SiteTitle;
                if (string.IsNullOrEmpty(currentWebView.DocumentTitle))
                {
                    if (string.IsNullOrEmpty(currentWebView.Source.DnsSafeHost))
                    {
                        SiteTitle = currentWebView.Source.AbsoluteUri;
                    }
                    else
                    {
                        SiteTitle = currentWebView.Source.DnsSafeHost;
                    }
                }
                else
                {
                    SiteTitle = currentWebView.DocumentTitle;
                }
                appView.Title = TitleBarApi.UserFriendlyTitle(SiteTitle);

                // TitleTextBlock.Text = string.Format(appView.Title);

                AlignTopBar();

                if (PivotMain.Items.Count <= 1)
                {
                    CloseTabButton.IsEnabled = false;
                    CloseTabButton2.IsEnabled = false;
                }
                else
                {
                    CloseTabButton.IsEnabled = true;
                    CloseTabButton2.IsEnabled = true;
                }

                if (currentWebView.Source.AbsoluteUri.StartsWith("http://") || currentWebView.Source.AbsoluteUri.StartsWith("https://"))
                {
                    if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                    {
                        BookmarkButton.Visibility = Visibility.Visible;
                    }
                    BookmarkHyperlinkButton.Visibility = Visibility.Visible;
                    // BookmarkImportButton.Visibility = Visibility.Visible;
                    ResourceLoader loader = ResourceLoader.GetForCurrentView();
                    if (IsBookmarkExist)
                    {
                        BookamrkState.Glyph = "\uE735;";
                        BookmarkHyperlinkButton.Content = loader.GetString("UnBookmarkHyperlinkText");
                    }
                    else
                    {
                        BookamrkState.Glyph = "\uE734;";
                        BookmarkHyperlinkButton.Content = loader.GetString("BookmarkHyperlinkText");
                    }
                }
                else
                {
                    BookmarkButton.Visibility = Visibility.Collapsed;
                    BookmarkHyperlinkButton.Visibility = Visibility.Collapsed;

                }

                if (currentWebView.Source.AbsoluteUri == "ms-appx-web:///PagesHTML/Home.html"
                    || currentWebView.Source.AbsoluteUri == "ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/Home.html")
                {
                    AddressAutoSuggestBox.Text = "";
                }
                else if ((currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NoInternet.html#")
                    || currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NotFound.html#"))
                    && !string.IsNullOrEmpty(currentWebView.Source.AbsoluteUri.Split('#')[1]))
                {
                    AddressAutoSuggestBox.Text = currentWebView.Source.AbsoluteUri.Split('#')[1];
                }
                else
                {
                    AddressAutoSuggestBox.Text = PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri;
                }



                CheckNavHistory();

                ZoomOutButton.IsEnabled = true;
                ZoomInButton.IsEnabled = true;

                switch (PivotMain.SelectedWebViewItem.WebViewCore.PageZoom)
                {
                    case "100%":
                        ZoomPercentTextBlock.Text = "100%";
                        break;
                    case "10%":
                        ZoomPercentTextBlock.Text = "10%";
                        ZoomOutButton.IsEnabled = false;
                        break;
                    case "25%":
                        ZoomPercentTextBlock.Text = "25%";
                        break;
                    case "50%":
                        ZoomPercentTextBlock.Text = "50%";
                        break;
                    case "200%":
                        ZoomPercentTextBlock.Text = "200%";
                        break;
                    case "300%":
                        ZoomPercentTextBlock.Text = "300%";
                        break;
                    case "400%":
                        ZoomPercentTextBlock.Text = "400%";
                        break;
                    case "500%":
                        ZoomPercentTextBlock.Text = "500%";
                        ZoomInButton.IsEnabled = false;
                        break;
                }

                AddressAutoSuggestBox.ItemsSource = null;
                AddressAutoSuggestBox.IsSuggestionListOpen = false;

                currentWebView.Focus(FocusState.Programmatic);
            }
        }

        #region "currentWebView"



        bool isAggro;
        private async void currentWebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            // test on clearing webvie data
           /* if (isAggro == true && !args.Uri.AbsoluteUri.Contains("71330982-ba82-4d35-b5cb-3488eefb31ed"))
            {
                if (GlobalLocalSettings.AggressiveCacheClean == "enabled")
                {
                    
                    int count = 0;
                    var timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                    timer.Start();
                    timer.Tick += (s, p) =>
                    {
                        currentWebView.Source = new Uri("about:blank");
                        count++;
                        if (count == 20)
                        {
                            timer.Stop();
                            isAggro = false;
                        }
                    };
                }
            } */
        



            // required overrides to pass JS console messages to C#
            currentWebView.AddWebAllowedObject("console", new ConsoleOverride());
            currentWebView.AddWebAllowedObject("xevents", new WindowOverride());


            string SiteHostName = PivotMain.SelectedWebViewItem.WebViewCore.URL.DnsSafeHost;

            appView.Title = TitleBarApi.UserFriendlyTitle(SiteHostName);
            //Debug.WriteLine(args.Uri.Host);
            //TitleTextBlock.Text = string.Format(appView.Title);

            PivotMain.SelectedWebViewItem.Header = SiteHostName;
            PivotMain.SelectedWebViewItem.ListViewItem.Title = PivotMain.SelectedWebViewItem.Header.ToString();

            SiteInfoPresenter.Content = new ProgressRing() { IsActive = true, Width = 32, Height = 32 };

            if (PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri == "ms-appx-web:///PagesHTML/Home.html"
                || PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri == "ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/Home.html")
            {
                AddressAutoSuggestBox.Text = ""; //shows placeholder of the address bar
            }
            else if ((currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NoInternet.html#")
                || currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NotFound.html#"))
                && !string.IsNullOrEmpty(currentWebView.Source.AbsoluteUri.Split('#')[1]))
            {
                AddressAutoSuggestBox.Text = currentWebView.Source.AbsoluteUri.Split('#')[1]; //shows the URL of the site that attempted to navigate
            }
            else
            {
                AddressAutoSuggestBox.Text = PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri; //shows the real URL
            }

            StopAppBarButton.Visibility = Visibility.Visible;
            RefreshAppBarButton.Visibility = Visibility.Collapsed;

            // WebView native object must be inserted in the OnNavigationStarting event handler
            // Expose the native WinRT object on the page's global object
            //currentWebView.AddWebAllowedObject("HandlerUI", webieHandlerUI);

            //timer.Stop();

           /* if (args.Uri.AbsoluteUri.Contains("youtube.com"))
            {
                StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFolder jsClasses = await appFolder.GetFolderAsync("ClassesJS\\h264ify");
                var file = await jsClasses.GetFileAsync("inject.js");
                string script = File.ReadAllText(file.Path);
                await currentWebView.InvokeScriptAsync("eval", new string[] { script });
            } */
           // isAggro = true;
            currentWebView.Focus(FocusState.Programmatic);

        }

        private void currentWebView_ContentLoading(FrameworkElement sender, WebViewContentLoadingEventArgs args)
        {
            if (PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri == "ms-appx-web:///PagesHTML/Home.html"
                || PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri == "ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/Home.html")
            {
                AddressAutoSuggestBox.Text = "";
            }
            else if ((currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NoInternet.html#")
                || currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NotFound.html#"))
                && !string.IsNullOrEmpty(currentWebView.Source.AbsoluteUri.Split('#')[1]))
            {
                AddressAutoSuggestBox.Text = currentWebView.Source.AbsoluteUri.Split('#')[1];
            }
            else
            {
                AddressAutoSuggestBox.Text = PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri;
            }

            currentWebView.Focus(FocusState.Programmatic);
        }

        private async void currentWebView_FrameNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri == "ms-appx-web:///PagesHTML/Home.html"
                || PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri == "ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/Home.html")
            {
                AddressAutoSuggestBox.Text = "";
            }
            else if ((currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NoInternet.html#")
                || currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NotFound.html#"))
                && !string.IsNullOrEmpty(currentWebView.Source.AbsoluteUri.Split('#')[1]))
            {
                AddressAutoSuggestBox.Text = currentWebView.Source.AbsoluteUri.Split('#')[1];
            }
            else
            {
                AddressAutoSuggestBox.Text = PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri;
            }

            CheckNavHistory();

            if (FocusManager.GetFocusedElement() != AddressAutoSuggestBox)
            {
                currentWebView.Focus(FocusState.Programmatic);
            }

            var url = args.Uri;
            /* if (url.AbsoluteUri.Contains("/PagesHTML/Home.html"))
             {
                 string fname = @"Assets\default.jpg";
                 StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                 StorageFile file = await InstallationFolder.GetFileAsync(fname);


                 string WallpaperScript = "var imagepaper = document.body.style.backgroundImage = \"url('" + file.Path + ")\"; }";
                 await currentWebView.InvokeScriptAsync("eval", new string[] { WallpaperScript });
             } */
            //GC.Collect();

        }

        private async void currentWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {



            string SiteTitle = currentWebView.DocumentTitle;
            if (string.IsNullOrEmpty(SiteTitle))
            {
                if (string.IsNullOrEmpty(currentWebView.Source.DnsSafeHost))
                {
                    SiteTitle = currentWebView.Source.AbsoluteUri;
                }
                else
                {
                    SiteTitle = currentWebView.Source.DnsSafeHost;
                }
            }

            PivotMain.SelectedWebViewItem.Header = SiteTitle;
            PivotMain.SelectedWebViewItem.ListViewItem.Title = PivotMain.SelectedWebViewItem.Header.ToString();

            appView.Title = TitleBarApi.UserFriendlyTitle(SiteTitle);


            AlignTopBar();

            if (PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri == "ms-appx-web:///PagesHTML/Home.html"
                || PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri == "ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/Home.html")
            {
                AddressAutoSuggestBox.Text = "";
            }
            else if ((currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NoInternet.html#")
                || currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NotFound.html#"))
                && !string.IsNullOrEmpty(currentWebView.Source.AbsoluteUri.Split('#')[1]))
            {
                AddressAutoSuggestBox.Text = currentWebView.Source.AbsoluteUri.Split('#')[1];
            }
            else
            {
                AddressAutoSuggestBox.Text = PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri;
            }

            if (PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri.StartsWith("http://")
                || PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri.StartsWith("https://"))
            {
                if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    BookmarkButton.Visibility = Visibility.Visible;
                }
                BookmarkHyperlinkButton.Visibility = Visibility.Visible;

                ResourceLoader loader = ResourceLoader.GetForCurrentView();
                if (IsBookmarkExist)
                {
                    BookamrkState.Glyph = "\uE735;";
                    BookmarkHyperlinkButton.Content = loader.GetString("UnBookmarkHyperlinkText");
                }
                else
                {
                    BookamrkState.Glyph = "\uE734;";
                    BookmarkHyperlinkButton.Content = loader.GetString("BookmarkHyperlinkText");
                }
            }
            else
            {
                BookmarkButton.Visibility = Visibility.Collapsed;
                BookmarkHyperlinkButton.Visibility = Visibility.Collapsed;

            }

            CheckNavHistory();

            RefreshAppBarButton.Visibility = Visibility.Visible;
            StopAppBarButton.Visibility = Visibility.Collapsed;

            // timer.Start();

            if (FocusManager.GetFocusedElement() != AddressAutoSuggestBox)
            {
                currentWebView.Focus(FocusState.Programmatic);
            }
            if (args.Uri.AbsoluteUri.Contains("71330982-ba82-4d35-b5cb-3488eefb31ed") == false && args.Uri.AbsoluteUri.Contains("about:blank") == false )
            {
                var newHistoryItem = new Bookmark
                {
                    Title = currentWebView.DocumentTitle,
                    SiteURL = currentWebView.Source.AbsoluteUri
                };
                Debug.WriteLine(args.Uri.AbsoluteUri);

                UpdateHistoryView(newHistoryItem);
                UpdatePageSettingsList(args.Uri.Host);
            }
           // GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);



            // Scripts to handle right click events and content detection

            var imagesScript = @"function MountContextMenuToImages(){ var imgs = document.images;
                                    for (var i = 0; i < imgs.length; i++) {
                                        if (!imgs[i].hasAttribute('xevent'))
                                        {
                                            imgs[i].setAttribute('xevent', 'ready');
                                            imgs[i].oncontextmenu = function(e) 
                                                {
                                                    var elementHTML = e.target.outerHTML;
                                                    console.notify('longclick', 'image|#|' + (this.hasAttribute('src') ? this.src : '') + '|#|' + (this.hasAttribute('alt') ? this.alt : '') + '|#|' + e.clientX + '|#|' + e.clientY + '|#|' + elementHTML);
                                                    return false;
                                                }
                                         }
                                        }
                                       }
                                    MountContextMenuToImages();";

            var linksScript = @"function MountContextMenuToLinks(){ var links = document.links;
                                    for (var i = 0; i < links.length; i++) {
                                    var innerImages = links[i].getElementsByTagName('img');
                                    if (!links[i].hasAttribute('xevent') && (innerImages.length == 0 || window.location.hostname.includes('youtube.com')))
                                    {
                                        links[i].setAttribute('xevent', 'ready');
                                        links[i].oncontextmenu = function(e) {
                                             var elementHTML = e.target.outerHTML;
                                             console.notify('longclick', 'link|#|' + (this.hasAttribute('href') ? this.href : '') + '|#|' + this.innerHTML + '|#|' + e.clientX + '|#|' + e.clientY + '|#|' + elementHTML);
                                             return false;
                                    }
                                  }
                                }
                              }
                            MountContextMenuToLinks();";

            var videosScript = @"function MountContextMenuToVideos(){var videos = document.getElementsByTagName('video');
for (var i = 0; i < videos.length; i++) {
    if (!videos[i].hasAttribute('xevent'))
    {
        videos[i].setAttribute('xevent', 'ready');
        videos[i].oncontextmenu = function(e) {
            var sources = this.getElementsByTagName('source');
            var sourceLink = sources.length > 0 ? sources[0].src : (this.hasAttribute('src') ? this.src : '');
            var elementHTML = e.target.outerHTML;
            console.notify('longclick', 'video|#|' + sourceLink + '|#|' + '' + '|#|' + e.clientX + '|#|' + e.clientY + '|#|' + elementHTML);
            return false;
        }
    }
}
}
MountContextMenuToVideos();
";

            var documentScript = @"function getSelectionText() { var text = ''; if (window.getSelection) { text = window.getSelection().toString(); } else if (document.selection && document.selection.type != 'Control') {
                                   text = document.selection.createRange().text; }return text; }
                                        document.oncontextmenu = function(e) {MountContextMenuToVideos(); MountContextMenuToLinks(); MountContextMenuToImages(); var targetText = getSelectionText(); var elementHTML=e.target.outerHTML; 
                                        if(targetText.length > 0){ console.notify('longclick', 'text|#|' + '' + '|#|' + targetText + '|#|' + e.clientX + '|#|' + e.clientY + '|#|' + elementHTML); }
                                        else{ if(e.target.nodeName != 'IMG' && e.target.nodeName != 'VIDEO' && e.target.nodeName != 'A'){ 
                                        console.notify('longclick', 'element|#|' + '' + '|#|' + targetText + '|#|' + e.clientX + '|#|' + e.clientY + '|#|' + elementHTML); } } return false;}
                                       ";



            var DocumentLoadedHandler = "var xeventsPageLoaderMonitor = setInterval(function () { if (document.readyState === 'complete'){ clearInterval(xeventsPageLoaderMonitor); xevents.loaded(); } },1000);";

            var scrollHandler = "var W10MScrollPreState = false, scrollState = false; var ScrollLoaderMonitor = setInterval(function () { if(W10MScrollPreState!=scrollState){ W10MScrollPreState = scrollState; xevents.address(scrollState); } },500); window.onscroll = function(e){ MountContextMenuToVideos(); MountContextMenuToLinks(); MountContextMenuToImages();  scrollState = this.oldScroll > this.scrollY; this.oldScroll = this.scrollY; }";


            await currentWebView.InvokeScriptAsync("eval", new string[] { linksScript });
            await currentWebView.InvokeScriptAsync("eval", new string[] { imagesScript });
            await currentWebView.InvokeScriptAsync("eval", new string[] { videosScript });
           // await currentWebView.InvokeScriptAsync("eval", new string[] { DocumentLoadedHandler });
            var docResult = await currentWebView.InvokeScriptAsync("eval", new string[] { documentScript });
           // await currentWebView.InvokeScriptAsync("eval", new string[] { scrollHandler });

           // Debug.WriteLine($"documentScript: {docResult}");


            //Debug.WriteLine("Script Result: " + testResult);
        }





        private void currentWebView_ContainsFullScreenElementChanged(WebView sender, object args)
        {
            AsyncEngine.Execute(MainPagePG.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () =>
                {
                    ApplicationView applicationView = ApplicationView.GetForCurrentView();

                    if (!applicationView.IsFullScreenMode && sender.ContainsFullScreenElement)
                    {
                        ShowFullScreen();
                    }
                    else if (!sender.ContainsFullScreenElement)
                    {
                        ExitFullScreen(applicationView);

                    }



                }));
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }

        private async void currentWebView_PermissionRequested(WebView sender, WebViewPermissionRequestedEventArgs args)
        {
            string currentPermission = "";

            UCNotification notification = new UCNotification("", "")
            {
                ToastLaunchArguments = "action=selecttab&tab=" + PivotMain.SelectedWebViewItem.ListViewItem.FullTitle.Replace(" ", "_") //to open selected tab after click
            };

            ContentDialog permissionDialog = new ContentDialog()
            {
                Title = "Permissions",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            };
            ContentDialogResult permissionDialogResult = ContentDialogResult.None;

            switch (args.PermissionRequest.PermissionType)
            {
                case WebViewPermissionType.UnlimitedIndexedDBQuota:
                    if (CommonSites.Contains(currentWebView.Source.DnsSafeHost))
                    {
                        args.PermissionRequest.Allow();
                    }
                    else
                    {
                        args.PermissionRequest.Deny();
                    }
                    break;
                case WebViewPermissionType.Geolocation:
                    GeolocationAccessStatus accessStatus = await Geolocator.RequestAccessAsync();
                    bool[] NotAllowed = new bool[] { accessStatus == GeolocationAccessStatus.Denied, accessStatus == GeolocationAccessStatus.Unspecified };



                    if (accessStatus == GeolocationAccessStatus.Allowed)
                    {
                        permissionDialog.Content = string.Format("\"{0}\" wants to access your location. Do you want to allow?",
                            args.PermissionRequest.Uri.Host);
                        currentPermission = GlobalLocalSettings.LocationPermission;
                    }
                    else
                    {
                        if (NotAllowed[0])
                        {
                            notification.Title = "Location is disabled";
                            notification.Content = "Geolocation is disabled on Windows privacy settings. Do you want to enable?";
                            notification.ToastButtonContent = "Open settings";
                            notification.ToastButtonArguments = "action=opensettings&section=privacy-location";
                        }
                        else
                        {
                            notification.Title = "Error";
                            notification.Content = "Unspecified error. Make sure that geolocation is enabled on Windows privacy settings.";
                        }

                        notification.ShowNotification();
                    }
                    break;
                case WebViewPermissionType.Media:
                    permissionDialog.Content = string.Format("\"{0}\" wants to access your camera or microphone. Do you want to allow?",
                        args.PermissionRequest.Uri.Host);
                    currentPermission = GlobalLocalSettings.MediaPermission;
                    break;
            }

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3) && args.PermissionRequest.PermissionType == WebViewPermissionType.WebNotifications)
            {
                permissionDialog.Content = string.Format("\"{0}\" wants to send notification. Do you want to allow?",
                            args.PermissionRequest.Uri.Host);
                currentPermission = GlobalLocalSettings.WebNotificationPermission;
            }

            if (currentPermission == "1")
            {
                permissionDialogResult = await permissionDialog.ShowAsync();
            }

            if (permissionDialogResult == ContentDialogResult.Primary || currentPermission == "2")
            {
                args.PermissionRequest.Allow();
            }
            else if (permissionDialogResult == ContentDialogResult.Secondary || currentPermission == "3")
            {
                args.PermissionRequest.Deny();
            }
            else
            {
                args.PermissionRequest.Defer();
            }
        }

        private void currentWebView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            Navigate(args.Uri.AbsoluteUri, true);

            args.Handled = true;
        }

        private void currentWebView_LongRunningScriptDetected(WebView sender, WebViewLongRunningScriptDetectedEventArgs args)
        {
           /* if (args.ExecutionTime.TotalSeconds > 50)
            {
                var unresponsiveScript = args.ExecutionTime.TotalSeconds;
                Debug.WriteLine("Script detected that is running for a long time: " + unresponsiveScript);
                args.StopPageScriptExecution = true;
            } */
            
        }

        private void currentWebView_UnviewableContentIdentified(WebView sender, WebViewUnviewableContentIdentifiedEventArgs args)
        {
            if (!isDownloading)
            {
                ShowContentDialog(args.Uri);
            }
            else
            {
                var CustErr = new MessageDialog($"Please wait for the current download to finish");
                CustErr.Commands.Add(new UICommand("Close"));
                CustErr.ShowAsync();
            }
        }





        private async void WebieHandlerUI_ReceivedData(string data)
        {
            AsyncEngine.Execute(Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                string[] messageArray = data.Split(':');
                string message, type;

                //this splits the message
                if (messageArray.Length > 1)
                {
                    message = messageArray[1];
                    type = messageArray[0];
                }
                else
                {
                    message = data;
                    type = "";
                }

                //commands are here
                if (type.Equals("typeAlert"))
                {
                    ApiResources.ShowPopUpDialogAsync("Alert", message);
                }
                else if (type.Equals("eventType") && message == "ZP")
                {
                    ZoomIn();
                }
                else if (type.Equals("eventType") && message == "ZM")
                {
                    ZoomOut();
                }
                else if (type.Equals("eventType") && message == "TZ")
                {
                    IAsyncOperation<string> asyncOperation = null;
                    switch (PivotMain.SelectedWebViewItem.WebViewCore.PageZoom)
                    {
                        case "100%":
                            asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { "document.body.style.zoom = '200%';" });
                            PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                            ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                            break;
                        case "10%":
                            asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '25%';" });
                            PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                            ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                            ZoomOutButton.IsEnabled = true;
                            break;
                        case "25%":
                            asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { "document.body.style.zoom = '50%';" });
                            PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                            ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                            break;
                        case "50%":
                            asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '100%';" });
                            PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                            ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                            break;
                        case "200%":
                            asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '100%';" });
                            PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                            ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                            break;
                        case "300%":
                            asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '200%';" });
                            PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                            ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                            break;
                        case "400%":
                            asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '300%';" });
                            PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                            ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                            break;
                        case "500%":
                            asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '400%';" });
                            PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                            ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                            ZoomInButton.IsEnabled = true;
                            break;
                    }

                    asyncOperation.Close();
                }

                switch (data)
                {
                    case "RefreshPage":
                        Refresh();
                        break;
                    case "ShowFullScreen":
                        if (appView.IsFullScreenMode)
                        {
                            appView.ExitFullScreenMode();
                        }
                        else
                        {
                            ShowFullScreen();
                        }
                        break;
                    case "FocusOnAddressBar":
                        AddressAutoSuggestBox.TextSelectAll();
                        break;
                    case "SavePageAs":
                        SavePageAs(PivotMain.SelectedWebViewItem);
                        break;
                    case "SelectAll":
                        await currentWebView.InvokeScriptAsync("eval", new string[] { @"
                            window.getSelection().removeAllRanges();
                            document.execCommand('selectAll', false, null);
                        " });
                        break;
                }

                GC.Collect();
            }));

            await Task.Run(() =>
            {
                AsyncEngine.Execute(Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //hides or show address bar
                    AlignTopBar();

                    GC.Collect();
                }));
            });
        }

        private async void WebieHandlerUI_ContextMenuOpening(int x, int y)
        {
            await Task.Run(() =>
            {
                AsyncEngine.Execute(Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                {


                    WebViewSelection wvSel = new WebViewSelection();

                    //others except Select all button doesn't work properly on first build of mobile
                    if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
                    {
                        wvSel.isFocusedElementEditiable = await currentWebView.IsFocusedElementEditiable();
                        wvSel.SelectionText = await currentWebView.SelectionText();
                        wvSel.ActiveElementLink = await currentWebView.ActiveElementLink();
                    }

                    currentWebView.Tag = wvSel;


                }));
            });
        }
        #endregion

        #endregion

        #region "Print events"
        private void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            // Create the PrintTask.
            // Defines the title and delegate for PrintTaskSourceRequested
            PrintTask printTask = args.Request.CreatePrintTask("Print", PrintTaskSourceRequrested);

            // Handle PrintTask.Completed to catch failed print jobs
            printTask.Completed += PrintTaskCompleted;
        }

        private void PrintTaskSourceRequrested(PrintTaskSourceRequestedArgs args)
        {
            // Set the document source.
            args.SetSource(printDocSource);
        }

        private void Paginate(object sender, PaginateEventArgs e)
        {
            // As I only want to print one Rectangle, so I set the count to 1
            printDoc.SetPreviewPageCount(1, PreviewPageCountType.Final);
        }

        private void GetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            // Provide a UIElement as the print preview.
            printDoc.SetPreviewPage(e.PageNumber, currentWebView);
        }

        private void AddPages(object sender, AddPagesEventArgs e)
        {
            printDoc.AddPage(currentWebView);

            // Indicate that all of the print pages have been provided
            printDoc.AddPagesComplete();
        }

        private async void PrintTaskCompleted(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
            // Notify the user when the print operation fails.
            if (args.Completion == PrintTaskCompletion.Failed)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    ContentDialog noPrintingDialog = new ContentDialog()
                    {
                        Title = "Printing error",
                        Content = "\nSorry, failed to print.",
                        PrimaryButtonText = "OK"
                    };
                    await noPrintingDialog.ShowAsync();
                });
            }
        }
        #endregion

        #region "Methods"

        private void AddTab()
        {
            WebViewPivotItem newTab = PivotMain.AddTab();
            newTab.Tag = newTab;

            currentWebView = newTab.WebViewCore.WebView;

            object Header = newTab.Header;
            Header = "New tab " + PivotMain.Items.Count;
            appView.Title = Header.ToString();

            // TitleTextBlock.Text = string.Format("{0} – {1}", appView.Title, Package.Current.DisplayName);

            //Adds items in all tabs list
            newTab.ListViewItem.Title = Header.ToString();
            dataList.Add(newTab.ListViewItem);

            currentWebView.NavigationStarting += currentWebView_NavigationStarting;
            currentWebView.ContentLoading += currentWebView_ContentLoading;
            currentWebView.FrameNavigationCompleted += currentWebView_FrameNavigationCompleted;
            currentWebView.NavigationCompleted += currentWebView_NavigationCompleted;
            currentWebView.NewWindowRequested +=
                new TypedEventHandler<WebView, WebViewNewWindowRequestedEventArgs>(currentWebView_NewWindowRequested);
            currentWebView.ContainsFullScreenElementChanged += currentWebView_ContainsFullScreenElementChanged;
            currentWebView.PermissionRequested += currentWebView_PermissionRequested;
            currentWebView.LongRunningScriptDetected += currentWebView_LongRunningScriptDetected;
            currentWebView.UnviewableContentIdentified += currentWebView_UnviewableContentIdentified;
            currentWebView.ScriptNotify += CurrentWebView_ScriptNotify;
            currentWebView.ContextRequested += CurrentWebView_ContextRequested;
            currentWebView.DOMContentLoaded += CurrentWebView_DOMContentLoaded;
            currentWebView.FrameDOMContentLoaded += CurrentWebView_FrameDOMContentLoaded;
            // webieHandlerUI.ReceivedData += WebieHandlerUI_ReceivedData;
            //webieHandlerUI.ContextMenuOpening += WebieHandlerUI_ContextMenuOpening;
            //webieHandlerUI.SelectionMenuOpening += WebieHandlerUI_SelectionMenuOpening;
        }

        private void CurrentWebView_FrameDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            Debug.WriteLine("Frame HTML finished processing");
        }

        private void CurrentWebView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            Debug.WriteLine("Page HTML finished processing");
        }

        private void CurrentWebView_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            args.Handled = true;
        }

        private void CurrentWebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            Debug.WriteLine("Script Notify from currentWebView: " + e.Value);
        }

        private async void NewWindow()
        {
            CoreApplicationView newAV = CoreApplication.CreateNewView();
            await newAV.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    Window newWindow = Window.Current;
                    ApplicationView newAppView = ApplicationView.GetForCurrentView();

                    Frame frame = new Frame();
                    frame.Navigate(typeof(MainPage), "NewWindow");
                    newWindow.Content = frame;
                    newWindow.Activate();

                    //Shows new window of same app
                    await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                        newAppView.Id,
                        ViewSizePreference.UseMinimum,
                        appView.Id,
                        ViewSizePreference.UseMinimum);
                });
        }

        private async void CloseOneTab(WebViewPivotItem item)
        {
            await Task.Delay(200);
            if (PivotMain.Items.Count > 1)
            {
                CloseTab(item);
            }
        }

        private void CloseTab(WebViewPivotItem item)
        {
            item.Tag = null;
            PivotMain.CloseTab(item);
            dataList.Remove(item.ListViewItem);

            if (AllTabsList != null)
            {
                AllTabsList.SelectedIndex = PivotMain.SelectedIndex;
            }
        }

        private async void ClearTabs()
        {
            //Code will not working normally when add only one tab
            await Task.Delay(100);
            AddTab();
            PivotMain.Items.Clear();
            dataList.Clear();
            AddTab();
            await Task.Delay(100);
        }

        private void Navigate(string address, bool newTab = false)
        {
            address = address.Trim().Replace('\\', '/');

            Regex protocolRegex = new Regex(@"^((http(s)?:\/\/|(ms-appx-web|ms-appdata):?(\/?)\/\/)[\w#!?+=&%\-\\\d]+(\.[\w\-\d]+)*?(:\d+)?(\/[\w#!?+=:&();%\s\-\\\d\.]*?)*|(about):.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex localhostRegex = new Regex(@"^localhost(:[0-9]+)*[\w#!:.?+=&%!\-\/]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex domainRegex = new Regex(@"^\w*\.\w+?(:[0-9]+)*[\/+\S+]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!protocolRegex.IsMatch(address))
            {
                if (localhostRegex.IsMatch(address) || domainRegex.IsMatch(address))
                {
                    try
                    {
                        UriBuilder urlBuilder = new UriBuilder(address);
                        address = urlBuilder.Uri.AbsoluteUri;
                    }
                    catch
                    {

                        if (GlobalLocalSettings.SearchEngine == "Bing")
                        {
                            address = "https://www.bing.com/search?q=" + address;
                        }
                        else if (GlobalLocalSettings.SearchEngine == "Google")
                        {
                            address = "https://www.google.am/search?q=" + address;
                        }
                        else if (GlobalLocalSettings.SearchEngine == "Yahoo")
                        {
                            address = "https://search.yahoo.com/search?p=" + address;
                        }
                        else if (GlobalLocalSettings.SearchEngine == "Yandex")
                        {
                            address = "https://yandex.com/search/?text=" + address;
                        }
                        else if (GlobalLocalSettings.SearchEngine == "Qwant")
                        {
                            address = "https://lite.qwant.com/?q=" + address;
                        }
                    }
                }
                else
                {
                    Regex schemeRegex = new Regex(@"^(javascript|data|chrome|mailto|tel|sms|callto|mms|onitor|textie|ms-settings|windowsdefender|magnet):?(\S)+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if (schemeRegex.IsMatch(address))
                    {
                        try
                        {
                            WebResources.Navigate(address);
                        }
                        catch (Exception) { }
                        return;
                    }
                    else
                    {
                        if (GlobalLocalSettings.SearchEngine == "Bing")
                        {
                            address = "https://www.bing.com/search?q=" + address;
                        }
                        else if (GlobalLocalSettings.SearchEngine == "Google")
                        {
                            address = "https://www.google.am/search?q=" + address;
                        }
                        else if (GlobalLocalSettings.SearchEngine == "Yahoo")
                        {
                            address = "https://search.yahoo.com/search?p=" + address;
                        }
                        else if (GlobalLocalSettings.SearchEngine == "Yandex")
                        {
                            address = "https://yandex.com/search/?text=" + address;
                        }
                        else if (GlobalLocalSettings.SearchEngine == "Qwant")
                        {
                            address = "https://lite.qwant.com/?q=" + address;
                        }
                    }
                }
            }

            WebViewPivotItem foundTab = GetSiteInTabs(new Uri(address));
            if (foundTab != null)
            {
                PivotMain.SelectedItem = foundTab;
                return;
            }
            else
            {
                if (newTab)
                {
                    if (PivotMain.GetFreeItem() == null)
                    {
                        AddTab();
                    }
                    else
                    {
                        if (PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri != "about:blank")
                        {
                            PivotMain.SelectedItem = PivotMain.GetFreeItem();
                        }
                    }
                }
            }

            try
            {


                currentWebView.Navigate(new Uri(address));
            }
            catch (Exception) { }
        }

        private void AlignTopBar()
        {
            currentWebView.Margin = new Thickness(0, 50, 0, 0);
            Bookmarks.Margin = new Thickness(0, 80, 0, 0);
        }

        private void CheckNavHistory()
        {
            bool canGoBack = currentWebView.CanGoBack;
            BackButton.IsEnabled = canGoBack;

            bool canGoForward = currentWebView.CanGoForward;
            ForwardButton.IsEnabled = canGoForward;

            if (PivotMain.SelectedWebViewItem.WebViewCore.IsPageLoaded)
            {
                SiteInfoPresenter.Content = PivotMain.SelectedWebViewItem.ListViewItem.Favicon;
            }
        }

        /*  private async void CheckMedia(object sender, object e)
          {
              try
              {
                  bool isFound = false;
                  WebViewPivotItem foundItem = null;
                  foreach (WebViewPivotItem item in PivotMain.Items)
                  {
                      if (item.WebViewCore.IsPageLoaded && item.WebViewCore.IsPageHaveMedia)
                      {
                          isFound = true;
                          foundItem = item;
                          break;
                      }
                  }

                  if (isFound)
                  {
                      MediaControls.IsEnabled = true;
                      MediaControls.DisplayUpdater.Update();

                      if (await foundItem.WebViewCore.WebView.IsPlayingVideo() == true)
                      {
                          MediaControls.DisplayUpdater.Type = MediaPlaybackType.Video;
                          MediaControls.DisplayUpdater.VideoProperties.Title = foundItem.WebViewCore.WebView.DocumentTitle;
                      }
                      else if (await foundItem.WebViewCore.WebView.IsPlayingAudio() == true)
                      {
                          MediaControls.DisplayUpdater.Type = MediaPlaybackType.Music;
                          MediaControls.DisplayUpdater.MusicProperties.Title = foundItem.WebViewCore.WebView.DocumentTitle;
                      }

                      if (await foundItem.WebViewCore.WebView.IsPlayingVideo() == true || await foundItem.WebViewCore.WebView.IsPlayingAudio() == true)
                      {
                          MediaControls.IsPlayEnabled = true;
                          MediaControls.IsPauseEnabled = true;
                          MediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                      }
                      else
                      {
                          MediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                      }
                  }
                  else
                  {
                      MediaControls.IsEnabled = false;
                      MediaControls.DisplayUpdater.Update();
                  }
              }
              catch (Exception) { }
          } */

        private async void Refresh()
        {
            if (!((currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NoInternet.html#") ||
                currentWebView.Source.AbsoluteUri.StartsWith("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/NotFound.html#"))
                && !string.IsNullOrEmpty(currentWebView.Source.AbsoluteUri.Split('#')[1])))
            {
                currentWebView.Refresh();
            }
            else
            {
                await currentWebView.InvokeScriptAsync("eval", new[] { " window.location.replace('" + currentWebView.Source.AbsoluteUri.Split('#')[1] + "');" });
            }
        }

        private WebViewPivotItem GetSiteInTabs(Uri uri)
        {
            foreach (WebViewPivotItem item in PivotMain.Items)
            {
                if (item.WebViewCore.WebView.Source == uri)
                {
                    return item;
                }
            }

            return null;
        }

        private bool IsBookmarkExist
        {
            get
            {
                return favs.Count(x => x.SiteURL == PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri) == 1;
            }
        }

        private void AddOrRemoveBookmark()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView();
            if (!IsBookmarkExist)
            {
                favs.Add(new Bookmark { Title = currentWebView.DocumentTitle, SiteURL = currentWebView.Source.AbsoluteUri });

                BookamrkState.Glyph = "\uE735;";
                BookmarkHyperlinkButton.Content = loader.GetString("UnBookmarkHyperlinkText");
            }
            else
            {
                favs.Remove(favs.First(x => x.SiteURL == currentWebView.Source.AbsoluteUri));

                BookamrkState.Glyph = "\uE734;";
                BookmarkHyperlinkButton.Content = loader.GetString("BookmarkHyperlinkText");
            }
        }

        private async void SavePageAs(WebViewPivotItem item)
        {
            if (!item.WebViewCore.WebView.Source.AbsoluteUri.StartsWith("ms-local-stream://"))
            {
                SaveAsButton.IsEnabled = false;

                FileSavePicker picker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                picker.FileTypeChoices.Add("HTML Files", new string[] { ".htm", ".html" });
                picker.FileTypeChoices.Add("Text Files", new string[] { ".txt", ".text" });
                picker.FileTypeChoices.Add("Video Files", new string[] { ".mp4", ".ogg", ".avi", ".webm", ".mpg", ".mpeg", ".mov", ".wmv" });
                picker.FileTypeChoices.Add("Audio Files", new string[] { ".mp3", ".mp4", ".aac", ".ogg", ".mid", ".midi", ".wma", ".wav" });
                picker.FileTypeChoices.Add("Picture Files", new string[] { ".png", ".apng", ".gif", ".ico", ".cur", ".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp", ".bmp" });
                picker.FileTypeChoices.Add("All files", new string[] { "." });
                picker.SuggestedFileName = item.WebViewCore.WebView.DocumentTitle;

                IAsyncAction asyncAction = Dispatcher.RunAsync(
                    CoreDispatcherPriority.Low,
                    async () =>
                    {
                        // timer.Stop();

                        IAsyncOperation<StorageFile> pickSaveFile = picker.PickSaveFileAsync();
                        StorageFile file = await pickSaveFile;

                        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                        if (file != null)
                        {
                            Uri uri = item.WebViewCore.WebView.Source;

                            IBuffer result;

                            if (uri.AbsoluteUri.StartsWith("ms-appx-web://") || uri.AbsoluteUri.StartsWith("ms-appdata://"))
                            {
                                uri = new Uri(uri.AbsoluteUri.Replace("ms-appx-web://", "ms-appx://"));
                                result = await FileIO.ReadBufferAsync(await StorageFile.GetFileFromApplicationUriAsync(uri));
                            }
                            else
                            {
                                Windows.Web.Http.HttpClient httpclient = new Windows.Web.Http.HttpClient();
                                httpclient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent.GetUserAgent());
                                result = await httpclient.GetBufferAsync(uri);
                            }

                            await FileIO.WriteBufferAsync(file, result);
                        }

                        SaveAsButton.IsEnabled = true;

                        pickSaveFile.Close();

                        //timer.Start();
                    });

                await asyncAction;
                asyncAction.Close();
            }
        }

        private async void Print()
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3)
                && PrintManager.IsSupported())
            {
                try
                {
                    // Show print UI
                    await PrintManager.ShowPrintUIAsync();
                }
                catch
                {
                    // Printing cannot proceed at this time
                    ContentDialog noPrintingDialog = new ContentDialog()
                    {
                        Title = "Printing error",
                        Content = "\r\nSorry, printing can't proceed at this time.",
                        PrimaryButtonText = "OK"
                    };

                    await noPrintingDialog.ShowAsync();
                }
            }
        }
        string StorageIconpath;
        private async void PinSiteToStartMenu()
        {
            // Use a display name you like
            string site = PivotMain.SelectedWebViewItem.WebViewCore.URL.AbsoluteUri;
            string displayName = currentWebView.DocumentTitle;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = currentWebView.Source.DnsSafeHost;
            }

            displayName = displayName.Replace("\"", "\uFF02").Replace("'", "\uFF07").Replace(",", "\uFF0C").Replace(":", "\uFF1A").Replace(";", "\uFF1B").Replace("/", "\uFF0F").Replace("\\", "\uFF3C")
                .Replace("?", "\uFF1F").Replace("<", "\uFF1C").Replace(">", "\uFF1E").Replace("*", "\uFF0A").Replace("|", "\uFF5C").Trim();

            try
            {
                
                var domain = currentWebView.Source.Host;
				///
				/// ADD YOUR API KEY HERE
				///
                string url = $"https://your-api-key.faviconkit.com/{domain}/512";



                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        using (var response = await client.GetAsync(new Uri(url)))
                        {
                            response.EnsureSuccessStatusCode();

                            using (IInputStream inputStream = await response.Content.ReadAsInputStreamAsync())
                            {
                                Debug.WriteLine("Image found");
                                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync($"{domain}.png", CreationCollisionOption.ReplaceExisting);

                                var fileStream = await file.OpenStreamForWriteAsync();

                                var stream = inputStream.AsStreamForRead();

                                await stream.CopyToAsync(fileStream);

                                await fileStream.FlushAsync();
                                fileStream.Dispose();
                                StorageFolder assets = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
                                await file.CopyAsync(assets, $"{domain}.png", NameCollisionOption.ReplaceExisting);
                                StorageIconpath = $"ms-appx:///Assets/{domain}.png";
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to load the image: {0}", ex.Message);
                }

                string tileId = domain + "_Tile";
                string arguments = currentWebView.Source.AbsoluteUri;

                SecondaryTile tile = new SecondaryTile(tileId, displayName, arguments, new Uri(StorageIconpath), TileSize.Default);
                tile.VisualElements.ShowNameOnSquare150x150Logo = true;
                tile.VisualElements.ShowNameOnWide310x150Logo = true;
                tile.VisualElements.ShowNameOnSquare310x310Logo = true;

                bool isPinned = await tile.RequestCreateAsync();

                if (isPinned == true)
                {
                    var CustErr = new MessageDialog($"Successfully pinned to Start Menu");
                    CustErr.Commands.Add(new UICommand("Close"));
                    await CustErr.ShowAsync();
                }
                else
                {
                    var CustErr = new MessageDialog($"Error pinning to Start Menu");
                    CustErr.Commands.Add(new UICommand("Close"));
                    await CustErr.ShowAsync();
                }


                // await AppTile.RequestPinSecondaryTile(site, displayName);
            }
            catch (Exception ex)
            {
                new UCNotification("Error", "Something gone wrong. Onitor can't pin the tile.").ShowNotification();
                //Debug.WriteLine(ex.StackTrace);
            }
        }

        private void ShareURL()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private async void ZoomOut()
        {
            IAsyncOperation<string> asyncOperation = null;
            switch (PivotMain.SelectedWebViewItem.WebViewCore.PageZoom)
            {
                case "100%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { "document.body.style.zoom = '50%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    break;
                case "25%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '10%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    ZoomOutButton.IsEnabled = false;
                    break;
                case "50%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '25%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    break;
                case "200%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '100%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    break;
                case "300%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '200%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    break;
                case "400%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '300%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    break;
                case "500%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '400%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    ZoomInButton.IsEnabled = true;
                    break;
            }
        }

        private async void ZoomIn()
        {
            IAsyncOperation<string> asyncOperation = null;
            switch (PivotMain.SelectedWebViewItem.WebViewCore.PageZoom)
            {
                case "100%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '200%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    break;
                case "10%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '25%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    ZoomOutButton.IsEnabled = true;
                    break;
                case "25%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { "document.body.style.zoom = '50%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    break;
                case "50%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '100%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    break;
                case "200%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '300%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    break;
                case "300%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '400%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    break;
                case "400%":
                    asyncOperation = currentWebView.InvokeScriptAsync("eval", new string[] { @"document.body.style.zoom = '500%';" });
                    PivotMain.SelectedWebViewItem.WebViewCore.PageZoom = await asyncOperation;
                    ZoomPercentTextBlock.Text = PivotMain.SelectedWebViewItem.WebViewCore.PageZoom;
                    ZoomInButton.IsEnabled = false;
                    break;
            }
        }

        private void ExitFullScreen(ApplicationView applicationView)
        {
            applicationView.ExitFullScreenMode();
            SiteInfoPresenter.Visibility = Visibility.Visible;
            AddressAutoSuggestBox.Visibility = Visibility.Visible;
            Bookmarks.Visibility = Visibility.Visible;
            LeftAppTitleBar.Visibility = Visibility.Visible;


        }

        private void ShowFullScreen()
        {
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            applicationView.TryEnterFullScreenMode();
            SecondaryCommands.Hide();
            SiteInfoPresenter.Visibility = Visibility.Collapsed;
            AddressAutoSuggestBox.Visibility = Visibility.Collapsed;
            Bookmarks.Visibility = Visibility.Collapsed;
            LeftAppTitleBar.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region "Key Accelerators"
        private void MakeKeyAccelerators()
        {
            //Windows.UI.Xaml.Input.KeyboardAccelerator is introduced in Windows 10 version 1709
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Input.KeyboardAccelerator"))
            {
                KeyboardAccelerator CtrlT = new KeyboardAccelerator
                {
                    Key = VirtualKey.T,
                    Modifiers = VirtualKeyModifiers.Control
                };
                AddTabButton.AccessKey = "T";
                AddTabButton.KeyboardAccelerators.Add(CtrlT);

                KeyboardAccelerator CtrlW = new KeyboardAccelerator
                {
                    Key = VirtualKey.W,
                    Modifiers = VirtualKeyModifiers.Control
                };
                CloseTabButton.AccessKey = "W";
                CloseTabButton.KeyboardAccelerators.Add(CtrlW);

                KeyboardAccelerator CtrlO = new KeyboardAccelerator
                {
                    Key = VirtualKey.O,
                    Modifiers = VirtualKeyModifiers.Control
                };

                KeyboardAccelerator CtrlS = new KeyboardAccelerator
                {
                    Key = VirtualKey.S,
                    Modifiers = VirtualKeyModifiers.Control
                };
                SaveAsButton.AccessKey = "S";
                SaveAsButton.KeyboardAccelerators.Add(CtrlS);

                KeyboardAccelerator CtrlMinus = new KeyboardAccelerator
                {
                    Key = VirtualKey.Subtract,
                    Modifiers = VirtualKeyModifiers.Control
                };
                ZoomOutButton.AccessKey = "-";
                ZoomOutButton.KeyboardAccelerators.Add(CtrlMinus);

                KeyboardAccelerator CtrlPlus = new KeyboardAccelerator
                {
                    Key = VirtualKey.Add,
                    Modifiers = VirtualKeyModifiers.Control
                };
                ZoomInButton.AccessKey = "+";
                ZoomInButton.KeyboardAccelerators.Add(CtrlPlus);

                KeyboardAccelerator F11 = new KeyboardAccelerator
                {
                    Key = VirtualKey.F11
                };
                FullScreenButton.KeyboardAccelerators.Add(F11);
            }
        }
        #endregion

        #region Design Methods
        private void MakeDesign()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            Brush BasicBackBrush =
                Resources["ApplicationPageBackgroundThemeBrush"] as Brush;

            if (GlobalLocalSettings.titleBarColor == "0")
            {
                LeftAppTitleBar.Background = BasicBackBrush;
            }
            else
            {
                BasicAccentBrush();
            }

            Style FlyoutStyle = new Style { TargetType = typeof(FlyoutPresenter) };

            FlyoutStyle.Setters.Add(new Setter(PaddingProperty,
                0));
            FlyoutStyle.Setters.Add(new Setter(RequestedThemeProperty,
                MainGrid.RequestedTheme));

            if (GlobalLocalSettings.theme == "WD")
            {
                //Adds reveal highlight
                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.RevealBrush"))
                {
                    Style AppBarButtonReveal =
                        Resources["AppBarButtonRevealStyle"] as Style;
                    Style AppBarButtonOverflowReveal =
                        Application.Current.Resources["RightAlignRevealAppBarButton"] as Style;
                    Style AppBarToggleOverflowReveal =
                        Application.Current.Resources["RightAlignRevealAppBarToggle"] as Style;

                    NewWindowButton.Style = AppBarButtonReveal;
                    HomePageButton.Style = AppBarButtonOverflowReveal;
                    AddTabButton2.Style = AppBarButtonReveal;
                    CloseTabButton2.Style = AppBarButtonReveal;
                    InsertButton.Style = AppBarButtonOverflowReveal;
                    FavoritesButton.Style = AppBarButtonOverflowReveal;
                    SaveAsButton.Style = AppBarButtonOverflowReveal;
                    PrintButton.Style = AppBarButtonOverflowReveal;
                    ShareButton.Style = AppBarButtonOverflowReveal;
                    PinButton.Style = AppBarButtonOverflowReveal;
                    ZoomOutButton.Style = AppBarButtonOverflowReveal;
                    ZoomInButton.Style = AppBarButtonOverflowReveal;
                    FullScreenButton.Style = AppBarButtonOverflowReveal;
                    SettingsButton.Style = AppBarButtonOverflowReveal;
                }

                //Adds transparency on flyouts
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
                {
                    UCAcrylicBrush AcrylicSystemBrush =
                        Application.Current.Resources["AcrylicSystemBrush"] as UCAcrylicBrush;

                    UCAcrylicBrush AcrylicElementBrush =
                        Application.Current.Resources["AcrylicElementBrush"] as UCAcrylicBrush;

                    UCAcrylicBrush AcrylicAccentBrush =
                        Application.Current.Resources["AcrylicAccentBrush"] as UCAcrylicBrush;

                    titleBar.BackgroundColor = Colors.Transparent;
                    titleBar.ButtonBackgroundColor = Colors.Transparent;
                    if (GlobalLocalSettings.titleBarColor == "0")
                    {
                        LeftAppTitleBar.Background = AcrylicSystemBrush;
                        //  MiddleAppTitleBar.Background = AcrylicSystemBrush;
                    }
                    else
                    {
                        titleBar.ButtonForegroundColor = Colors.White;
                        titleBar.BackgroundColor = Resources["SystemAccentColor"] as Color?;
                        LeftAppTitleBar.RequestedTheme = ElementTheme.Dark;
                        LeftAppTitleBar.Background = AcrylicAccentBrush;
                        //  MiddleAppTitleBar.RequestedTheme = ElementTheme.Dark;
                        // MiddleAppTitleBar.Background = AcrylicAccentBrush;
                    }

                    Style AcrylicCommandBarStyle = new Style { TargetType = typeof(CommandBar) };
                    AcrylicCommandBarStyle.Setters.Add(new Setter(BackgroundProperty,
                        AcrylicElementBrush));

                    MainCommandBar.Style = AcrylicCommandBarStyle;
                    FlyoutStyle.Setters.Add(new Setter(BackgroundProperty,
                        AcrylicElementBrush));

                    if (GlobalLocalSettings.transparency == "1")
                    {
                        MainGrid.Background = AcrylicSystemBrush;
                    }

                    Bookmarks.PaneBackground = AcrylicElementBrush;

                    Style AcrylicMenuFlyoutStyle = new Style { TargetType = typeof(MenuFlyoutPresenter) };

                    AcrylicMenuFlyoutStyle.Setters.Add(new Setter(BackgroundProperty,
                        AcrylicElementBrush));
                    AcrylicMenuFlyoutStyle.Setters.Add(new Setter(RequestedThemeProperty,
                        MainGrid.RequestedTheme));

                    InsertSelection.MenuFlyoutPresenterStyle = AcrylicMenuFlyoutStyle;
                }
            }

            AllTabsFlyout.FlyoutPresenterStyle = FlyoutStyle;
            SecondaryCommands.FlyoutPresenterStyle = FlyoutStyle;
            SiteInfoFlyout.FlyoutPresenterStyle = FlyoutStyle;
        }

        private void BasicAccentBrush()
        {
            Brush BasicAccentBrush =
                Resources["SystemControlBackgroundAccentBrush"] as Brush;

            titleBar.ButtonBackgroundColor = Colors.Transparent;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
            }
            var color = (Color)Resources["SystemAccentColorDark1"];

            LeftAppTitleBar.RequestedTheme = ElementTheme.Dark;
            LeftAppTitleBar.Background = BasicAccentBrush;
            // MiddleAppTitleBar.RequestedTheme = ElementTheme.Dark;
            // MiddleAppTitleBar.Background = BasicAccentBrush;
            PivotMain.Background = new SolidColorBrush(color);
            SiteInfoPresenter.Background = new SolidColorBrush(color);
            //TopBarGrid.Background = new SolidColorBrush(color);
            MainCommandBar.Background = BasicAccentBrush;
        }
        #endregion



        public async void RetrieveHistory()
        {
            var history = await LocalDataManager.GetData<ObservableCollection<Bookmark>>("SiteHistory.bin");
            if (history != null)
            {
                historyList = history;
            }
            else
            {
                historyList = new ObservableCollection<Bookmark>();
            }

            if (HistoryListView.Items.Count != 0)
            {
                HistoryListView.Items.Clear();
            }

            if (historyListURLs.Count != 0)
            {
                historyListURLs.Clear();
            }

            foreach (var page in historyList)
            {
                if (!page.SiteURL.Contains("ms-appx-web://"))
                {
                    HistoryListView.Items.Add(page);
                    if (page.SiteURL.Contains("https://"))
                    {
                        var newURL = page.SiteURL.Replace("https://", "");
                        historyListURLs.Add(newURL);
                    }
                    else
                    {
                        historyListURLs.Add(page.SiteURL);
                    }
                }
            }

        }
        public async void UpdatePageSettingsList(string domain)
        {
            if (!domain.Contains("about:blank") || !domain.Contains("ms-appx-web://"))
            {
                if (WhitelistedPages.UserExemptPageList.Count != 0)
                {
                    if (WhitelistedPages.UserExemptPageList.Any(d => d.pageDomain.Contains(domain)))
                    {
                        foreach (var item in WhitelistedPages.UserExemptPageList)
                        {
                            if (item.pageDomain.Contains(domain))
                            {

                            }
                        }

                    }
                    else
                    {
                        WhitelistedPages.UserExemptPageList.Add(new WhitelistedPages.PageSettings { pageDomain = domain, isAdsExempt = false, isXhrExempt = false, pageUserAgent = null });
                    }
                }
                await LocalDataManager.SaveData<ObservableCollection<WhitelistedPages.PageSettings>>("PageSettings.bin", WhitelistedPages.UserExemptPageList);
            }
        }





        private void UpdateHistoryView(Bookmark historyPage)
        {
            if (historyPage != null)
            {
                // Debug.WriteLine($"Passed Title: {historyPage.Title} || Real Title: {currentWebView.DocumentTitle}");

                if (!historyPage.SiteURL.Contains("ms-appx-web://") && historyPage.SiteURL.Length != 0 && historyPage.SiteURL != "about:blank")
                {
                    if (historyPage.Title.Length == 0)
                    {
                        historyPage.Title = historyPage.SiteURL;
                    }

                    historyList.Add(historyPage);

                    if (!historyPage.SiteURL.Contains("ms-appx-web://"))
                    {
                        historyListURLs.Add(historyPage.SiteURL);
                    }

                    if (HistoryListView.Items.Count != 0)
                    {
                        HistoryListView.Items.Clear();
                        historyList.Reverse();
                        foreach (var item in historyList)
                        {
                            HistoryListView.Items.Add(item);
                        }
                    }
                    else
                    {
                        historyList.Reverse();
                        foreach (var item in historyList)
                        {
                            if (!item.SiteURL.Contains("ms-appx-web://"))
                            {
                                HistoryListView.Items.Add(item);
                            }
                        }
                    }


                }
                SaveHistoryToFile();
            }


        }

        private async void SaveHistoryToFile()
        {
            await LocalDataManager.SaveData<ObservableCollection<Bookmark>>("SiteHistory.bin", historyList);
        }

        private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryListView.SelectedIndex != -1)
            {
                Bookmark bm = e.AddedItems[0] as Bookmark;
                Navigate(bm.SiteURL);
                HistoryListView.SelectedIndex = -1;
                SecondaryCommands.Hide();
            }
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            HistoryListView.SelectedItem = senderElement.DataContext;

            selectedIndex = HistoryListView.SelectedIndex;

            var test = HistoryListView.SelectedItem as Bookmark;

            currentWebView.Navigate(new Uri(test.SiteURL));
            HistoryPane.IsPaneOpen = false;
        }


        private void HistoryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            selectedIndex = HistoryListView.SelectedIndex;
            string PageUrl = (e.ClickedItem as Bookmark).SiteURL;
            Debug.WriteLine(PageUrl);
            //HistoryGrid.Visibility = Visibility.Collapsed;
            currentWebView.Navigate(new Uri(PageUrl));
            SecondaryCommands.Hide();
        }

        int selectedIndex;
        private async void HistoryListItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            HistoryListView.SelectedItem = senderElement.DataContext;

            selectedIndex = HistoryListView.SelectedIndex;

            var test = HistoryListView.SelectedItem as Bookmark;


            MessageDialog dialog = new MessageDialog($"{test.Title}\n\nRemove this bookmark?");
            dialog.Commands.Add(new UICommand("Yes", null));
            dialog.Commands.Add(new UICommand("No", null));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            var cmd = await dialog.ShowAsync();

            if (cmd.Label == "Yes")
            {

                DeleteHistoryItem_Click();
            }
        }

        private async void DeleteHistoryItem_Click()
        {
            try
            {
                HistoryListView.Items.RemoveAt(selectedIndex);

                historyList.Clear();
                foreach (Bookmark item in HistoryListView.Items)
                {
                    historyList.Add(item);
                }

                SaveHistoryToFile();
            }
            catch (Exception ex)
            {
                var CustErr = new MessageDialog($"{ex.Message}\n{ex.StackTrace}\n{ex.Source}\n\n{selectedIndex}");
                CustErr.Commands.Add(new UICommand("Close"));
                await CustErr.ShowAsync();
            }
        }

        private async void HistoryListItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            HistoryListView.SelectedItem = senderElement.DataContext;

            selectedIndex = HistoryListView.SelectedIndex;

            var test = HistoryListView.SelectedItem as Bookmark;


            MessageDialog dialog = new MessageDialog($"{test.Title}\n\nRemove this bookmark?");
            dialog.Commands.Add(new UICommand("Yes", null));
            dialog.Commands.Add(new UICommand("No", null));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            var cmd = await dialog.ShowAsync();

            if (cmd.Label == "Yes")
            {

                DeleteHistoryItem_Click();
            }
        }
        private async void PopulateTranslateComboBoxes()
        {
            if (LanguageList == null)
            {
                LanguageList = new ObservableCollection<string>();
            }


            string fname = @"Assets\languages.txt";
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await InstallationFolder.GetFileAsync(fname);

            if (File.Exists(file.Path))
            {
                LanguageList.Clear();
                string[] list = File.ReadAllLines(file.Path);
                foreach (var line in list)
                {
                    LanguageList.Add(line);
                }
            }


        }


        private void TranslateWebPage(string uri, string langFrom, string langTo)
        {
            var googleTranslateUrl = $"https://translate.google.com/translate?sl={langFrom}&tl={langTo}&u={uri}";
            currentWebView.Navigate(new Uri(googleTranslateUrl));



        }

        ContentDialog dialog;
        string TranslateFromLanguage;
        string TranslateTolanguage;
        private async void TranslateButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel panel = new StackPanel();


            // Add selection changed events
            TextBlock FromHeader = new TextBlock();
            FromHeader.Margin = new Thickness(5, 5, 5, 5);
            FromHeader.Text = "From";
            FromHeader.HorizontalAlignment = HorizontalAlignment.Left;
            ComboBox fromCombo = new ComboBox();
            //fromCombo.ItemsSource = LanguageList;
            fromCombo.Items.Add("Auto Detect");
            foreach (var item in LanguageList)
            {
                fromCombo.Items.Add(item);
            }

            fromCombo.SelectionChanged += FromCombo_SelectionChanged;
            fromCombo.SelectedIndex = 0;
            fromCombo.MinWidth = 150;
            TextBlock ToHeader = new TextBlock();
            ToHeader.Margin = new Thickness(5, 5, 5, 5);
            ToHeader.Text = "To";
            ToHeader.HorizontalAlignment = HorizontalAlignment.Left;
            ComboBox toCombo = new ComboBox();
            //toCombo.ItemsSource = LanguageList;
            foreach (var item in LanguageList)
            {
                toCombo.Items.Add(item);
            }

            toCombo.MinWidth = 150;
            toCombo.SelectionChanged += ToCombo_SelectionChanged;
            panel.Children.Add(FromHeader);
            panel.Children.Add(fromCombo);
            panel.Children.Add(ToHeader);
            panel.Children.Add(toCombo);

            dialog = new ContentDialog();
            dialog.Content = panel;
            //dialog.Height = 200;
            dialog.VerticalContentAlignment = VerticalAlignment.Center;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Translate";
            dialog.PrimaryButtonClick += Dialog_PrimaryButtonClick;
            dialog.SecondaryButtonText = "Cancel";
            dialog.SecondaryButtonClick += Dialog_SecondaryButtonClick;


            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {




            }
        }

        private void ToCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            var selected = combo.SelectedItem;
            //Debug.WriteLine(selected);
            TranslateTolanguage = onitor.Classes.ParseLang.FetchLanguageCode(selected.ToString());
        }

        private void FromCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            var selected = combo.SelectedItem;
            //Debug.WriteLine(selected);
            if (selected.ToString().Contains("Auto Detect"))
            {
                TranslateFromLanguage = "auto";
            }
            else
            {
                TranslateFromLanguage = onitor.Classes.ParseLang.FetchLanguageCode(selected.ToString());
            }
        }

        private void Dialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            dialog.Hide();
            dialog = null;
        }

        private void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var url = currentWebView.Source.AbsoluteUri;
            TranslateWebPage(url, TranslateFromLanguage, TranslateTolanguage);
            SecondaryCommands.Hide();
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {

            HistoryPane.IsPaneOpen = true;
            //HistoryListView.ItemsSource = 
        }



        private async void BookmarkImportButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".html");
            picker.FileTypeFilter.Add(".htm");

            var htmFile = await picker.PickSingleFileAsync();
            if (htmFile != null)
            {
                ObservableCollection<Bookmark> newList = await ImportBookmarks.ParseBookmarksFile(htmFile);

                if (newList.Count != 0)
                {
                    foreach (var i in newList)
                    {
                        favs.Add(i);
                    }


                }
            }
        }
        public async void SetPageSettingsDefaults()
        {
            if (WhitelistedPages.UserExemptPageList.Count != 0)
            {
                WhitelistedPages.UserExemptPageList.Clear();
            }
            WhitelistedPages.UserExemptPageList.Add(new WhitelistedPages.PageSettings { pageDomain = "www.twitter.com", isAdsExempt = true, isXhrExempt = true, pageUserAgent = "Android/Linux" });
            WhitelistedPages.UserExemptPageList.Add(new WhitelistedPages.PageSettings { pageDomain = "mobile.twitter.com", isAdsExempt = true, isXhrExempt = true, pageUserAgent = "Android/Linux" });
            WhitelistedPages.UserExemptPageList.Add(new WhitelistedPages.PageSettings { pageDomain = "www.facebook.com", isAdsExempt = true, isXhrExempt = true, pageUserAgent = "Samsung" });
            WhitelistedPages.UserExemptPageList.Add(new WhitelistedPages.PageSettings { pageDomain = "m.facebook.com", isAdsExempt = true, isXhrExempt = true, pageUserAgent = "Samsung" });
            WhitelistedPages.UserExemptPageList.Add(new WhitelistedPages.PageSettings { pageDomain = "meet.google.com", isAdsExempt = true, isXhrExempt = true, pageUserAgent = "Firefox" });
            WhitelistedPages.UserExemptPageList.Add(new WhitelistedPages.PageSettings { pageDomain = "bing.com", isAdsExempt = false, isXhrExempt = false, pageUserAgent = "Windows" });
            WhitelistedPages.UserExemptPageList.Add(new WhitelistedPages.PageSettings { pageDomain = "www.bing.com", isAdsExempt = false, isXhrExempt = false, pageUserAgent = "Windows" });

            await LocalDataManager.SaveData<ObservableCollection<WhitelistedPages.PageSettings>>("PageSettings.bin", WhitelistedPages.UserExemptPageList);

        }

        public async void PopulatePageSettings()
        {


            var pageSettingsBin = await ApplicationData.Current.LocalFolder.TryGetItemAsync("PageSettings.bin");
            await Task.Delay(3000);
            if (pageSettingsBin == null)
            {
                SetPageSettingsDefaults();
            }
            else
            {
                ObservableCollection<WhitelistedPages.PageSettings> fetchedList = new ObservableCollection<WhitelistedPages.PageSettings>();

                var LoadedList = await LocalDataManager.GetData<ObservableCollection<WhitelistedPages.PageSettings>>("PageSettings.bin");
                Debug.WriteLine("PageSettings Count: {0}", LoadedList.Count);
                Debug.WriteLine(ApplicationData.Current.LocalFolder.Path);
                if (LoadedList.Count == 0)
                {
                    var CustErr = new MessageDialog($"There was an error loading Page Settings file, resetting config");
                    CustErr.Commands.Add(new UICommand("Close"));
                    await CustErr.ShowAsync();
                    var pageSettings = await ApplicationData.Current.LocalFolder.GetFileAsync("PageSettings.bin");
                    await pageSettings.DeleteAsync();
                    PopulatePageSettings();
                    return;
                }
                foreach (var item in LoadedList)
                {
                    fetchedList.Add(item);
                }
                await Task.Delay(3000);

                foreach (var item in fetchedList)
                {

                    if (WhitelistedPages.UserExemptPageList.Any(d => d.pageDomain.Contains(item.pageDomain)))
                    {
                    }
                    else
                    {

                        WhitelistedPages.UserExemptPageList.Add(new WhitelistedPages.PageSettings { pageDomain = item.pageDomain, isAdsExempt = item.isAdsExempt, isXhrExempt = item.isXhrExempt, pageUserAgent = item.pageUserAgent });
                    }

                    //WhitelistedPages.UserExemptPageList.Add(item);
                }
            }

            if (historyList.Count != 0 && historyList != null)
            {
                foreach (var page in historyList.Distinct())
                {
                    var url = new Uri(page.SiteURL);
                    if (WhitelistedPages.UserExemptPageList.Any(d => d.pageDomain.Contains(url.Host)))
                    {
                    }
                    else
                    {

                        WhitelistedPages.UserExemptPageList.Add(new WhitelistedPages.PageSettings { pageDomain = url.Host, isAdsExempt = false, isXhrExempt = false, pageUserAgent = null });
                    }
                }
            }

        }


        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadsFlyout.ShowAt(PivotMain);
        }
        static ObservableCollection<DownloadInfo> downloadInfos = new ObservableCollection<DownloadInfo>();

        #region DownloadManager Class


        Uri _uri;
        private string FileName;
        private string FileExtension;

        DownloadOperation downloadOperation;
        CancellationTokenSource cancellationToken;
        BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
        ContentDialog downloadDialog;
        DownloadAction DlAction;
        bool isDownloading = false;
        StorageFolder localDownloads;
        TextBox renameFileBox;


        public async void ShowContentDialog(Uri uri)
        {
            try
            {
                if (backgroundDownloader.FailureToastNotification == null)
                {
                    Debug.WriteLine("FailureNotification is Null");
                }
                _uri = uri;

                FileName = Path.GetFileName(_uri.AbsoluteUri);
                FileExtension = Path.GetExtension(_uri.AbsoluteUri);


                Debug.WriteLine(FileName);

                downloadDialog = new ContentDialog()
                {
                    Title = string.Format("What to do with \"{0}\"?", FileName),
                };
                if (downloadInfos.Count != 0)
                {
                    downloadInfos.Clear();
                }
                StackPanel ContentStackPanel = new StackPanel();
                renameFileBox = new TextBox();
                renameFileBox.Text = FileName;
                renameFileBox.MaxWidth = 350;


                ContentStackPanel.Children.Add(new TextBlock() { Text = "File name: " });
                ContentStackPanel.Children.Add(renameFileBox);
                ContentStackPanel.Children.Add(new TextBlock() { Text = "File format: " + FileExtension });
                ContentStackPanel.Children.Add(new TextBlock() { Text = "URL: " + _uri.DnsSafeHost });

                StackPanel ActionStackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                HyperlinkButton SaveAsHyperlinkButton = new HyperlinkButton() { Content = "Save As" };
                SaveAsHyperlinkButton.Click += SaveAsHyperlinkButton_Click;
                ActionStackPanel.Children.Add(SaveAsHyperlinkButton);

                if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                {
                    if (FileExtension != ".exe")
                    {
                        downloadDialog.PrimaryButtonText = "Open";
                        downloadDialog.SecondaryButtonText = "Save";
                    }
                    else
                    {
                        downloadDialog.PrimaryButtonText = "Save";
                    }
                }
                else
                {
                    if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "CloseButtonText") && FileExtension != ".exe")
                    {
                        downloadDialog.PrimaryButtonText = "Open";
                        downloadDialog.SecondaryButtonText = "Save";
                        downloadDialog.CloseButtonText = "Cancel";
                    }
                    else
                    {
                        if (FileExtension != ".exe")
                        {
                            HyperlinkButton OpenHyperlinkButton = new HyperlinkButton() { Content = "Open file" };
                            OpenHyperlinkButton.Click += OpenHyperlinkButton_Click;
                            ActionStackPanel.Children.Insert(0, OpenHyperlinkButton);
                        }

                        downloadDialog.PrimaryButtonText = "Save";
                        downloadDialog.SecondaryButtonText = "Cancel";
                    }
                }

                ContentStackPanel.Children.Add(ActionStackPanel);
                downloadDialog.Content = ContentStackPanel;




                ContentDialogResult result = await downloadDialog.ShowAsync();

                if (result == ContentDialogResult.Primary && downloadDialog.PrimaryButtonText == "Open")
                {
                    localDownloads = await ApplicationData.Current.TemporaryFolder.TryGetItemAsync("Downloads") as StorageFolder;

                    if (localDownloads == null)
                    {
                        localDownloads = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("Downloads");
                    }
                    FileName = renameFileBox.Text;
                    StorageFile file = await localDownloads.CreateFileAsync(FileName, CreationCollisionOption.GenerateUniqueName);
                    DlAction = DownloadAction.Open;
                    Download(file);
                    FileInformation.Text = $"Downloading: {FileName}\n";

                    DownloadHeaderText.Visibility = Visibility.Collapsed;
                    FileInformation.Visibility = Visibility.Visible;
                    FileProgress.Visibility = Visibility.Visible;
                    isDownloading = true;
                }
                else if ((result == ContentDialogResult.Primary && downloadDialog.PrimaryButtonText == "Save")
                    || (result == ContentDialogResult.Secondary && downloadDialog.SecondaryButtonText == "Save"))
                {
                    FileName = renameFileBox.Text;
                    StorageFile file = await DownloadsFolder.CreateFileAsync(FileName, CreationCollisionOption.GenerateUniqueName);
                    DlAction = DownloadAction.Save;
                    Download(file);
                    FileInformation.Text = $"Downloading: {FileName}\n";
                    DownloadHeaderText.Visibility = Visibility.Collapsed;
                    FileInformation.Visibility = Visibility.Visible;
                    FileProgress.Visibility = Visibility.Visible;
                    isDownloading = true;

                }
            }
            catch (Exception ex)
            {
                var CustErr = new MessageDialog($"Exception Raised from Download Class:\n{ex.Message}\n{ex.HResult}\n{ex.InnerException}\n{ex.StackTrace}");
                CustErr.Commands.Add(new UICommand("Close"));
                CustErr.ShowAsync();
            }
        }

        private async void OpenHyperlinkButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            FileName = renameFileBox.Text;
            StorageFile file = await localDownloads.CreateFileAsync(FileName, CreationCollisionOption.GenerateUniqueName);
            DlAction = DownloadAction.Open;
            Download(file);
            FileInformation.Text = $"Downloading: {FileName}\n";

            DownloadHeaderText.Visibility = Visibility.Collapsed;
            FileInformation.Visibility = Visibility.Visible;
            FileProgress.Visibility = Visibility.Visible;
            isDownloading = true;

            downloadDialog.Hide();
        }

        private async void SaveAsHyperlinkButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            FileSavePicker savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("All files", new string[] { "." });
            // Default file name if the user does not type one in or select a file to replace
            FileName = renameFileBox.Text;
            savePicker.SuggestedFileName = FileName;

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                DlAction = DownloadAction.Save;
                Download(file);
                DownloadHeaderText.Visibility = Visibility.Collapsed;
                FileInformation.Visibility = Visibility.Visible;
                FileProgress.Visibility = Visibility.Visible;
                FileInformation.Text = $"Downloading: {FileName}\n";

                isDownloading = true;

            }

            downloadDialog.Hide();
        }

        public async void Download(StorageFile file, string imgLink = null)
        {

            if (imgLink == null)
            {
                downloadOperation = backgroundDownloader.CreateDownload(_uri, file);
            }
            else
            {
                downloadOperation = backgroundDownloader.CreateDownload(new Uri(imgLink), file);

            }
            downloadOperation.Priority = BackgroundTransferPriority.High;
            Progress<DownloadOperation> progress = new Progress<DownloadOperation>();
            progress.ProgressChanged += Progress_ProgressChanged;
            cancellationToken = new CancellationTokenSource();
            try
            {
                new UCNotification(string.Format("Downloading \"{0}\"...", file.Name), "Download started!",
                    DateTime.Now.AddSeconds(4), new ToastAudio() { Silent = true }, ToastDuration.Short).ShowNotification();
                await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);
            }
            catch (TaskCanceledException)
            {
                await downloadOperation.ResultFile.DeleteAsync();
                new UCNotification(string.Format("Download failed \"{0}\"", file.Name), "Download failed!");
                downloadOperation = null;
            }
        }
        public double percent = 100;
        private async void Progress_ProgressChanged(object sender, DownloadOperation e)
        {
            percent = 100;
            if (e.Progress.TotalBytesToReceive > 0)
            {
                percent = e.Progress.BytesReceived * 100 / e.Progress.TotalBytesToReceive;
                FileProgress.Value = percent;
            }

            if (percent == 100 && DlAction == DownloadAction.Open)
            {
                var file = e.ResultFile;
                new UCNotification(string.Format("Downloaded \"{0}\"", file.Name), "Download completed!",
                    DateTime.Now.AddSeconds(4), new ToastAudio() { Silent = true }, ToastDuration.Short).ShowNotification();
                DownloadHeaderText.Visibility = Visibility.Visible;
                FileInformation.Visibility = Visibility.Collapsed;
                FileProgress.Visibility = Visibility.Collapsed;

                isDownloading = false;

                if (file != null)
                {

                    var success = await Launcher.LaunchFileAsync(file);

                    if (success)
                    {
                        // File launched
                    }
                    else
                    {
                        // File launch failed
                    }
                }
                else
                {
                    // Could not find file
                }
            }
            else if (percent == 100)
            {
                var file = e.ResultFile;
                new UCNotification(string.Format("Downloaded \"{0}\"", file.Name), "Download completed!",
                    DateTime.Now.AddSeconds(4), new ToastAudio() { Silent = true }, ToastDuration.Short).ShowNotification();

                FileProgress.Value = percent;
                DownloadHeaderText.Visibility = Visibility.Visible;
                FileInformation.Visibility = Visibility.Collapsed;
                FileProgress.Visibility = Visibility.Collapsed;


                isDownloading = false;
            }
        }




        #region Context Menu Functions and Data Event 

        public static string clickedLink;
        public static string selectedImage;
        public static string imageName;
        public static string selectedText;
        /// <summary>
        /// Check the data recieved from the Console Override data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CheckNotifyData(object sender, EventArgs e)
        {
            try
            {
                //Debug.WriteLine("Reached CheckNotifyData");
                if (e.GetType() == typeof(NotifyData))
                {

                    NotifyData notifyData = (NotifyData)e;
                    if (notifyData != null)
                    {

                        string[] seprator = { "|#|" };
                        var position = notifyData.data.Split(seprator, StringSplitOptions.None);

                        foreach (var item in position)
                        {
                            Debug.WriteLine($"{item}");
                        }
                        var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            if (position[0].Contains("image"))
                            {

                                List<string> imgList = new List<string>();
                                if (position[5].Contains("<img "))
                                {
                                    if (position[5].Contains("https://") || position[5].Contains("http://"))
                                    {
                                        if (imgList.Count != 0)
                                        {
                                            imgList.Clear();
                                        }
                                        foreach (Match m in linkParser.Matches(position[5]))
                                        {
                                            imgList.Add(m.Value);
                                        }
                                        if (imgList.Count != 0)
                                        {
                                            selectedImage = imgList[0];
                                            imageName = position[2];
                                            OpenNewTab.Visibility = Visibility.Visible;
                                            OpenTabSeperator.Visibility = Visibility.Visible;
                                            CopyLinkTextButton.Visibility = Visibility.Visible;
                                            if (!position[5].Contains("base64"))
                                            {
                                                SaveImageSeperator.Visibility = Visibility.Visible;
                                                SaveImageButton.Visibility = Visibility.Visible;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    SaveImageSeperator.Visibility = Visibility.Collapsed;
                                    SaveImageButton.Visibility = Visibility.Collapsed;
                                }

                                if (!position[1].Contains("http://") && !position[1].Contains("https://"))
                                {
                                    OpenNewTab.Visibility = Visibility.Collapsed;
                                    OpenTabSeperator.Visibility = Visibility.Collapsed;
                                    CopyLinkTextButton.Visibility = Visibility.Collapsed;

                                }
                                else
                                {

                                    OpenNewTab.Visibility = Visibility.Visible;
                                    OpenTabSeperator.Visibility = Visibility.Visible;
                                    CopyLinkTextButton.Visibility = Visibility.Visible;

                                    clickedLink = position[1];
                                    CopyTextButton.Visibility = Visibility.Collapsed;
                                    SearchWebButton.Visibility = Visibility.Collapsed;

                                }
                            }
                            else if (position[0].Contains("text"))
                            {

                                selectedText = position[2];
                                SearchWebButton.Visibility = Visibility.Visible;
                                CopyTextButton.Visibility = Visibility.Visible;
                                CopyLinkTextButton.Visibility = Visibility.Collapsed;
                                SaveImageButton.Visibility = Visibility.Collapsed;
                                SaveImageSeperator.Visibility = Visibility.Collapsed;
                                OpenTabSeperator.Visibility = Visibility.Collapsed;

                                OpenNewTab.Visibility = Visibility.Collapsed;

                            }
                            else if (position[0].Contains("link"))
                            {
                                if (position[1].Contains("https://") || position[1].Contains("http://"))
                                {

                                    clickedLink = position[1];
                                    OpenNewTab.Visibility = Visibility.Visible;
                                    CopyLinkTextButton.Visibility = Visibility.Visible;
                                    SaveImageButton.Visibility = Visibility.Collapsed;
                                    SaveImageSeperator.Visibility = Visibility.Collapsed;
                                    CopyTextButton.Visibility = Visibility.Collapsed;
                                    SearchWebButton.Visibility = Visibility.Collapsed;

                                }
                            }

                            var PositionX = double.Parse(position[3]);
                            var PositionY = double.Parse(position[4]);
                            Point point = new Point(PositionX, PositionY);

                            WebContextMenu.ShowAt(currentWebView, point);
                        });
                    }
                }

            } catch (Exception ex)
            {
                var CustErr = new MessageDialog($"Exception Raised from CheckNotifyData:\n{ex.Message}\n{ex.HResult}\n{ex.InnerException}\n{ex.StackTrace}");
                CustErr.Commands.Add(new UICommand("Close"));
                await CustErr.ShowAsync();
            }
            // Debug.WriteLine(e.ToString());
        }

        private void OpenNewTab_Click(object sender, RoutedEventArgs e)
        {
            if (clickedLink != null)
            {
                AddTab();
                currentWebView.Navigate(new Uri(clickedLink));
            }
        }

        private async void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {

            var url = selectedImage;
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add(".jpg");
            folderPicker.FileTypeFilter.Add(".png");
            folderPicker.FileTypeFilter.Add(".bmp");

            var savedImageFolder = await folderPicker.PickSingleFolderAsync();

            //string[] splitURL = url.Split('/');
            //string filename = splitURL.Last();
            StorageFile savedImageFile;
            if (imageName != null)
            {
                savedImageFile = await savedImageFolder.CreateFileAsync($"{imageName}.jpg", CreationCollisionOption.GenerateUniqueName);
            }
            else
            {
                string filename = $"image.jpg";
                savedImageFile = await savedImageFolder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
            }

            Download(savedImageFile, url);

        }

        private void SearchWebButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedText != null)
            {
                Navigate(selectedText, true);
            }
        }

        private void CopyTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedText != null)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetText(selectedText);
                Clipboard.SetContent(dataPackage);
            }
        }


        #endregion

        private void CopyLinkTextButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(clickedLink);
            Clipboard.SetContent(dataPackage);
        }
    }

    public class DownloadInfo
    {
        public string FileInfo { get; set; }
        public double FilePercent { get; set; }
    }


    internal enum DownloadAction
    {
        Save = 0,
        Open = 1
    }

    #endregion



}