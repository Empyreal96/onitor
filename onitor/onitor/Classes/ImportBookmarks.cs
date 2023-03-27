using HtmlAgilityPack;
using Onitor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace onitor.Classes
{
    class ImportBookmarks
    {
        public static async Task<ObservableCollection<Bookmark>> ParseBookmarksFile(StorageFile exported)
        {

            ObservableCollection<Bookmark> newList = new ObservableCollection<Bookmark>();

            if (exported != null)
            {
                string htmlData = await FileIO.ReadTextAsync(exported);

                var html = new HtmlDocument();
                html.LoadHtml(htmlData);
                foreach (HtmlNode link in html.DocumentNode.SelectNodes("//a[@href]"))
                {
                    //BookmarkInfo.Text += $"{link.InnerText}\n";
                    var attrib = link.Attributes;
                    foreach (var a in attrib)
                    {
                        if (a.Value.Contains("https://") || a.Value.Contains("http://"))
                        {
                            //BookmarkInfo.Text += $"{a.Value}\n\n";
                            newList.Add(new Bookmark { Title = link.InnerText, SiteURL = a.Value });
                        }
                    }
                }
            }
            return newList;
        }
    }
}
