using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace onitor.Classes
{
    class BlockedDomains
    {
        public static List<string> URL_LIST;
        public static bool IsUrlAllowed(Uri url)
        {
            if (URL_LIST == null)
            {
                URL_LIST = new List<string>();
                ReadDomainList();
            }
            string URL = url.ToString().ToLower();
            
            if (URL_LIST.Count != 0)
            {

                foreach (string item in URL_LIST)
                {
                    var i = item.ToLower();
                    if (URL.Contains(i))
                    {
                        return false;
                    }
                }
                return true;
            }

            return true;

        }

        public static async void ReadDomainList()
        {
            string fname = @"Assets\domains.txt";
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await InstallationFolder.GetFileAsync(fname);

            if (File.Exists(file.Path))
            {
                   string[] list = File.ReadAllLines(file.Path);
                foreach(var line in list)
                {
                    URL_LIST.Add(line);
                }
            }

        }


    }
}
