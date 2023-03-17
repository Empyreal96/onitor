using System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Onitor
{
    public class WebViewPivot : Pivot
    {
        public WebViewPivotItem SelectedWebViewItem
        {
            get
            {
                return (WebViewPivotItem)SelectedItem;
            }
        }

        public WebView SelectedWebView
        {
            get
            {
                return SelectedWebViewItem.WebViewCore.WebView;
            }
        }

        public WebViewPivotItem AddTab()
        {
            WebViewPivotItem item = new WebViewPivotItem();
            Items.Add(item);
            SelectedItem = item;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.AddMemoryPressure(16000000);
            return item;
        }

        public bool CloseTab(WebViewPivotItem tab)
        {
            bool canRemove = Items.Count > 1;

            if (canRemove)
            {
                Items.Remove(tab);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.RemoveMemoryPressure(16000000);
            }

            return canRemove;
        }

        public void CloseCurrentTab()
        {
            WebViewPivotItem item = SelectedWebViewItem;
            CloseTab(item);
        }

        public bool FindTab(WebViewPivotItem tabToFind)
        {
            foreach (WebViewPivotItem item in Items)
            {
                if (item == tabToFind)
                {
                    return true;
                }
            }

            return false;
        }

        public WebViewPivotItem GetFreeItem()
        {
            WebViewPivotItem freeItem = null;
            foreach (WebViewPivotItem item in Items)
            {
                if (!(item.WebViewCore.WebView.CanGoBack && item.WebViewCore.WebView.CanGoForward) && item.WebViewCore.WebView.Source.AbsoluteUri == "about:blank")
                {
                    freeItem = item;
                    break;
                }
            }

            return freeItem;
        }
    }
}
