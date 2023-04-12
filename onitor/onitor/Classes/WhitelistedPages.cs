using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onitor.Classes
{



    class WhitelistedPages
    {
        public static ObservableCollection<PageSettings> UserExemptPageList = new ObservableCollection<PageSettings>();


        public static bool CheckPageSettings(string domain, bool CheckXHR = false, bool CheckAds = false)
        {
            foreach (var item in UserExemptPageList)
            {
                if (item.pageDomain.Contains(domain))
                {
                    if (CheckXHR == true)
                    {
                        if (item.isXhrExempt == true)
                        {
                            return true;
                        }

                    }
                    else if (CheckAds == true)
                    {
                        if (item.isAdsExempt == true)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }


        public static string CheckPageUserAgent(string domain)
        {
            if (UserExemptPageList.Count != 0)
            {
                if (UserExemptPageList.Any(d => d.pageDomain.Contains(domain)))
                {
                    foreach (var item in UserExemptPageList)
                    {
                        if (item.pageDomain.Contains(domain))
                        {
                            Debug.WriteLine(item.pageUserAgent);
                            if (item.pageUserAgent == null)
                            {
                                return null;
                            }
                            else
                            {
                                return item.pageUserAgent;
                            }
                        }
                    }

                }
                else
                {
                    return null;
                }
            }
            return null;

        }


        public class PageSettings
        {
            public bool isAdsExempt { get; set; }
            public bool isXhrExempt { get; set; }
            public string pageDomain { get; set; }
            public string pageUserAgent { get; set; }
        }


        public void UserExemptPages(string domain, bool adsAllowed, bool xhrAllowed)
        {

            if (UserExemptPageList.Count != 0)
            {
                if (UserExemptPageList.Any(d => d.pageDomain.Contains(domain)))
                {
                    foreach (var item in UserExemptPageList)
                    {
                        if (item.pageDomain == domain)
                        {
                            item.isAdsExempt = adsAllowed;
                            item.isXhrExempt = xhrAllowed;
                        }
                    }
                }
                else
                {
                    UserExemptPageList.Add(new PageSettings { pageDomain = domain, isAdsExempt = adsAllowed, isXhrExempt = xhrAllowed });
                }
            }

        }
    }
}
