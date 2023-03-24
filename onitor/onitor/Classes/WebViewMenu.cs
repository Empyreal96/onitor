using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using UnitedCodebase.Classes;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using System.Text.RegularExpressions;
using Windows.UI.Popups;

namespace Onitor
{
    public class WebViewMenu
    {
        WebViewContextFlyout _contextFlyout;
        FrameworkElement _core;
        readonly object _selectionFlyout;
        public WebViewMenu() : this(new WebViewContextFlyout())
        {
        }

        public WebViewMenu(WebViewContextFlyout contextFlyout)
        {
            _contextFlyout = contextFlyout;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                _selectionFlyout = new WebViewSelectionFlyout();
            }
            else
            {
                _selectionFlyout = null;
            }
        }

        public FrameworkElement Core
        {
            get
            {
                return _core;
            }
            set
            {
                _core = value;
                _contextFlyout.Core = value;
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
                {
                    WebViewSelectionFlyout Selection = _selectionFlyout as WebViewSelectionFlyout;
                    Selection.Core = value as WebView;
                }
            }
        }

        public object SelectionFlyout
        {
            get
            {
                return _selectionFlyout;
            }
        }

        public WebViewContextFlyout ContextFlyout
        {
            get
            {
                return _contextFlyout;
            }
        }

        public class WebViewContextFlyout : UCMenuFlyout
        {
            FrameworkElement _core;

            MenuFlyoutItem CutButton;
            MenuFlyoutItem CopyButton;
            MenuFlyoutItem PasteButton;
            MenuFlyoutItem PastenGoButton;
            MenuFlyoutSeparator CutCopyPasteSeparator;
            MenuFlyoutItem OpenLinkNewTabButton;
            MenuFlyoutSeparator LinkSeparator;
            MenuFlyoutItem SelectAllButton;
            MenuFlyoutSeparator SearchSeparator;
            public MenuFlyoutItem SearchButton;

            public WebViewContextFlyout()
            {
                
                    ResourceLoader loader = ResourceLoader.GetForCurrentView();
                    Opening += WebViewContextFlyout_Opening;
                    Opened += WebViewContextFlyout_Opened;
                    Closed += WebViewContextFlyout_Closed;

                    OpenLinkNewTabButton = new MenuFlyoutItem() { Text = loader.GetString("OpenLinkNewTabButton/Text") };
                    OpenLinkNewTabButton.Click += OpenLinkNewTabButton_Click;

                    LinkSeparator = new MenuFlyoutSeparator();
                    Items.Add(LinkSeparator);

                    CutButton = new MenuFlyoutItem() { Text = loader.GetString("CutButton/Text") };
                    CutButton.Click += CutButton_Click;

                    CopyButton = new MenuFlyoutItem() { Text = loader.GetString("CopyButton/Text") };
                    CopyButton.Click += CopyButton_Click;

                    PasteButton = new MenuFlyoutItem() { Text = loader.GetString("PasteButton/Text") };
                    PasteButton.Click += PasteButton_Click;

                    PastenGoButton = new MenuFlyoutItem() { Text = loader.GetString("PastenGoButton/Text") };
                    PastenGoButton.Click += PastenGoButton_Click;

                    CutCopyPasteSeparator = new MenuFlyoutSeparator();
                    Items.Add(CutCopyPasteSeparator);

                    SelectAllButton = new MenuFlyoutItem() { Text = loader.GetString("SelectAllButton/Text") };
                    SelectAllButton.Click += SelectAllButton_Click;
                    Items.Add(SelectAllButton);

                    SearchSeparator = new MenuFlyoutSeparator();
                    Items.Add(SearchSeparator);

                    SearchButton = new MenuFlyoutItem() { Text = "[search]" };
                    SearchButton.Click += SearchButton_Click;

                    if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
                    {
                        CutButton.Icon = new FontIcon { Glyph = "\uE8C6" };
                        CopyButton.Icon = new FontIcon { Glyph = "\uE8C8" };
                        PasteButton.Icon = new FontIcon { Glyph = "\uE77F" };
                    }

                    if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Input.KeyboardAccelerator"))
                    {
                        KeyboardAccelerator CtrlX = new KeyboardAccelerator
                        {
                            Key = VirtualKey.X,
                            Modifiers = VirtualKeyModifiers.Control,
                            IsEnabled = false
                        };
                        CutButton.AccessKey = "X";
                        CutButton.KeyboardAccelerators.Add(CtrlX);

                        KeyboardAccelerator CtrlC = new KeyboardAccelerator
                        {
                            Key = VirtualKey.C,
                            Modifiers = VirtualKeyModifiers.Control,
                            IsEnabled = false
                        };
                        CopyButton.AccessKey = "C";
                        CopyButton.KeyboardAccelerators.Add(CtrlC);

                        KeyboardAccelerator CtrlV = new KeyboardAccelerator
                        {
                            Key = VirtualKey.V,
                            Modifiers = VirtualKeyModifiers.Control,
                            IsEnabled = false
                        };
                        PasteButton.AccessKey = "V";
                        PasteButton.KeyboardAccelerators.Add(CtrlV);

                        KeyboardAccelerator CtrlA = new KeyboardAccelerator
                        {
                            Key = VirtualKey.A,
                            Modifiers = VirtualKeyModifiers.Control,
                            IsEnabled = false
                        };
                        SelectAllButton.AccessKey = "A";
                        SelectAllButton.KeyboardAccelerators.Add(CtrlA);
                    }
               
            }

            private void WebViewContextFlyout_Opening(object sender, object e)
            {
                 ToolTipService.SetToolTip(_core, null);

                    ShowItems();

                    CutCopyPasteSeparator.Visibility = Visibility.Collapsed;
                    if (Items.Contains(CopyButton) || Items.Contains(PasteButton))
                    {
                        CutCopyPasteSeparator.Visibility = Visibility.Visible;
                    }

                    LinkSeparator.Visibility = Visibility.Collapsed;
                    if (Items.Contains(OpenLinkNewTabButton))
                    {
                        LinkSeparator.Visibility = Visibility.Visible;
                    }

                    SearchSeparator.Visibility = Visibility.Collapsed;
                    if (Items.Contains(SearchButton))
                    {
                        SearchSeparator.Visibility = Visibility.Visible;
                    }

                    _core.KeyDown += _core_KeyDown;
              
            }

            internal FrameworkElement Core
            {
                get
                {
                    return _core;
                }
                set
                {
                    _core = value;
                }
            }

            private async void WebViewContextFlyout_Opened(object sender, object e)
            {
                try
                {
                    if (Core is WebView)
                    {
                        WebView coreWView = Core as WebView;

                        string SelectionText = await coreWView.SelectionText();
                        if (Uri.IsWellFormedUriString(SelectionText, UriKind.Absolute) && SelectionText.Contains("."))
                        {
                            Uri selectionUri = new Uri(SelectionText);
                            string uriWithoutScheme = selectionUri.Host + selectionUri.PathAndQuery + selectionUri.Fragment;
                            if (uriWithoutScheme.Length > 20)
                            {
                                SearchButton.Text = string.Format("Go to \"{0}...\"",
                                    uriWithoutScheme.Substring(0, 20));
                            }
                            else
                            {
                                SearchButton.Text = string.Format("Go to \"{0}\"",
                                    uriWithoutScheme.TrimEnd('/'));
                            }
                        }
                        else if (SelectionText.Length > 20)
                        {
                            SearchButton.Text = string.Format("Search \"{0}...\" in The Web",
                                 SelectionText.Substring(0, 20).Replace("\r", " "));
                        }
                        else
                        {
                            SearchButton.Text = string.Format("Search \"{0}\" in The Web",
                                SelectionText.Replace("\r", " "));
                        }

                        if (InputPane.GetForCurrentView().Visible)
                        {
                            InputPane.GetForCurrentView().TryHide();
                        }

                        await Task.Delay(100);
                        coreWView.Focus(FocusState.Keyboard);

                    }
                    else if (Core is AddressTextBox)
                    {
                        AddressTextBox coreATBox = Core as AddressTextBox;

                        if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
                        {
                            coreATBox.TextBox.PreventKeyboardDisplayOnProgrammaticFocus = true;

                            if (InputPane.GetForCurrentView().Visible)
                            {
                                InputPane.GetForCurrentView().TryHide();
                            }
                        }

                        await Task.Delay(100);
                        coreATBox.TextBox.Focus(FocusState.Programmatic);

                        await Task.Delay(50);
                        coreATBox.IsSuggestionListOpen = false;
                        coreATBox.ItemsSource = null;
                    }
                }
                catch (Exception ex)
                {
                    var CustErr = new MessageDialog($"{ex.Message}\n\n{ex.StackTrace}\n\n{ex.InnerException}");
                    CustErr.Commands.Add(new UICommand("Close"));
                    CustErr.ShowAsync();
                }
            }

            private void ShowItems()
            {
                try
                {
                    if (Core is WebView)
                    {
                        WebView coreWView = Core as WebView;
                        WebViewSelection Selection = coreWView.Tag as WebViewSelection;
                        if (!string.IsNullOrEmpty(Selection.SelectionText))
                        {
                            if (Selection.isFocusedElementEditiable)
                            {
                                Items.Insert(Items.IndexOf(CutCopyPasteSeparator), CutButton);
                            }

                            Items.Insert(Items.IndexOf(CutCopyPasteSeparator), CopyButton);
                        }

                        DataPackageView dataPackageView = Clipboard.GetContent();
                        if (dataPackageView.Contains(StandardDataFormats.Text) && Selection.isFocusedElementEditiable)
                        {
                            Items.Insert(Items.IndexOf(CutCopyPasteSeparator), PasteButton);
                        }

                        if (!string.IsNullOrEmpty(Selection.ActiveElementLink))
                        {
                            Items.Insert(Items.IndexOf(LinkSeparator), OpenLinkNewTabButton);
                        }

                        if (!string.IsNullOrEmpty(Selection.SelectionText))
                        {
                            Items.Insert(Items.IndexOf(SearchSeparator) + 1, SearchButton);
                        }

                    }
                    else if (Core is AddressTextBox)
                    {
                        AddressTextBox coreATBox = Core as AddressTextBox;

                        string SelectionText = coreATBox.SelectedText;
                        IsMenuOpened = true;

                        if (SelectionText != string.Empty)
                        {
                            Items.Insert(Items.IndexOf(CutCopyPasteSeparator), CutButton);
                            Items.Insert(Items.IndexOf(CutCopyPasteSeparator), CopyButton);
                        }

                        DataPackageView dataPackageView = Clipboard.GetContent();
                        if (dataPackageView.Contains(StandardDataFormats.Text))
                        {
                            Items.Insert(Items.IndexOf(CutCopyPasteSeparator), PasteButton);
                            Items.Insert(Items.IndexOf(CutCopyPasteSeparator), PastenGoButton);
                        }

                        coreATBox.IsSuggestionListOpen = false;
                        coreATBox.ItemsSource = null;
                    }
                }
                catch (Exception ex)
                {
                    var CustErr = new MessageDialog($"{ex.Message}\n\n{ex.StackTrace}\n\n{ex.Source}");
                    CustErr.Commands.Add(new UICommand("Close"));
                    CustErr.ShowAsync();
                }
            }

            private void _core_KeyDown(object sender, KeyRoutedEventArgs e)
            {
                Hide();
            }

            void HideItems()
            {

                if (Items.Contains(OpenLinkNewTabButton))
                {
                    Items.Remove(OpenLinkNewTabButton);
                }

                if (Items.Contains(CutButton))
                {
                    Items.Remove(CutButton);
                }

                if (Items.Contains(CopyButton))
                {
                    Items.Remove(CopyButton);
                }

                if (Items.Contains(PasteButton))
                {
                    Items.Remove(PasteButton);
                }

                if (Items.Contains(PastenGoButton))
                {
                    Items.Remove(PastenGoButton);
                }

                if (Items.Contains(SearchButton))
                {
                    Items.Remove(SearchButton);
                }
            }

            private async void WebViewContextFlyout_Closed(object sender, object e)
            {
                _core.KeyDown -= _core_KeyDown;

                HideItems();
                if (Core is WebView)
                {
                    WebView coreWView = Core as WebView;

                    await Task.Delay(100);
                    coreWView.Focus(FocusState.Programmatic);
                }
                else if (Core is AddressTextBox)
                {
                    AddressTextBox coreATBox = Core as AddressTextBox;

                    if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
                    {
                        coreATBox.TextBox.Focus(FocusState.Programmatic);
                    }
                    else
                    {
                        coreATBox.TextBox.PreventKeyboardDisplayOnProgrammaticFocus = false;

                        await Task.Delay(20);
                        coreATBox.TextBox.Focus(FocusState.Keyboard);

                        if (!InputPane.GetForCurrentView().Visible)
                        {
                            InputPane.GetForCurrentView().TryShow();
                        }
                    }
                }

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }

            private void CutButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.Cut(_core); }

            private void CopyButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.Copy(_core); }

            private void PasteButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.Paste(_core); }

            private void PastenGoButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.PastenGo(_core as AddressTextBox); }

            private void OpenLinkNewTabButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.OpenLinkNewTab(_core as WebView); }

            private void SelectAllButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.SelectAll(_core); }

            private void SearchButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.Search(_core as WebView); }
        }

        public class WebViewSelectionFlyout : CommandBarFlyout
        {
            WebView _core;

            AppBarButton CutButton;
            AppBarButton CopyButton;
            AppBarButton PasteButton;
            AppBarButton SelectAllButton;
            /*AppBarButton HighlightButton;
            AppBarButton RemoveHighlightButton;
            AppBarSeparator CaseSeparator;
            AppBarButton UppercaseButton;
            AppBarButton LowercaseButton;*/

            AppBarSeparator SearchSeparator;
            AppBarButton SearchButton;

            public WebViewSelectionFlyout()
            {
                try
                {
                    Opened += WebViewSelectionFlyout_Opened;

                    ResourceLoader loader = ResourceLoader.GetForCurrentView();

                    CutButton = new AppBarButton() { Label = loader.GetString("CutButton/Text") };
                    CutButton.Click += CutButton_Click;
                    PrimaryCommands.Add(CutButton);

                    CopyButton = new AppBarButton() { Label = loader.GetString("CopyButton/Text") };
                    CopyButton.Click += CopyButton_Click;
                    PrimaryCommands.Add(CopyButton);

                    PasteButton = new AppBarButton() { Label = loader.GetString("PasteButton/Text") };
                    PasteButton.Click += PasteButton_Click;
                    PrimaryCommands.Add(PasteButton);

                    SelectAllButton = new AppBarButton() { Label = loader.GetString("SelectAllButton/Text") };
                    SelectAllButton.Click += SelectAllButton_Click;
                    SecondaryCommands.Add(SelectAllButton);

                    SearchSeparator = new AppBarSeparator();
                    SecondaryCommands.Add(SearchSeparator);

                    SearchButton = new AppBarButton() { Label = "[search]" };
                    SearchButton.Click += SearchButton_Click;
                    SecondaryCommands.Add(SearchButton);

                    CutButton.Icon = new FontIcon { Glyph = "\uE8C6" };
                    CopyButton.Icon = new FontIcon { Glyph = "\uE8C8" };
                    PasteButton.Icon = new FontIcon { Glyph = "\uE77F" };

                    if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Input.KeyboardAccelerator"))
                    {
                        KeyboardAccelerator CtrlX = new KeyboardAccelerator
                        {
                            Key = VirtualKey.X,
                            Modifiers = VirtualKeyModifiers.Control,
                            IsEnabled = false
                        };
                        CutButton.AccessKey = "X";
                        CutButton.KeyboardAccelerators.Add(CtrlX);

                        KeyboardAccelerator CtrlC = new KeyboardAccelerator
                        {
                            Key = VirtualKey.C,
                            Modifiers = VirtualKeyModifiers.Control,
                            IsEnabled = false
                        };
                        CopyButton.AccessKey = "C";
                        CopyButton.KeyboardAccelerators.Add(CtrlC);

                        KeyboardAccelerator CtrlV = new KeyboardAccelerator
                        {
                            Key = VirtualKey.V,
                            Modifiers = VirtualKeyModifiers.Control,
                            IsEnabled = false
                        };
                        PasteButton.AccessKey = "V";
                        PasteButton.KeyboardAccelerators.Add(CtrlV);

                        KeyboardAccelerator CtrlA = new KeyboardAccelerator
                        {
                            Key = VirtualKey.A,
                            Modifiers = VirtualKeyModifiers.Control,
                            IsEnabled = false
                        };
                        SelectAllButton.AccessKey = "A";
                        SelectAllButton.KeyboardAccelerators.Add(CtrlA);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            private async void WebViewSelectionFlyout_Opened(object sender, object e)
            {
                try
                {
                    WebView coreWView = Core as WebView;

                    string SelectionText = await coreWView.SelectionText();
                    if (await coreWView.IsFocusedElementEditiable())
                    {
                        if (SelectionText != string.Empty)
                        {
                            CutButton.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            CutButton.Visibility = Visibility.Collapsed;
                        }

                        DataPackageView dataPackageView = Clipboard.GetContent();
                        if (dataPackageView.Contains(StandardDataFormats.Text))
                        {
                            PasteButton.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            PasteButton.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        CutButton.Visibility = Visibility.Collapsed;
                        PasteButton.Visibility = Visibility.Collapsed;
                    }

                    if (!string.IsNullOrEmpty(SelectionText))
                    {
                        CopyButton.Visibility = Visibility.Visible;

                        SearchSeparator.Visibility = Visibility.Visible;
                        SearchButton.Visibility = Visibility.Visible;

                        if (Uri.IsWellFormedUriString(SelectionText, UriKind.Absolute) && SelectionText.Contains("."))
                        {
                            Uri selectionUri = new Uri(SelectionText);
                            string uriWithoutScheme = selectionUri.Host + selectionUri.PathAndQuery + selectionUri.Fragment;
                            if (uriWithoutScheme.Length > 20)
                            {
                                SearchButton.Label = string.Format("Go to \"{0}...\"",
                                    uriWithoutScheme.Substring(0, 20));
                            }
                            else
                            {
                                SearchButton.Label = string.Format("Go to \"{0}\"",
                                    uriWithoutScheme.TrimEnd('/'));
                            }
                        }
                        else if (SelectionText.Length > 20)
                        {
                            SearchButton.Label = string.Format("Search \"{0}...\" in The Web",
                                    SelectionText.Substring(0, 20).Replace("\r", " "));
                        }
                        else
                        {
                            SearchButton.Label = string.Format("Search \"{0}\" in The Web",
                            SelectionText.Replace("\r", " "));
                        }
                    }
                    else
                    {
                        CopyButton.Visibility = Visibility.Collapsed;

                        SearchSeparator.Visibility = Visibility.Collapsed;
                        SearchButton.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {

                }
            }

            internal WebView Core
            {
                get
                {
                    return _core;
                }
                set
                {
                    _core = value;
                }
            }

            private void CutButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.Cut(_core); Hide(); }

            private void CopyButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.Copy(_core); Hide(); }

            private void PasteButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.Paste(_core); Hide(); }

            private void SelectAllButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.SelectAll(_core); }

            private void SearchButton_Click(object sender, RoutedEventArgs e) { WebViewMenuEvents.Search(_core); }
        }

        internal class WebViewMenuEvents
        {
            private readonly FrameworkElement _core;

            internal WebViewMenuEvents(FrameworkElement Core)
            {
                _core = Core;
            }

            internal static async void Cut(FrameworkElement Core)
            {
                if (Core is WebView)
                {
                    WebView coreWView = Core as WebView;

                    var dataPackage = new DataPackage();
                    dataPackage.SetText(await coreWView.SelectionText());
                    Clipboard.SetContent(dataPackage);

                    await coreWView.InvokeScriptAsync("eval", new string[] { @" document.execCommand('delete'); " });

                }
                else if (Core is AddressTextBox)
                {
                    AddressTextBox coreATBox = Core as AddressTextBox;

                    var dataPackage = new DataPackage();
                    dataPackage.SetText(coreATBox.SelectedText);
                    Clipboard.SetContent(dataPackage);
                    coreATBox.SelectedText = "";
                }
            }

            internal static async void Copy(FrameworkElement Core)
            {
                if (Core is WebView)
                {
                    WebView coreWView = Core as WebView;

                    var dataPackage = new DataPackage();
                    dataPackage.SetText(await coreWView.SelectionText());
                    Clipboard.SetContent(dataPackage);
                }
                else if (Core is AddressTextBox)
                {
                    AddressTextBox coreATBox = Core as AddressTextBox;
                    coreATBox.Copy();
                }
            }

            internal static async void Paste(FrameworkElement Core)
            {
                if (Core is WebView)
                {
                    WebView coreWView = Core as WebView;

                    var dataPackageView = Clipboard.GetContent();
                    if (dataPackageView.Contains(StandardDataFormats.Text))
                    {
                        string text = await dataPackageView.GetTextAsync();
                        text = Regex.Escape(text.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("'", "&#39;"));
                        try
                        {
                            await coreWView.InvokeScriptAsync("eval", new string[] { string.Format(@"
                                    document.execCommand('removeformat', false, null);
                                    document.execCommand('insertText', false, '{0}'.replace(/\r?\n/g, '\r').replace(/&#39;/g, String.fromCharCode(39)).replace(/&quot;/g,String.fromCharCode(34)).replace(/&amp;/g,String.fromCharCode(38)));
                                    document.activeElement.focus(); ", text) });
                        }
                        catch { }
                    }
                }
                else if (Core is AddressTextBox)
                {
                    AddressTextBox coreATBox = Core as AddressTextBox;
                    coreATBox.Paste();
                }
            }

            internal static async void PastenGo(AddressTextBox Core)
            {
                var dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    var text = await dataPackageView.GetTextAsync();
                    Core.Text = text;
                    var ap = new ButtonAutomationPeer(Core.QueryButton);
                    var ip = ap.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    ip?.Invoke();
                }
            }

            internal static async void OpenLinkNewTab(WebView Core)
            {
                WebView coreWView = Core as WebView;

                WebResources.Navigate("onitor:Go=" + await coreWView.ActiveElementLink());
            }

            internal static async void SelectAll(FrameworkElement Core)
            {
                if (Core is WebView)
                {
                    WebView coreWView = Core as WebView;

                    await coreWView.InvokeScriptAsync("eval", new string[] { @"
                        window.getSelection().removeAllRanges();
                        document.execCommand('selectAll', false, null);
                    " });
                }
                else if (Core is AddressTextBox)
                {
                    AddressTextBox coreATBox = Core as AddressTextBox;
                    coreATBox.TextSelectAll();
                }
            }

            internal static async void Search(WebView Core)
            {
                ApplicationDataContainer localSettings =
                    ApplicationData.Current.LocalSettings;

                string SelectionText = await Core.SelectionText();

                string SearchEngine = localSettings.Values["SearchEngine"].ToString();
                if ((SelectionText.StartsWith("http://") || SelectionText.StartsWith("https://")) && SelectionText.Contains("."))
                {
                    WebResources.Navigate("onitor:Go=" + SelectionText);
                }
                else
                {
                    if (SearchEngine == "Bing")
                    {
                        WebResources.Navigate("onitor:Go=https://www.bing.com/search?q=" + SelectionText);
                    }
                    else if (SearchEngine == "Google")
                    {
                        WebResources.Navigate("onitor:Go=https://www.google.com/search?q=" + SelectionText);
                    }
                    else if (SearchEngine == "Yahoo")
                    {
                        WebResources.Navigate("onitor:Go=https://search.yahoo.com/search?p=" + SelectionText);
                    }
                }
            }
        }
    }
}