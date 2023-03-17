using System;
using System.Linq;
using System.Threading.Tasks;
using UnitedCodebase.Classes;
using UnitedCodebase.WinRT;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.System.Threading;
using Windows.Networking.Connectivity;
using Windows.Web;
using onitor.Classes;

namespace Onitor
{
    public class WebViewCore
    {
        private WebView _webView;
        private string _pageZoom;

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        WebieHandler taskHandler = new WebieHandler();

        public bool IsPageHaveMedia = false;
        public bool IsPageLoaded = false;

        public bool SupportsOnitorTheme = false;

        public Uri URL;

        public WebViewCore()
        {
            _webView = new WebView(WebViewExecutionMode.SeparateThread);

            PageZoom = "100%";

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                _webView.ContextFlyout = null;
            }

            _webView.Loaded += WebView_Loaded;
            _webView.NavigationStarting += _webView_NavigationStarting;
            _webView.FrameNavigationStarting += _webView_FrameNavigationStarting;
            _webView.ContentLoading += _webView_ContentLoading;
            _webView.FrameNavigationCompleted += _webView_FrameNavigationCompleted;
            _webView.NavigationCompleted += _webView_NavigationCompleted;

            _webView.Settings.IsIndexedDBEnabled = true;

            URL = _webView.Source;

            taskHandler.ReceivedData += TaskHandler_ReceivedData;
        }



        private async void TaskHandler_ReceivedData(string e)
        {
            await ThreadPool.RunAsync((WorkItemHandler) => {
                if(e == "PageHaveMedia")
                {
                    IsPageHaveMedia = true;
                }

                if(e == "SupportingTheme")
                {
                    SupportsOnitorTheme = true;

                    string themeSetting = localSettings.Values["WebViewTheme"].ToString();
                    if (themeSetting == "Default")
                    {
                        var DefaultTheme = new Windows.UI.ViewManagement.UISettings();
                        string uiTheme = DefaultTheme.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
                        if (uiTheme == "#FF000000")
                        {
                            Theme = WebViewTheme.Dark;
                        }
                        else if (uiTheme == "#FFFFFFFF")
                        {
                            Theme = WebViewTheme.Light;
                        }
                    }
                    else if (themeSetting == "Dark")
                    {
                        Theme = WebViewTheme.Dark;
                    }
                    else if (themeSetting == "Light")
                    {
                        Theme = WebViewTheme.Light;
                    }
                }
            });
        }

        private WebViewTheme _theme = WebViewTheme.NotSupported;
        public WebViewTheme Theme
        {
            get
            {
                if (SupportsOnitorTheme)
                {
                    return _theme;
                }
                else
                {
                    _theme = WebViewTheme.NotSupported;
                }

                return _theme;
            }
            set
            {
                _theme = value;

                ChangeOnitorTheme();
            }
        }

        async void ChangeOnitorTheme()
        {
            if(_theme != WebViewTheme.NotSupported)
            {
                if(_theme == WebViewTheme.Dark)
                {
                    string theme = @"
                        var t = document.querySelectorAll('*');
                        for (var i=0; i < t.length; i++) {
                            t[i].setAttribute('onitor-theme', 'dark');
                        }
                    ";

                    await Task.Run(async () =>
                        await _webView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            AsyncEngine.ExecuteString(_webView.InvokeScriptAsync("eval", new[] { theme }))
                        )
                    );
                }
                else
                {
                    string theme = @"
                        var t = document.querySelectorAll('*');
                        for (var i=0; i < t.length; i++) {
                            t[i].setAttribute('onitor-theme', 'light');
                        }
                    ";

                    await Task.Run(async () =>
                        await _webView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>          
                            AsyncEngine.ExecuteString(_webView.InvokeScriptAsync("eval", new[] { theme }))
                    ));
                }
            }
        }


        private void _webView_FrameNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            var url = args.Uri;
            var allowed = BlockedDomains.IsUrlAllowed(url);

            if (!allowed)
            {
                args.Cancel = true;
            }
        }

        private void _webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri != null)
            {
                //setting user agent for mobile
                string DeviceVersion = localSettings.Values["DeviceVersion"].ToString();
                if (DeviceVersion == "Mobile")
                {
                    string DnsSafeHost = args.Uri.DnsSafeHost;
                    if(!UserAgentManager.BadDisplayingSites.Contains(_webView.Domain(DnsSafeHost)) && UserAgentManager.GetMobileOSMode() != UserAgentManager.MobileOSMode.WindowsPhone)
                    {
                        UserAgentManager.ChangeMobileOSUserAgent(UserAgentManager.MobileOSMode.WindowsPhone);
                    }
                    else if(UserAgentManager.BadDisplayingSites.Contains(_webView.Domain(DnsSafeHost)) && UserAgentManager.GetMobileOSMode() != UserAgentManager.MobileOSMode.iOS)
                    {
                        UserAgentManager.ChangeMobileOSUserAgent(UserAgentManager.MobileOSMode.iOS);
                    }
                }

                //redirecting to real page
                if (args.Uri.Scheme == "about" && args.Uri.Segments[0] == "home")
                {
                    args.Cancel = true;
                    sender.Source = new Uri("ms-appx-web://71330982-ba82-4d35-b5cb-3488eefb31ed/PagesHTML/Home.html");
                }


                URL = args.Uri;
            }

            IsPageLoaded = false;
            _webView.AddWebAllowedObject("TaskHandler", taskHandler); //initializing Webie handler
        }

        private void _webView_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            if(args.Uri != null)
            {
                URL = args.Uri;
            }
        }

        private void _webView_FrameNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            IsPageHaveMedia = false;

            AsyncEngine.ExecuteString(_webView.InvokeScriptAsync("eval", new[] { @"
                var videoElem = document.querySelector('video');
                var audioElem = document.querySelector('audio');
                if (videoElem !== null || audioElem !== null)
                {
                    TaskHandler.sendData('PageHaveMedia');
                }
            " })); //checks for a media
        }

        Uri lastPage;
        private async void _webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            IsPageLoaded = true;
            IsPageHaveMedia = false;

            SupportsOnitorTheme = false;

            if (args.Uri != null)
            {
                URL = args.Uri;
            }

            StorageFile extJS = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///ClassesJS/ExtensionUI.js"));
            StorageFile cmJS = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///ClassesJS/ContextMenu.js"));

            //initializing elements for manipulation
            AsyncEngine.ExecuteString(sender.InvokeScriptAsync("eval", new[] { "document.body.style.zoom = '" + PageZoom + "';" }));
            AsyncEngine.ExecuteString(sender.InvokeScriptAsync("eval", new[] { await FileIO.ReadTextAsync(extJS) }));
            AsyncEngine.ExecuteString(sender.InvokeScriptAsync("eval", new[] { await FileIO.ReadTextAsync(cmJS) }));

            //error pages
            if (!args.IsSuccess)
            {
                if (args.WebErrorStatus == WebErrorStatus.NotFound || args.WebErrorStatus == WebErrorStatus.CannotConnect)
                {
                    if (NetworkInformation.GetInternetConnectionProfile() == null
                        || NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel() == NetworkConnectivityLevel.None)
                    {
                        if ((lastPage != null && lastPage == args.Uri) || !ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 6))
                        { 
                            await sender.InvokeScriptAsync("eval", new[] { "window.location.replace('ms-appx-web:///PagesHTML/NoInternet.html#' + location.href);" });
                        }
                        else
                        {
                            sender.Source = new Uri("ms-appx-web:///PagesHTML/NoInternet.html#" + args.Uri);
                        }
                    }
                    else if(NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess 
                        || NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel() == NetworkConnectivityLevel.ConstrainedInternetAccess
                        || NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel() == NetworkConnectivityLevel.LocalAccess)
                    {
                        if ((lastPage != null && lastPage == args.Uri) || !ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 6))
                        {
                            await sender.InvokeScriptAsync("eval", new[] { "window.location.replace('ms-appx-web:///PagesHTML/NotFound.html#' + location.href);" });
                        }
                        else
                        {
                            sender.Source = new Uri("ms-appx-web:///PagesHTML/NotFound.html#" + args.Uri);
                        }
                    }
                }
            }

            lastPage = args.Uri;
        }

        private void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            IsWebViewLoaded = true;
        }

        public bool IsWebViewLoaded { get; private set; } = false;

        public WebView WebView
        {
            get
            {
                return _webView;
            }
        }

        public string PageZoom
        {
            set
            {
                _pageZoom = value;
            }
            get
            {
                return _pageZoom;
            }
        }

        public enum WebViewTheme
        {
            NotSupported,
            Light,
            Dark
        }
    }

    public static class WebViewExtensions
    {
        #region "Media"

        public async static void PlayMedia(this WebView webView)
        {
            string PlayScript =
                @"
                    if(document.body.getElementsByTagName('video').length > 0)
                    {
                        document.body.getElementsByTagName('video')[0].play();
                    }
                    else if(document.body.getElementsByTagName('audio').length > 0)
                    {
                        document.body.getElementsByTagName('audio')[0].play();
                    }
                ";

            await webView.InvokeScriptAsync("eval", new string[] { PlayScript });
        }

        public async static void PauseMedia(this WebView webView)
        {
            string PauseScript =
                @"
                    if(document.body.getElementsByTagName('video').length > 0)
                    {
                        document.body.getElementsByTagName('video')[0].pause();
                    }
                    else if(document.body.getElementsByTagName('audio').length > 0)
                    {
                        document.body.getElementsByTagName('audio')[0].pause();
                    }
                ";

            await webView.InvokeScriptAsync("eval", new string[] { PauseScript });
        }

        public static async Task<bool> IsPlayingVideo(this WebView webView)
        {
            string scriptJS = await webView.InvokeScriptAsync("eval", new string[] { @"
                if(document.body.getElementsByTagName('video').length > 0) { 
                    var video = document.body.getElementsByTagName('video')[0];
                    if(video.currentTime > 0 && !video.paused && !video.ended && video.readyState > 2) { 'true' };
                }
            " });

            return scriptJS == "true";
        }

        public static async Task<bool> IsPlayingAudio(this WebView webView)
        {
            string scriptJS = await webView.InvokeScriptAsync("eval", new string[] { @"
                if(document.body.getElementsByTagName('audio').length > 0) {
                    var audio = document.body.getElementsByTagName('audio')[0];
                    if(audio.currentTime > 0 && !audio.paused && !audio.ended && audio.readyState > 2) { 'true' };
                }
            " });

            return scriptJS == "true";
        }

        #endregion

        public static string Domain(this WebView webView, string sub)
        {
            string[] subdomain = sub.Split('.');
            string domain = sub;
            if (domain.Contains('.'))
            {
                domain = string.Format("{0}.{1}", subdomain[subdomain.Length - 2], subdomain[subdomain.Length - 1]);
            }

            return domain;
        }


        public static async Task<bool> IsFocusedElementEditiable(this WebView webView)
        {
            IAsyncOperation<string> jsEdit = webView.InvokeScriptAsync("eval", new string[] { @"
                const elem = document.activeElement;
                var textControls = ['text', 'search', 'url'];
                if(elem.tagName === 'TEXTAREA' || (elem.tagName === 'INPUT' && textControls.indexOf(elem.type) != -1))
                {
                    'true';
                }
            " });

            return await jsEdit == "true";
        }

        public static async void FocusOnPointer(this WebView webView, int X, int Y)
        {
            await webView.InvokeScriptAsync("eval", new string[] { @" document.elementFromPoint(" + X + ", " + Y + ").focus(); " });
        }

        public static async Task<string> ActiveElement(this WebView webView)
        {
            return await webView.InvokeScriptAsync("eval", new string[] { @" document.activeElement " });
        }

        public static async Task<string> ActiveElementTagName(this WebView webView)
        {
            return await webView.InvokeScriptAsync("eval", new string[] { @" document.activeElement.tagName " });
        }

        public static async Task<string> ActiveElementLink(this WebView webView)
        {
            if (await ActiveElementTagName(webView) == "A")
            {
                string Link = await webView.InvokeScriptAsync("eval", new string[] { @" document.activeElement.href.toString() " });
                if (Link.Length > 0)
                {
                    return Link;
                }
            }
            return null;
        }

        public static async Task<string> SelectionText(this WebView webView)
        {
            string selectionText;
            if (await webView.IsFocusedElementEditiable())
            {
                selectionText = await webView.InvokeScriptAsync("eval", new string[] { @"
                    var elem = document.activeElement;
                    elem.value.substring(elem.selectionStart, elem.selectionEnd);
                " });
            }
            else
            {
                selectionText = await webView.InvokeScriptAsync("eval", new string[] { @"
                     window.getSelection().toString();
                " });
            }

            return selectionText;
        }
    }
}