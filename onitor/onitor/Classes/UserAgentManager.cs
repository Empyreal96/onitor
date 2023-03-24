using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnitedCodebase.Classes;
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
                            return string.Format("Mozilla/5.0 (Linux {0}) AppleWebKit/606.0.0 (KHTML, like Gecko) Version/12.0 Safari/606.0.0 Epiphany/606.0.0", DeviceDetails.ProcessorArchitecture);
                        case "iOS/Safari":
                            return "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_2_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.3 Safari/605.1.15";
                        case "Windows":
                            return string.Format("Mozilla/5.0 (Windows NT 10.0; {0};) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/605.1.15 Edge/18.19042", DeviceDetails.ProcessorArchitecture);
                        case "Firefox":
                           return string.Format("Mozilla / 5.0 (Windows NT 10.0; Win32; {0}; rv:111.0) Gecko/20100101 Firefox/111.0", DeviceDetails.ProcessorArchitecture);
                    
                    };


                    return string.Format("Mozilla/5.0 (Windows NT 10.0; {0};) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/605.1.15 Edge/18.19042", DeviceDetails.ProcessorArchitecture);
                } else
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
                    };
                }
            } else
            {
                if (isDesktopMode)
                {
                    return string.Format("Mozilla/5.0 (Windows NT 10.0; {0};) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/605.1.15 Edge/18.19042", DeviceDetails.ProcessorArchitecture);

                } else
                {
                    return string.Format("Mozilla/5.0 (Windows Phone 10.0; Android 8.0.0; {0}; {1}) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/87.0.4280.141 Mobile Safari/605.1.15 Edge/18.19042", DeviceDetails.Manufacturer, DeviceDetails.PhoneName);

                }
            }

            return null;
        }


    }
}
