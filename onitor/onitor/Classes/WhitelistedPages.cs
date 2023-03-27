using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onitor.Classes
{
    class WhitelistedPages
    {
        public static bool IsWhitelisted(string domain)
        {

            if (domain.Contains("twitter.com"))
            {
                return true;
            }
            else if (domain.Contains("facebook.com"))
            {
                return true;
            }
            else if (domain.Contains("bing.com"))
            {
                return true;
            }
            else if (domain.Contains("instagram.com"))
            {
                return true;
            }
            else if (domain.Contains("meet.google.com"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
