using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace Onitor
{
    public class WebViewPivotItem : PivotItem
    {
        private readonly WebView _webView;
        private readonly WebViewCore _webViewCore;
        private readonly Item _listViewItem;

        public string DefaultUA;

        public WebViewPivotItem() : this(new Item(), new WebViewCore())
        {
        }

        public WebViewPivotItem(Item listViewItem, WebViewCore webViewCore)
        {
            listViewItem.PivotItem = this;
            _listViewItem = listViewItem;
            
            Margin = new Thickness(0, 0, 0, 0);
            Content = webViewCore.WebView;
            TabIndex = 0;
            _webViewCore = webViewCore;
            _webView = webViewCore.WebView;
            Padding = new Thickness(0, 0, 0, 0);

            webViewCore.WebView.Source = new Uri("about:blank");
            webViewCore.WebView.Settings.IsIndexedDBEnabled = true;

            DefaultUA = UserAgent.GetUserAgent();
        }

        public WebViewCore WebViewCore
        {
            get
            {
                return _webViewCore;
            }
        }

        public Item ListViewItem
        {
            get
            {
                return _listViewItem;
            }
        }
    }
}
