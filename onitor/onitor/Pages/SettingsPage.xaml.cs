using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.Storage;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Core;
using UnitedCodebase.Brushes;
using Windows.Graphics.Display;
using DataManager;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.Web.Http.Filters;
using Windows.Web.Http;
using Windows.Storage.Pickers;
using onitor.Classes;
using System.Linq;
using System.Collections.ObjectModel;
using Windows.UI.StartScreen;
using System.IO;
using Windows.Storage.Streams;

namespace Onitor
{
    public sealed partial class SettingsPage : Page
    {
        SystemNavigationManager currentView =
            SystemNavigationManager.GetForCurrentView();

        ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

        ApplicationView appView = ApplicationView.GetForCurrentView();

        string UserSelectedUserAgent { get; set; }
        string WallpaperScript;
        public SettingsPage()
        {
            try
            {
                InitializeComponent();

                Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;

                this.NavigationCacheMode = NavigationCacheMode.Required;

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                {
                    if (titleBar != null)
                    {
                        ContentGrid.Margin = new Thickness(0, coreTitleBar.Height, 0, 0);
                        SettingsTextBarBlock.Margin = new Thickness(0, 5.5, 64, 0);

                        MiddleAppTitleBar.Margin = new Thickness(64, 0, 0, 0);
                        MiddleAppTitleBar.Height = coreTitleBar.Height;

                        LeftAppTitleBar.Visibility = Visibility.Visible;

                        Window.Current.SetTitleBar(MiddleAppTitleBar);
                    }
                }

                coreTitleBar.LayoutMetricsChanged += coreTitleBar_LayoutMetricsChanged;

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    StatusBar statusBar = StatusBar.GetForCurrentView();
                    if (statusBar != null)
                    {
                        statusBar.BackgroundColor = Colors.Transparent;
                        statusBar.BackgroundOpacity = 0;

                        SettingsTextBarBlock.FontSize = 13;
                        SettingsTextBarBlock.Margin = new Thickness(23, 0.6, 30, 0);
                        MiddleAppTitleBar.Margin = new Thickness(0, -statusBar.OccludedRect.Height, 0, 0);
                        MiddleAppTitleBar.Height = statusBar.OccludedRect.Height;

                        ContentGrid.Margin = new Thickness(0, 20, 0, 0);

                        LeftAppTitleBar.Visibility = Visibility.Visible;
                    }
                }

                if (CurrentSavedDomainList.Items.Count != 0)
                {
                    CurrentSavedDomainList.Items.Clear();

                }
                

                appView.VisibleBoundsChanged += appView_VisibleBoundsChanged;


                MakeDesign();
            }
            catch (Exception ex)
            {
                var CustErr = new MessageDialog($"{ex.Message}\n\n{ex.StackTrace}\n\n{ex.Source}");
                CustErr.Commands.Add(new UICommand("Close"));
                CustErr.ShowAsync();
            }
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            On_BackRequested();
            e.Handled = true;
        }

        private void coreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            ContentGrid.Margin = new Thickness(0, sender.Height, 0, 0);

            SettingsTextBarBlock.Margin = new Thickness(0, 5.5, 64, 0);

            MiddleAppTitleBar.Margin = new Thickness(64, 0, 0, 0);
            MiddleAppTitleBar.Height = sender.Height;
        }

        private void appView_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();

                MiddleAppTitleBar.Height = statusBar.OccludedRect.Height;
                MiddleAppTitleBar.Width = statusBar.OccludedRect.Width;

                SettingsTextBarBlock.Visibility = Visibility.Collapsed;

                DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
                if (displayInformation.CurrentOrientation == DisplayOrientations.Landscape)
                {
                    MiddleAppTitleBar.HorizontalAlignment = HorizontalAlignment.Left;
                    MiddleAppTitleBar.Margin = new Thickness(-MiddleAppTitleBar.Width, -sender.VisibleBounds.Top, 0, -sender.VisibleBounds.Bottom);
                }
                else if (displayInformation.CurrentOrientation == DisplayOrientations.LandscapeFlipped)
                {
                    MiddleAppTitleBar.HorizontalAlignment = HorizontalAlignment.Right;
                    MiddleAppTitleBar.Margin = new Thickness(0, -sender.VisibleBounds.Top, -MiddleAppTitleBar.Width, -sender.VisibleBounds.Bottom);
                }
                else
                {
                    MiddleAppTitleBar.HorizontalAlignment = HorizontalAlignment.Stretch;
                    MiddleAppTitleBar.Margin = new Thickness(0, -statusBar.OccludedRect.Height, 0, 0);
                    SettingsTextBarBlock.Visibility = Visibility.Visible;
                }

                ContentGrid.Margin = new Thickness(0, statusBar.OccludedRect.Top, 0, 0);
            }
        }

        private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            ApplicationView appView = ApplicationView.GetForCurrentView();
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                ApplicationViewTitleBar titleBar = appView.TitleBar;
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
        }

        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            if (args.CurrentPoint.Properties.IsXButton1Pressed)
            {
                On_BackRequested();
                args.Handled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                if (titleBar != null)
                {
                    Window.Current.SetTitleBar(MiddleAppTitleBar);
                }
            }
            PageSettingsLoad();


            currentView.BackRequested += CurrentView_BackRequested;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            Window.Current.SetTitleBar(null);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            On_BackRequested();
        }

        private void SettingsGridMain_Loaded(object sender, RoutedEventArgs e)
        {

            if (GlobalLocalSettings.theme == "WD")
            {
                // WindowsDefaultRadioButton.IsChecked = true;

                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
                {
                    TransparencyEffectToggleSwitch.Visibility = Visibility.Visible;
                }
            }
            else if (GlobalLocalSettings.theme == "Dark")
            {
                DarkRadioButton.IsChecked = true;

                // TransparencyEffectToggleSwitch.Visibility = Visibility.Collapsed;
            }
            else if (GlobalLocalSettings.theme == "Light")
            {
                LightRadioButton.IsChecked = true;

                //TransparencyEffectToggleSwitch.Visibility = Visibility.Collapsed;
            }


            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                if (GlobalLocalSettings.transparency == "1")
                {
                    TransparencyEffectToggleSwitch.IsOn = true;
                }
                else
                {
                    TransparencyEffectToggleSwitch.IsOn = false;
                }
            }
            else
            {
                // TransparencyEffectToggleSwitch.Visibility = Visibility.Collapsed;
            }


            if (GlobalLocalSettings.WebViewTheme == "Default")
            {
                WebViewThemeComboBox.SelectedItem = DefaultWebViewThemeComboBoxItem;
            }
            else if (GlobalLocalSettings.WebViewTheme == "Light")
            {
                WebViewThemeComboBox.SelectedItem = LightWebViewThemeComboBoxItem;
            }
            else if (GlobalLocalSettings.WebViewTheme == "Dark")
            {
                WebViewThemeComboBox.SelectedItem = DarkWebViewThemeComboBoxItem;
            }

            if (GlobalLocalSettings.titleBarColor == "0")
            {
                ThemeColorRadioButton.IsChecked = true;
            }
            else
            {
                AccentColorRadioButton.IsChecked = true;
            }


            HomePageSettingsTextBox.Text = GlobalLocalSettings.homePage;

            if (GlobalLocalSettings.SearchEngine == "Bing")
            {
                SearchEngineComboBox.SelectedIndex = 0;
            }
            else if (GlobalLocalSettings.SearchEngine == "Google")
            {
                SearchEngineComboBox.SelectedIndex = 1;
            }
            else if (GlobalLocalSettings.SearchEngine == "Yahoo")
            {
                SearchEngineComboBox.SelectedIndex = 2;
            }
            else if (GlobalLocalSettings.SearchEngine == "Yandex")
            {
                SearchEngineComboBox.SelectedIndex = 3;
            }
            else if (GlobalLocalSettings.SearchEngine == "Qwant")
            {
                SearchEngineComboBox.SelectedIndex = 4;
            }

            if (ApiInformation.IsTypePresent("Windows.Phone.PhoneContract"))
            {
                if (GlobalLocalSettings.vibrate == "1")
                {
                    VibrateToggleSwitch.IsOn = true;
                }
                else
                {
                    VibrateToggleSwitch.IsOn = false;
                }
            }
            else
            {
                VibrateToggleSwitch.Visibility = Visibility.Collapsed;
                VibrateNotAvailableTextBlock.Visibility = Visibility.Visible;
            }

            if (GlobalLocalSettings.DeviceVersion == "Desktop")
            {
                DeviceVersionComboBox.SelectedItem = DesktopComboBoxItem;
            }
            else if (GlobalLocalSettings.DeviceVersion == "Mobile")
            {
                DeviceVersionComboBox.SelectedItem = MobileComboBoxItem;
            }




            if (GlobalLocalSettings.SavedUserAgent == null)
            {
                UserAgentCombo.SelectedItem = WindowsUserAgentItem;
            }
            else
            {
                if (GlobalLocalSettings.DeviceVersion == "Mobile")
                {
                    UserSelectedUserAgent = UserAgent.ModifyUserAgent(false, GlobalLocalSettings.SavedUserAgent);

                    if (GlobalLocalSettings.SavedUserAgent == "Android/Linux")
                    {
                        UserAgentCombo.SelectedItem = AndroidUserAgentItem;

                    }
                    else if (GlobalLocalSettings.SavedUserAgent == "iOS/Safari")
                    {
                        UserAgentCombo.SelectedItem = iOSUserAgentItem;

                    }
                    else if (GlobalLocalSettings.SavedUserAgent == "Firefox")
                    {
                        UserAgentCombo.SelectedItem = FirefoxUserAgentItem;

                    }
                    else if (GlobalLocalSettings.SavedUserAgent == "Samsung")
                    {
                        UserAgentCombo.SelectedItem = SamsungUserAgentItem;
                    }
                    else if (GlobalLocalSettings.SavedUserAgent == "Hybrid")
                    {
                        UserAgentCombo.SelectedItem = HybridUserAgentItem;
                    }
                    else
                    {
                        UserAgentCombo.SelectedItem = WindowsUserAgentItem;
                    }
                }
                else
                {
                    UserSelectedUserAgent = UserAgent.ModifyUserAgent(true, GlobalLocalSettings.SavedUserAgent);
                    if (GlobalLocalSettings.SavedUserAgent == "Android/Linux")
                    {
                        UserAgentCombo.SelectedItem = AndroidUserAgentItem;

                    }
                    else if (GlobalLocalSettings.SavedUserAgent == "iOS/Safari")
                    {
                        UserAgentCombo.SelectedItem = iOSUserAgentItem;

                    }
                    else if (GlobalLocalSettings.SavedUserAgent == "Firefox")
                    {
                        UserAgentCombo.SelectedItem = FirefoxUserAgentItem;

                    }
                    else if (GlobalLocalSettings.SavedUserAgent == "Hybrid")
                    {
                        UserAgentCombo.SelectedItem = HybridUserAgentItem;
                    }
                    else if (GlobalLocalSettings.SavedUserAgent == "Samsung")
                    {
                        UserAgentCombo.SelectedItem = SamsungUserAgentItem;
                    }
                    else
                    {
                        UserAgentCombo.SelectedItem = WindowsUserAgentItem;
                    }

                }
            }
            UserAgentCombo.SelectionChanged += UserAgentCombo_SelectionChanged;


            if (GlobalLocalSettings.javaScript == "1")
            {
                JavaScriptToggleSwitch.IsOn = true;
            }
            else
            {
                JavaScriptToggleSwitch.IsOn = false;
            }

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {

                if (GlobalLocalSettings.WebNotificationPermission == "1")
                {
                    AlwaysAskWebNotificationsRadioButton.IsChecked = true;
                }
                else if (GlobalLocalSettings.WebNotificationPermission == "2")
                {
                    AllowWebNotificationsRadioButton.IsChecked = true;
                }
                else if (GlobalLocalSettings.WebNotificationPermission == "3")
                {
                    BlockWebNotificationsRadioButton.IsChecked = true;
                }
            }
            else
            {
                AccessWebNotifyStackPanel.Visibility = Visibility.Collapsed;
            }

            if (GlobalLocalSettings.LocationPermission == "1")
            {
                AlwaysAskLocationRadioButton.IsChecked = true;
            }
            else if (GlobalLocalSettings.LocationPermission == "2")
            {
                AllowLocationRadioButton.IsChecked = true;
            }
            else if (GlobalLocalSettings.LocationPermission == "3")
            {
                BlockLocationRadioButton.IsChecked = true;
            }


            if (GlobalLocalSettings.MediaPermission == "1")
            {
                AlwaysAskMediaRadioButton.IsChecked = true;
            }
            else if (GlobalLocalSettings.MediaPermission == "2")
            {
                AllowMediaRadioButton.IsChecked = true;
            }
            else if (GlobalLocalSettings.MediaPermission == "3")
            {
                BlockMediaRadioButton.IsChecked = true;
            }

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            ProgramVersionTextBlock.Text = string.Format("{0} {1} {2}.{3}.{4}.{5}", package.DisplayName,
                 UnitedCodebaseExtra.Classes.DeviceDetails.OperatingSystemArchitecture, version.Major, version.Minor, version.Build, version.Revision);
            CopyrightTextBlock.Text = string.Format("© 2017-2023 {0}", package.PublisherDisplayName);

            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
            {
                this.FeedbackButton.Visibility = Visibility.Collapsed;
            }



            if (GlobalLocalSettings.DebugStats != null)
            {
                if (GlobalLocalSettings.DebugStats == "enabled")
                {
                    DebugStatsToggle.IsOn = true;
                }
                else
                {
                    DebugStatsToggle.IsOn = false;
                }

            }
            else
            {
                DebugStatsToggle.IsOn = false;

            }
            DebugStatsToggle.Toggled += DebugStatsToggle_Toggled;


            if (GlobalLocalSettings.AutoCacheClear != null)
            {
                if (GlobalLocalSettings.AutoCacheClear == "enabled")
                {
                    ClearWebViewCacheAuto.IsOn = true;
                }
                else
                {
                    ClearWebViewCacheAuto.IsOn = false;
                }
            }
            ClearWebViewCacheAuto.Toggled += ClearWebViewCacheAuto_Toggled;

            if (GlobalLocalSettings.JSConsole != null)
            {
                if (GlobalLocalSettings.JSConsole == "enabled")
                {
                    EnableJSConsole.IsOn = true;
                }
                else
                {
                    EnableJSConsole.IsOn = false;

                }
            }
            EnableJSConsole.Toggled += EnableJSConsole_Toggled;


            if (GlobalLocalSettings.AggressiveCacheClean != null)
            {
                if (GlobalLocalSettings.AggressiveCacheClean == "enabled")
                {
                    AggressiveCacheClean.IsOn = true;

                }
                else
                {
                    AggressiveCacheClean.IsOn = false;
                }
            }
            AggressiveCacheClean.Toggled += AggressiveCacheClean_Toggled;
        }

        private void AggressiveCacheClean_Toggled(object sender, RoutedEventArgs e)
        {
            if (AggressiveCacheClean.IsOn)
            {
                localSettings.Values["AggressiveCacheClean"] = "enabled";
                GlobalLocalSettings.AggressiveCacheClean = "enabled";
            }
            else
            {
                localSettings.Values["AggressiveCacheClean"] = "disabled";
                GlobalLocalSettings.AggressiveCacheClean = "disabled";
            }
        }

        private void EnableJSConsole_Toggled(object sender, RoutedEventArgs e)
        {
            if (EnableJSConsole.IsOn)
            {
                localSettings.Values["JSConsole"] = "enabled";
                GlobalLocalSettings.JSConsole = "enabled";
            }
            else
            {
                localSettings.Values["JSConsole"] = "disabled";
                GlobalLocalSettings.JSConsole = "disabled";

            }
        }

        private void ClearWebViewCacheAuto_Toggled(object sender, RoutedEventArgs e)
        {
            if (ClearWebViewCacheAuto.IsOn)
            {
                localSettings.Values["AutoCacheClear"] = "enabled";
                GlobalLocalSettings.AutoCacheClear = "enabled";
            }
            else
            {
                localSettings.Values["AutoCacheClear"] = "disabled";
                GlobalLocalSettings.AutoCacheClear = "disabled";

            }
        }

        private void DebugStatsToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (DebugStatsToggle.IsOn)
            {
                localSettings.Values["DebugStats"] = "enabled";
                GlobalLocalSettings.DebugStats = "enabled";
            }
            else
            {
                localSettings.Values["DebugStats"] = "disabled";
                GlobalLocalSettings.DebugStats = "disabled";

            }
        }

        private void WindowsDefaultRadioButton_Checked(object sender, RoutedEventArgs e)
        {

            if (GlobalLocalSettings.theme == "Dark" || GlobalLocalSettings.theme == "Light")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                TransparencyEffectToggleSwitch.Visibility = Visibility.Visible;
            }

            localSettings.Values["theme"] = "WD";
            GlobalLocalSettings.theme = "WD";
        }

        private void TransparencyToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (TransparencyEffectToggleSwitch.IsOn == true)
            {
                localSettings.Values["transparency"] = "1";
                GlobalLocalSettings.transparency = "1";
            }
            else
            {
                localSettings.Values["transparency"] = "0";
                GlobalLocalSettings.transparency = "0";
            }
        }

        private void LightRadioButton_Checked(object sender, RoutedEventArgs e)
        {

            if (GlobalLocalSettings.theme == "WD" || GlobalLocalSettings.theme == "Dark")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            // TransparencyEffectToggleSwitch.Visibility = Visibility.Collapsed;

            localSettings.Values["theme"] = "Light";
            GlobalLocalSettings.theme = "Light";
        }

        private void DarkRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (GlobalLocalSettings.theme == "WD" || GlobalLocalSettings.theme == "Light")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            // TransparencyEffectToggleSwitch.Visibility = Visibility.Collapsed;

            localSettings.Values["theme"] = "Dark";
            GlobalLocalSettings.theme = "Dark";
        }



        private void WebViewThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DefaultWebViewThemeComboBoxItem.IsSelected == true)
            {
                localSettings.Values["WebViewTheme"] = "Default";
                GlobalLocalSettings.WebViewTheme = "Default";
            }
            else if (LightWebViewThemeComboBoxItem.IsSelected == true)
            {
                localSettings.Values["WebViewTheme"] = "Light";
                GlobalLocalSettings.WebViewTheme = "Light";

            }
            else if (DarkWebViewThemeComboBoxItem.IsSelected == true)
            {
                localSettings.Values["WebViewTheme"] = "Dark";
                GlobalLocalSettings.WebViewTheme = "Dark";

            }
        }

        private void ThemeColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (GlobalLocalSettings.titleBarColor == "1")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            localSettings.Values["titleBarColor"] = "0";
            GlobalLocalSettings.titleBarColor = "0";
        }

        private void AccentColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (GlobalLocalSettings.titleBarColor == "0")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            localSettings.Values["titleBarColor"] = "1";
            GlobalLocalSettings.titleBarColor = "1";
        }



        private void HomePageSettingsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            localSettings.Values["homePage"] = HomePageSettingsTextBox.Text;
            GlobalLocalSettings.homePage = HomePageSettingsTextBox.Text;
        }

        private void AboutHomeSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            HomePageSettingsTextBox.Text = "about:home";
        }

        private void AboutBlankSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            HomePageSettingsTextBox.Text = "about:blank";
        }



        private void VibrateToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (VibrateToggleSwitch.IsOn == true)
            {
                localSettings.Values["vibrate"] = "1";
                GlobalLocalSettings.vibrate = "1";
            }
            else
            {
                localSettings.Values["vibrate"] = "0";
                GlobalLocalSettings.vibrate = "0";
            }
        }

        private void DeviceVersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DesktopComboBoxItem.IsSelected == true)
            {
                localSettings.Values["DeviceVersion"] = "Desktop";
                GlobalLocalSettings.DeviceVersion = "Desktop";
            }
            else if (MobileComboBoxItem.IsSelected == true)
            {
                localSettings.Values["DeviceVersion"] = "Mobile";
                GlobalLocalSettings.DeviceVersion = "Mobile";
            }
        }

        private void JavaScriptToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (JavaScriptToggleSwitch.IsOn == true)
            {
                localSettings.Values["javaScript"] = "1";
                GlobalLocalSettings.javaScript = "1";
            }
            else
            {
                localSettings.Values["javaScript"] = "0";
                GlobalLocalSettings.javaScript = "0";
            }
        }

        private void AlwaysAskWebNotificationsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["WebNotificationPermission"] = "1";
            GlobalLocalSettings.WebNotificationPermission = "1";
        }

        private void AllowWebNotificationsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["WebNotificationPermission"] = "2";
            GlobalLocalSettings.WebNotificationPermission = "2";
        }

        private void BlockWebNotificationsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["WebNotificationPermission"] = "3";
            GlobalLocalSettings.WebNotificationPermission = "3";
        }

        private void AlwaysAskLocationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["LocationPermission"] = "1";
            GlobalLocalSettings.LocationPermission = "1";
        }

        private void AllowLocationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["LocationPermission"] = "2";
            GlobalLocalSettings.LocationPermission = "2";

        }

        private void BlockLocationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["LocationPermission"] = "3";
            GlobalLocalSettings.LocationPermission = "3";

        }

        private void AlwaysAskMediaRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["MediaPermission"] = "1";
            GlobalLocalSettings.MediaPermission = "1";
        }

        private void AllowMediaRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["MediaPermission"] = "2";
            GlobalLocalSettings.MediaPermission = "2";

        }

        private void BlockMediaRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["MediaPermission"] = "3";
            GlobalLocalSettings.MediaPermission = "3";

        }

        private async void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        #region "Methods"
        private bool On_BackRequested()
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();

                return true;
            }
            return false;
        }
        #endregion

        #region Design Methods

        private void MakeDesign()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;


            Brush BasicBackBrush =
                Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as Brush;

            if (GlobalLocalSettings.titleBarColor == "0")
            {
                LeftAppTitleBar.Background = BasicBackBrush;
                MiddleAppTitleBar.Background = BasicBackBrush;

            }
            else
            {
                BasicAccentBrush();
            }

            if (GlobalLocalSettings.theme == "WD")
            {
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
                        MiddleAppTitleBar.Background = AcrylicSystemBrush;
                    }
                    else
                    {
                        titleBar.ButtonForegroundColor = Colors.White;
                        titleBar.BackgroundColor = Resources["SystemAccentColor"] as Color?;
                        LeftAppTitleBar.RequestedTheme = ElementTheme.Dark;
                        LeftAppTitleBar.Background = AcrylicAccentBrush;
                        MiddleAppTitleBar.RequestedTheme = ElementTheme.Dark;
                        MiddleAppTitleBar.Background = AcrylicAccentBrush;
                    }

                    if (GlobalLocalSettings.transparency == "1")
                    {
                        SettingsPG.Background = AcrylicSystemBrush;
                    }
                }

                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
                {
                }
            }
            else
            {
                SettingsPG.Background = BasicBackBrush;
            }
        }

        private void BasicAccentBrush()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            Brush BasicAccentBrush =
                Resources["SystemControlBackgroundAccentBrush"] as Brush;

            titleBar.ButtonBackgroundColor = Colors.Transparent;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
            }

            LeftAppTitleBar.RequestedTheme = ElementTheme.Dark;
            LeftAppTitleBar.Background = BasicAccentBrush;
            MiddleAppTitleBar.RequestedTheme = ElementTheme.Dark;
            MiddleAppTitleBar.Background = BasicAccentBrush;
        }




        #endregion

        private void ClearHistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            LocalDataManager.DeleteData("SiteHistory.bin");
            NoteChangeTextBlock2.Visibility = Visibility.Visible;

        }

        private void UserAgentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var result = ((ComboBoxItem)UserAgentCombo.SelectedItem).Content.ToString();
            var mode = ((ComboBoxItem)DeviceVersionComboBox.SelectedItem).Content.ToString();
            Debug.WriteLine(result);

            if (mode == "Mobile")
            {
                UserSelectedUserAgent = UserAgent.ModifyUserAgent(false, result);
            }
            else
            {
                UserSelectedUserAgent = UserAgent.ModifyUserAgent(true, result);
            }
            UserAgent.SetUserAgent(UserSelectedUserAgent);


            localSettings.Values["SavedUserAgent"] = result;
            GlobalLocalSettings.SavedUserAgent = result;

        }

        private void ClearCookiesBtn_Click(object sender, RoutedEventArgs e)
        {
            HttpBaseProtocolFilter httpFilter = new HttpBaseProtocolFilter();
            HttpCookieManager cookieManager = httpFilter.CookieManager;
            foreach (var item in MainPage.historyList)
            {
                if (item.SiteURL != "about:blank" && item.SiteURL.Contains("ms-appx-web://") == false)
                {
                    Uri uri = new Uri(item.SiteURL);
                    if (item.SiteURL.Contains("https:\\"))
                    {
                        HttpCookieCollection storedCookies = cookieManager.GetCookies(new Uri("https://" + uri.Host));
                        if (storedCookies != null)
                        {
                            foreach (HttpCookie cookie in storedCookies)
                            {
                                cookieManager.DeleteCookie(cookie);
                            }
                        }
                    }
                    else if (item.SiteURL.Contains("http:\\"))
                    {
                        HttpCookieCollection storedCookies = cookieManager.GetCookies(new Uri("http://" + uri.Host));
                        if (storedCookies != null)
                        {
                            foreach (HttpCookie cookie in storedCookies)
                            {
                                cookieManager.DeleteCookie(cookie);
                            }
                        }
                    }
                }
            }

        }

        private async void ChooseHomeWallpaperBtn_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string fname = @"Assets\default.jpg";
                StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                await file.CopyAsync(InstallationFolder, fname, NameCollisionOption.ReplaceExisting);
                StorageFile file2 = await InstallationFolder.GetFileAsync(fname);


                WallpaperScript = "var imagepaper = document.body.style.backgroundImage = \"url('" + file2.Path + ")\"; }";
                await MainPage.currentWebView.InvokeScriptAsync("eval", new string[] { WallpaperScript });
            }
        }

        private void PageSettingsLoad()
        {
            if (CurrentSavedDomainList.Items.Count != 0)
            {
                CurrentSavedDomainList.Items.Clear();
            }

            ObservableCollection<WhitelistedPages.PageSettings> filteredList = new ObservableCollection<WhitelistedPages.PageSettings>(WhitelistedPages.UserExemptPageList.Distinct());
            foreach (var item in filteredList.Distinct())
            {
                Debug.WriteLine($"Domain: {item.pageDomain}, UA {item.pageUserAgent}");
                CurrentSavedDomainList.Items.Add(item);

            }
            if (MainPage.historyList.Count != 0)
            {
                foreach (var page in MainPage.historyList.Distinct())
                {
                    var url = new Uri(page.SiteURL);
                    if (WhitelistedPages.UserExemptPageList.Any(d => d.pageDomain.Contains(url.Host)))
                    {
                    }
                    else
                    {
                        Debug.WriteLine($"Domain: {url.Host}");

                        CurrentSavedDomainList.Items.Add(new WhitelistedPages.PageSettings { pageDomain = url.Host, isAdsExempt = false, isXhrExempt = false, pageUserAgent = null });
                    }
                }
            }

            //PageSettingsSaveBtn_Click(this, new RoutedEventArgs());
        }


        private void CurrentSavedDomainList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedIndex = CurrentSavedDomainList.SelectedIndex;
            var Title = (e.ClickedItem as WhitelistedPages.PageSettings).pageDomain;
            var AdsSettings = (e.ClickedItem as WhitelistedPages.PageSettings).isAdsExempt;
            var XhrSettings = (e.ClickedItem as WhitelistedPages.PageSettings).isXhrExempt;
            var userAgent = (e.ClickedItem as WhitelistedPages.PageSettings).pageUserAgent;

            PageSettingsCustomDomain.Text = Title;
            PageSettingsCheckDomain.IsEnabled = false;
            PageSettingsAdsBlocked.IsChecked = AdsSettings;
            PageSettingsXHRBlocked.IsChecked = XhrSettings;

            switch (userAgent)
            {
                case null:
                    PageSettingsUserAgent.SelectedIndex = 0;
                    break;
                case "Windows":
                    PageSettingsUserAgent.SelectedIndex = 1;
                    break;
                case "Android/Linux":
                    PageSettingsUserAgent.SelectedIndex = 2;
                    break;
                case "iOS/Safari":
                    PageSettingsUserAgent.SelectedIndex = 3;
                    break;
                case "Firefox":
                    PageSettingsUserAgent.SelectedIndex = 4;
                    break;
                case "Samsung":
                    PageSettingsUserAgent.SelectedIndex = 5;
                    break;
                case "Hybrid":
                    PageSettingsUserAgent.SelectedIndex = 6;
                    break;
            }



        }

        private async void PageSettingsSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var domain = PageSettingsCustomDomain.Text;
            var AdsSetting = PageSettingsAdsBlocked.IsChecked;
            var XhrSetting = PageSettingsXHRBlocked.IsChecked;
            string selectedAgent = null;
            switch (PageSettingsUserAgent.SelectedIndex)
            {
                case 0:
                    selectedAgent = null;
                    break;
                case 1:
                    selectedAgent = "Windows";
                    break;
                case 2:
                    selectedAgent = "Android/Linux";
                    break;
                case 3:
                    selectedAgent = "iOS/Safari";
                    break;
                case 4:
                    selectedAgent = "Firefox";
                    break;
                case 5:
                    selectedAgent = "Samsung";
                    break;
                case 6:
                    selectedAgent = "Hybrid";
                    break;
            }


            if (WhitelistedPages.UserExemptPageList.Any(d => d.pageDomain.Contains(domain)))
            {
                foreach (var item in WhitelistedPages.UserExemptPageList)
                {

                    if (item.pageDomain == domain)
                    {
                        item.isAdsExempt = (bool)AdsSetting;
                        item.isXhrExempt = (bool)XhrSetting;
                        item.pageUserAgent = selectedAgent;
                    }
                }
            }
            else
            {
                WhitelistedPages.UserExemptPageList.Add(new WhitelistedPages.PageSettings { pageDomain = domain, isAdsExempt = (bool)AdsSetting, isXhrExempt = (bool)XhrSetting, pageUserAgent = selectedAgent });
            }

            CurrentSavedDomainList.Items.Clear();
            foreach (var item in WhitelistedPages.UserExemptPageList)
            {
                CurrentSavedDomainList.Items.Add(item);
            }
            var newList = new ObservableCollection<WhitelistedPages.PageSettings>();
            foreach (WhitelistedPages.PageSettings item in CurrentSavedDomainList.Items)
            {
                Debug.WriteLine(item.pageDomain);
                newList.Add(item);
            }

            await LocalDataManager.SaveData<ObservableCollection<WhitelistedPages.PageSettings>>("PageSettings.bin", newList);
        }

        private void PageSettingsCustomDomain_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            PageSettingsCheckDomain.IsEnabled = true;
        }


        int selectedIndex;

        private async void DeletePageSetting()
        {
            try
            {
                CurrentSavedDomainList.Items.RemoveAt(selectedIndex);

                var updatedList = new ObservableCollection<WhitelistedPages.PageSettings>();
                foreach (WhitelistedPages.PageSettings item in CurrentSavedDomainList.Items)
                {
                    updatedList.Add(item);
                }
                await LocalDataManager.SaveData<ObservableCollection<WhitelistedPages.PageSettings>>("PageSettings.bin", updatedList);

            }
            catch (Exception ex)
            {

            }
        }

        private async void StackPanel_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            CurrentSavedDomainList.SelectedItem = senderElement.DataContext;

            selectedIndex = CurrentSavedDomainList.SelectedIndex;

            var test = CurrentSavedDomainList.SelectedItem as WhitelistedPages.PageSettings;


            MessageDialog dialog = new MessageDialog($"{test.pageDomain}\n\nRemove this config?");
            dialog.Commands.Add(new UICommand("Yes", null));
            dialog.Commands.Add(new UICommand("No", null));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            var cmd = await dialog.ShowAsync();

            if (cmd.Label == "Yes")
            {
                DeletePageSetting();
            }
        }

        private async void StackPanel_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            CurrentSavedDomainList.SelectedItem = senderElement.DataContext;

            selectedIndex = CurrentSavedDomainList.SelectedIndex;

            var test = CurrentSavedDomainList.SelectedItem as WhitelistedPages.PageSettings;


            MessageDialog dialog = new MessageDialog($"{test.pageDomain}\n\nRemove this config?");
            dialog.Commands.Add(new UICommand("Yes", null));
            dialog.Commands.Add(new UICommand("No", null));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            var cmd = await dialog.ShowAsync();

            if (cmd.Label == "Yes")
            {
                DeletePageSetting();
            }
        }

        private void PageSettingsCheckDomain_Click(object sender, RoutedEventArgs e)
        {
            var userDomain = PageSettingsCustomDomain.Text;

            foreach (WhitelistedPages.PageSettings item in CurrentSavedDomainList.Items)
            {
                if (item.pageDomain == userDomain)
                {
                    PageSettingsXHRBlocked.IsChecked = item.isXhrExempt;
                    PageSettingsAdsBlocked.IsChecked = item.isAdsExempt;
                    switch (item.pageUserAgent)
                    {
                        case null:
                            PageSettingsUserAgent.SelectedIndex = 0;
                            break;
                        case "Windows":
                            PageSettingsUserAgent.SelectedIndex = 1;
                            break;
                        case "Android/Linux":
                            PageSettingsUserAgent.SelectedIndex = 2;
                            break;
                        case "iOS/Safari":
                            PageSettingsUserAgent.SelectedIndex = 3;
                            break;
                        case "Firefox":
                            PageSettingsUserAgent.SelectedIndex = 4;
                            break;
                        case "Samsung":
                            PageSettingsUserAgent.SelectedIndex = 5;
                            break;
                        case "Hybrid":
                            PageSettingsUserAgent.SelectedIndex = 6;
                            break;
                    }

                }
            }
        }

        private void SearchEngineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = SearchEngineComboBox.SelectedIndex;

            switch (selectedIndex)
            {
                case 0:
                    localSettings.Values["SearchEngine"] = "Bing";
                    GlobalLocalSettings.SearchEngine = "Bing";
                    break;
                case 1:
                    localSettings.Values["SearchEngine"] = "Google";
                    GlobalLocalSettings.SearchEngine = "Google";

                    break;
                case 2:
                    localSettings.Values["SearchEngine"] = "Yahoo";
                    GlobalLocalSettings.SearchEngine = "Yahoo";

                    break;
                case 3:
                    localSettings.Values["SearchEngine"] = "Yandex";
                    GlobalLocalSettings.SearchEngine = "Yandex";

                    break;
                case 4:
                    localSettings.Values["SearchEngine"] = "Qwant";
                    GlobalLocalSettings.SearchEngine = "Qwant";

                    break;
            }
        }
        string StorageIconpath;
        private async void FixTilesButton_Click(object sender, RoutedEventArgs e)
        {
            var tiles = await SecondaryTile.FindAllAsync();

            if (tiles.Count != 0)
            {
                foreach (var tile in tiles)
                {


                    try
                    {
                        var args = new Uri(tile.Arguments);
                        var domain = args.Host;
						
						///
						/// YOUR API KEY HERE
						///
                        string url = $"https://your-api-keyfaviconkit.com/{domain}/512";



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
                            var CustErr = new MessageDialog($"{ex.Message}\n{ex.StackTrace}");
                            CustErr.Commands.Add(new UICommand("Close"));
                            CustErr.ShowAsync();
                        }
                        tile.VisualElements.Square150x150Logo = new Uri(StorageIconpath);
                        await tile.UpdateAsync();
                    }
                    catch (Exception ex)
                    {
                        var CustErr = new MessageDialog($"{ex.Message}\n{ex.StackTrace}");
                        CustErr.Commands.Add(new UICommand("Close"));
                        CustErr.ShowAsync();
                    }
                }
            }
            else
            {
                var CustErr = new MessageDialog($"No tiles found");
                CustErr.Commands.Add(new UICommand("Close"));
                CustErr.ShowAsync();
            }

            FixTilesButton.IsEnabled = false;
        }
    }
}