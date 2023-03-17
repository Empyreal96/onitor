using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnitedCodebase.Classes;
using Windows.Storage;

namespace Onitor
{
    public class UserAgentManager
    {
        public static ArrayList BadDisplayingSites = new ArrayList { "facebook.com", "twitter.com" };

        static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private static readonly string DesktopUserAgent = string.Format("Mozilla/5.0 (Windows NT 10.0; {0};) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/605.1.15 Edge/18.19042",
            DeviceDetails.ProcessorArchitecture);
        private static readonly string MobileUserAgent = string.Format("Mozilla/5.0 (Windows Phone 10.0; Android 8.0.0; {0}; {1}) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/87.0.4280.141 Mobile Safari/605.1.15 Edge/18.19042",
            DeviceDetails.Manufacturer, DeviceDetails.PhoneName);
        //Mozilla/5.0 (Windows Phone 10.0; Android 6.0.1; Microsoft; Lumia 650) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Mobile Safari/537.36 Edge/15.15063
        private static readonly string iOSUserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 10_3_4 like Mac OS X) AppleWebKit/601.1.56 (KHTML, like Gecko) Version/10.0 EdgiOS/45.2.16 Mobile/14G61 Safari/601.1.56";
        private static readonly string AndroidUserAgent = "Mozilla/5.0 (Linux; U; Android 4.4.2; en-us; SCH-I535 Build/KOT49H) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";
        //Mozilla/5.0 (iPhone; CPU iPhone OS 10_3_4 like Mac OS X) AppleWebKit/601.1.56 (KHTML, like Gecko) Version/10.0 Mobile/14G61 Safari/601.1.56
        //Mozilla/5.0 (iPhone; CPU iPhone OS 10_3_4 like Mac OS X) AppleWebKit/601.1.56 (KHTML, like Gecko) FxiOS/12.1b10941 Mobile/14G61 Safari/601.1.56

        public static void ChangeUserAgent(DeviceMode mode)
        {
            if (mode == DeviceMode.Desktop)
            {
                UserAgent.SetUserAgent(DesktopUserAgent);
            }
        }

        private static DeviceMode GetDeviceMode()
        {
            string DeviceVersion = localSettings.Values["DeviceVersion"].ToString();
            if (DeviceVersion == "Desktop")
            {
                return DeviceMode.Desktop;
            }

            return DeviceMode.Mobile;
        }

        public static void ChangeMobileOSUserAgent(MobileOSMode mobileOSMode)
        {
            if(GetDeviceMode() != DeviceMode.Desktop)
            {
                if(mobileOSMode == MobileOSMode.iOS)
                {
                    UserAgent.SetUserAgent(iOSUserAgent);
                }
                else
                {
                    UserAgent.SetUserAgent(MobileUserAgent);
                }
            }
        }

        public static MobileOSMode GetMobileOSMode()
        {
            if(GetDeviceMode() == DeviceMode.Desktop)
            {
                return MobileOSMode.None;
            }
            else
            {
                if(UserAgent.GetUserAgent() == MobileUserAgent)
                {
                    //return MobileOSMode.WindowsPhone;
                    return MobileOSMode.iOS;
                }
                else
                {
                    return MobileOSMode.iOS;
                }
            }
        }

        public enum DeviceMode
        {
            Desktop = 0,
            Mobile = 1
        }

        public enum MobileOSMode
        {
            WindowsPhone = 0,
            iOS = 1,
            Android = 2,
            None = 3
        }
    }

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
    }
}
