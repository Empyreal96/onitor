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

        public SettingsPage()
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

                    ContentGrid.Margin = new Thickness(0, statusBar.OccludedRect.Top, 0, 0);

                    LeftAppTitleBar.Visibility = Visibility.Collapsed;
                }
            }

            appView.VisibleBoundsChanged += appView_VisibleBoundsChanged;
            MakeDesign();
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
            string theme = localSettings.Values["theme"].ToString();

            if (theme == "WD")
            {
                // WindowsDefaultRadioButton.IsChecked = true;

                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
                {
                    TransparencyToggleSwitch.Visibility = Visibility.Visible;
                }
            }
            else if (theme == "Dark")
            {
                DarkRadioButton.IsChecked = true;

                TransparencyToggleSwitch.Visibility = Visibility.Collapsed;
            }
            else if (theme == "Light")
            {
                LightRadioButton.IsChecked = true;

                TransparencyToggleSwitch.Visibility = Visibility.Collapsed;
            }

            string TransparencyBool = localSettings.Values["transparency"].ToString();
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                if (TransparencyBool == "1")
                {
                    TransparencyToggleSwitch.IsOn = true;
                }
                else
                {
                    TransparencyToggleSwitch.IsOn = false;
                }
            }
            else
            {
                TransparencyToggleSwitch.Visibility = Visibility.Collapsed;
            }

            string WebViewTheme = localSettings.Values["WebViewTheme"].ToString();
            if (WebViewTheme == "Default")
            {
                WebViewThemeComboBox.SelectedItem = DefaultWebViewThemeComboBoxItem;
            }
            else if (WebViewTheme == "Light")
            {
                WebViewThemeComboBox.SelectedItem = LightWebViewThemeComboBoxItem;
            }
            else if (WebViewTheme == "Dark")
            {
                WebViewThemeComboBox.SelectedItem = DarkWebViewThemeComboBoxItem;
            }

            string titleBarColor = localSettings.Values["titleBarColor"].ToString();
            if (titleBarColor == "0")
            {
                ThemeColorRadioButton.IsChecked = true;
            }
            else
            {
                AccentColorRadioButton.IsChecked = true;
            }


            string homePage = localSettings.Values["homePage"].ToString();
            HomePageSettingsTextBox.Text = homePage;

            string SearchEngine = localSettings.Values["SearchEngine"].ToString();
            if (SearchEngine == "Bing")
            {
                BingRadioButton.IsChecked = true;
            }
            else if (SearchEngine == "Google")
            {
                GoogleRadioButton.IsChecked = true;
            }
            else if (SearchEngine == "Yahoo")
            {
                YahooRadioButton.IsChecked = true;
            }

            if (ApiInformation.IsTypePresent("Windows.Phone.PhoneContract"))
            {
                string VibrateBool = localSettings.Values["vibrate"].ToString();
                if (VibrateBool == "1")
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

            string DeviceVersion = localSettings.Values["DeviceVersion"].ToString();
            if (DeviceVersion == "Desktop")
            {
                DeviceVersionComboBox.SelectedItem = DesktopComboBoxItem;
            }
            else if (DeviceVersion == "Mobile")
            {
                DeviceVersionComboBox.SelectedItem = MobileComboBoxItem;
            }





            var savedUserAgent = localSettings.Values["SavedUserAgent"] as string;
            if (savedUserAgent == null)
            {
                UserAgentCombo.SelectedItem = WindowsUserAgentItem;
            }
            else
            {
                if (DeviceVersion == "Mobile")
                {
                    UserSelectedUserAgent = UserAgent.ModifyUserAgent(false, savedUserAgent);

                    if (savedUserAgent == "Android")
                    {
                        UserAgentCombo.SelectedItem = AndroidUserAgentItem;

                    }
                    else if (savedUserAgent == "iOS")
                    {
                        UserAgentCombo.SelectedItem = iOSUserAgentItem;

                    }
                    else
                    {
                        UserAgentCombo.SelectedItem = WindowsUserAgentItem;

                    }
                }
                else
                {
                    UserSelectedUserAgent = UserAgent.ModifyUserAgent(true);
                    UserAgentCombo.SelectedItem = WindowsUserAgentItem;

                }
            }
            UserAgentCombo.SelectionChanged += UserAgentCombo_SelectionChanged;


            string JavaScriptBool = localSettings.Values["javaScript"].ToString();
            if (JavaScriptBool == "1")
            {
                JavaScriptToggleSwitch.IsOn = true;
            }
            else
            {
                JavaScriptToggleSwitch.IsOn = false;
            }

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                string WebNotifyPermission = localSettings.Values["WebNotificationPermission"].ToString();
                if (WebNotifyPermission == "1")
                {
                    AlwaysAskWebNotificationsRadioButton.IsChecked = true;
                }
                else if (WebNotifyPermission == "2")
                {
                    AllowWebNotificationsRadioButton.IsChecked = true;
                }
                else if (WebNotifyPermission == "3")
                {
                    BlockWebNotificationsRadioButton.IsChecked = true;
                }
            }
            else
            {
                AccessWebNotifyStackPanel.Visibility = Visibility.Collapsed;
            }

            string LocationPermission = localSettings.Values["LocationPermission"].ToString();
            if (LocationPermission == "1")
            {
                AlwaysAskLocationRadioButton.IsChecked = true;
            }
            else if (LocationPermission == "2")
            {
                AllowLocationRadioButton.IsChecked = true;
            }
            else if (LocationPermission == "3")
            {
                BlockLocationRadioButton.IsChecked = true;
            }

            string MediaPermission = localSettings.Values["MediaPermission"].ToString();
            if (MediaPermission == "1")
            {
                AlwaysAskMediaRadioButton.IsChecked = true;
            }
            else if (MediaPermission == "2")
            {
                AllowMediaRadioButton.IsChecked = true;
            }
            else if (MediaPermission == "3")
            {
                BlockMediaRadioButton.IsChecked = true;
            }

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            ProgramVersionTextBlock.Text = string.Format("{0} {1} {2}.{3}.{4}.{5}", package.DisplayName,
                UnitedCodebase.Classes.DeviceDetails.ProcessorArchitecture, version.Major, version.Minor, version.Build, version.Revision);
            CopyrightTextBlock.Text = string.Format("© 2017-2023 {0}", package.PublisherDisplayName);

            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
            {
                this.FeedbackButton.Visibility = Visibility.Visible;
            }


        }


        private void WindowsDefaultRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string theme = localSettings.Values["theme"].ToString();
            if (theme == "Dark" || theme == "Light")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                TransparencyToggleSwitch.Visibility = Visibility.Visible;
            }

            localSettings.Values["theme"] = "WD";
        }

        private void TransparencyToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (TransparencyToggleSwitch.IsOn == true)
            {
                localSettings.Values["transparency"] = "1";
            }
            else
            {
                localSettings.Values["transparency"] = "0";
            }
        }

        private void LightRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string theme = localSettings.Values["theme"].ToString();
            if (theme == "WD" || theme == "Dark")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            TransparencyToggleSwitch.Visibility = Visibility.Collapsed;

            localSettings.Values["theme"] = "Light";
        }

        private void DarkRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string theme = localSettings.Values["theme"].ToString();
            if (theme == "WD" || theme == "Light")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            TransparencyToggleSwitch.Visibility = Visibility.Collapsed;

            localSettings.Values["theme"] = "Dark";
        }



        private void WebViewThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DefaultWebViewThemeComboBoxItem.IsSelected == true)
            {
                localSettings.Values["WebViewTheme"] = "Default";
            }
            else if (LightWebViewThemeComboBoxItem.IsSelected == true)
            {
                localSettings.Values["WebViewTheme"] = "Light";
            }
            else if (DarkWebViewThemeComboBoxItem.IsSelected == true)
            {
                localSettings.Values["WebViewTheme"] = "Dark";
            }
        }

        private void ThemeColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string titleBarColor = localSettings.Values["titleBarColor"].ToString();
            if (titleBarColor == "1")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            localSettings.Values["titleBarColor"] = "0";
        }

        private void AccentColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string titleBarColor = localSettings.Values["titleBarColor"].ToString();
            if (titleBarColor == "0")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            localSettings.Values["titleBarColor"] = "1";
        }



        private void HomePageSettingsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            localSettings.Values["homePage"] = HomePageSettingsTextBox.Text;
        }

        private void AboutHomeSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            HomePageSettingsTextBox.Text = "about:home";
        }

        private void AboutBlankSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            HomePageSettingsTextBox.Text = "about:blank";
        }

        private void BingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["SearchEngine"] = "Bing";
        }

        private void GoogleRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["SearchEngine"] = "Google";
        }

        private void YahooRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["SearchEngine"] = "Yahoo";
        }

        private void VibrateToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (VibrateToggleSwitch.IsOn == true)
            {
                localSettings.Values["vibrate"] = "1";
            }
            else
            {
                localSettings.Values["vibrate"] = "0";
            }
        }

        private void DeviceVersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DesktopComboBoxItem.IsSelected == true)
            {
                localSettings.Values["DeviceVersion"] = "Desktop";
            }
            else if (MobileComboBoxItem.IsSelected == true)
            {
                localSettings.Values["DeviceVersion"] = "Mobile";
            }
        }

        private void JavaScriptToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (JavaScriptToggleSwitch.IsOn == true)
            {
                localSettings.Values["javaScript"] = "1";
            }
            else
            {
                localSettings.Values["javaScript"] = "0";
            }
        }

        private void AlwaysAskWebNotificationsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["WebNotificationPermission"] = "1";
        }

        private void AllowWebNotificationsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["WebNotificationPermission"] = "2";
        }

        private void BlockWebNotificationsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["WebNotificationPermission"] = "3";
        }

        private void AlwaysAskLocationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["LocationPermission"] = "1";
        }

        private void AllowLocationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["LocationPermission"] = "2";
        }

        private void BlockLocationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["LocationPermission"] = "3";
        }

        private void AlwaysAskMediaRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["MediaPermission"] = "1";
        }

        private void AllowMediaRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["MediaPermission"] = "2";
        }

        private void BlockMediaRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["MediaPermission"] = "3";
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
            string theme = localSettings.Values["theme"].ToString();
            string titleBarColor = localSettings.Values["titleBarColor"].ToString();

            Brush BasicBackBrush =
                Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as Brush;

            if (titleBarColor == "0")
            {
                LeftAppTitleBar.Background = BasicBackBrush;
                MiddleAppTitleBar.Background = BasicBackBrush;

            }
            else
            {
                BasicAccentBrush();
            }

            if (theme == "WD")
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
                    if (titleBarColor == "0")
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

                    string TransparencyBool = localSettings.Values["transparency"].ToString();
                    if (TransparencyBool == "1")
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
                UserSelectedUserAgent = UserAgent.ModifyUserAgent(true);
            }
            UserAgent.SetUserAgent(UserSelectedUserAgent);


            localSettings.Values["SavedUserAgent"] = result;

        }
    }
}