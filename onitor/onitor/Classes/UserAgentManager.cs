using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnitedCodebase.Classes;
using Windows.Foundation.Metadata;
using Windows.Storage;

namespace Onitor
{


    public static class UserAgent
    {
        const int URLMON_OPTION_USERAGENT = 0x10000001;

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkGetSessionOption(int dwOption, StringBuilder pBuffer, int dwBufferLength, ref int pdwBufferLength, int dwReserved);

        public static string GetUserAgent()
        {
            int capacity = 255;
            StringBuilder buf = new StringBuilder(capacity);
            int length = 0;

            UrlMkGetSessionOption(URLMON_OPTION_USERAGENT, buf, capacity, ref length, 0);

            return buf.ToString();
        }

        public static void SetUserAgent(string agent)
        {
            int hr = UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, agent, agent.Length, 0);
            Exception ex = Marshal.GetExceptionForHR(hr);
            if (null != ex)
            {
                throw ex;
            }
            GC.Collect(0, GCCollectionMode.Forced);
        }

        public static void AppendUserAgent(string suffix)
        {
            SetUserAgent(GetUserAgent() + suffix);
        }


        static string OSArchitecture = UnitedCodebaseExtra.Classes.DeviceDetails.OperatingSystemArchitecture, AppArchitecture = UnitedCodebaseExtra.Classes.DeviceDetails.ApplicationArchitecture,
            DeviceManufacturer = DeviceDetails.Manufacturer, PhoneName = DeviceDetails.PhoneName;
        static ulong OSMajor = UnitedCodebaseExtra.Classes.DeviceDetails.OperatingSystemVersion.Major, OSMinor = UnitedCodebaseExtra.Classes.DeviceDetails.OperatingSystemVersion.Minor, OSBuild = UnitedCodebaseExtra.Classes.DeviceDetails.OperatingSystemVersion.Build;

        private static readonly string HybridMobileUserAgent = $"Mozilla/5.0 (Windows Phone {OSMajor}.{OSMinor}; WebView/3.0; {DeviceManufacturer}; {PhoneName}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{ChromeVersion} Mobile Safari/537.36 Edge/{EdgeHTMLversion}.{OSBuild}";
        private static string ChromeVersion
        {
            get
            {
                switch (EdgeHTMLversion)
                {
                    case 13:
                        return "46.0.2486.0";
                    case 14:
                        return "51.0.2704.79";
                    case 15:
                        return "52.0.2743.116";
                    case 16:
                        return "58.0.3029.110";
                    case 17:
                        return "64.0.3282.140";
                    case 18:
                        return "70.0.3538.102";
                }

                return "Chrome/42.0.2311.135";
            }
        }

        private static int EdgeHTMLversion
        {
            get
            {
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7)) return 18;
                else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 6)) return 17;
                else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5)) return 16;
                else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4)) return 15;
                else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3)) return 14;
                else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 2)) return 13;
                return 12;
            }
        }




        static string UserSelectedUserAgent { get; set; }

        public static string ModifyUserAgent(bool isDesktopMode, string selectedAgent = null)
        {
            if (selectedAgent != null)
            {
                if (isDesktopMode)
                {
                    switch (selectedAgent)
                    {

                        case "Android/Linux":
                            return string.Format("Mozilla/5.0 (Linux {0}) AppleWebKit/606.0.0 (KHTML, like Gecko) Version/12.0 Safari/606.0.0 Epiphany/606.0.0", UnitedCodebaseExtra.Classes.DeviceDetails.OperatingSystemArchitecture);
                        case "iOS/Safari":
                            return "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_2_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.3 Safari/605.1.15";
                        case "Windows":
                            return string.Format("Mozilla/5.0 (Windows NT 10.0; {0};) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/605.1.15 Edge/18.19042", UnitedCodebaseExtra.Classes.DeviceDetails.OperatingSystemArchitecture);
                        case "Firefox":
                            return string.Format("Mozilla / 5.0 (Windows NT 10.0; Win32; {0}; rv:111.0) Gecko/20100101 Firefox/111.0", UnitedCodebaseExtra.Classes.DeviceDetails.OperatingSystemArchitecture);
                        case "Samsung":
                            return "Mozilla/5.0 (Linux; Android 13; CPH2197) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/20.0 Chrome/106.0.5249.126 Mobile Safari/537.36";
                        case "Hybrid":
                            return $"Mozilla/5.0 (Windows NT {OSMajor}.{OSMinor}; WebView/3.0; {DeviceManufacturer}; {PhoneName}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{ChromeVersion} Mobile Safari/537.36 Edge/{EdgeHTMLversion}.{OSBuild}";

                    };


                    return string.Format("Mozilla/5.0 (Windows NT 10.0; {0};) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/605.1.15 Edge/18.19042", UnitedCodebaseExtra.Classes.DeviceDetails.OperatingSystemArchitecture);
                }
                else
                {
                    switch (selectedAgent)
                    {

                        case "Android/Linux":
                            return "Mozilla / 5.0(Linux; U; Android 4.4.2; en - us; SCH - I535 Build / KOT49H) AppleWebKit / 534.30(KHTML, like Gecko) Version / 4.0 Mobile Safari/ 534.30";
                        case "iOS/Safari":
                            return "Mozilla / 5.0(iPhone; CPU iPhone OS 10_3_4 like Mac OS X) AppleWebKit / 601.1.56(KHTML, like Gecko) Version / 10.0 EdgiOS / 45.2.16 Mobile / 14G61 Safari/ 601.1.56";
                        case "Windows":
                            return string.Format("Mozilla/5.0 (Windows Phone 10.0; Android 8.0.0; {0}; {1}) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/87.0.4280.141 Mobile Safari/605.1.15 Edge/18.19042", DeviceDetails.Manufacturer, DeviceDetails.PhoneName);
                        case "Firefox":
                            return "Mozilla/5.0 (Android 13; Mobile; rv:68.0) Gecko/68.0 Firefox/111.0";
                        case "Samsung":
                            return "Mozilla/5.0 (Linux; Android 13; CPH2197) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/20.0 Chrome/106.0.5249.126 Mobile Safari/537.36";
                        case "Hybrid":
                            return $"Mozilla/5.0 (Windows Phone {OSMajor}.{OSMinor}; WebView/3.0; {DeviceManufacturer}; {PhoneName}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{ChromeVersion} Mobile Safari/537.36 Edge/{EdgeHTMLversion}.{OSBuild}";
                    };
                }
            }
            else
            {
                if (isDesktopMode)
                {
                    return string.Format("Mozilla/5.0 (Windows NT 10.0; {0};) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/605.1.15 Edge/18.19042", UnitedCodebaseExtra.Classes.DeviceDetails.OperatingSystemArchitecture);

                }
                else
                {
                    return string.Format("Mozilla/5.0 (Windows Phone 10.0; Android 8.0.0; {0}; {1}) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/87.0.4280.141 Mobile Safari/605.1.15 Edge/18.19042", DeviceDetails.Manufacturer, DeviceDetails.PhoneName);

                }
            }

            return null;
        }



        public static string PageSpecificMobileAgents(string domain)
        {
            if (domain.Contains("twitter.com"))
            {
                return "Android/Linux";
            }
            else if (domain.Contains("bing.com"))
            {
                return "Windows";
            }
            else if (domain.Contains("facebook.com") || domain.Contains("instagram.com"))
            {
                return "Samsung";
            }
            else
            {
                return null;
            }


        }

    }
}
