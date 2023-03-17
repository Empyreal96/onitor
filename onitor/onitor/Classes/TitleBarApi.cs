using Windows.Foundation.Metadata;
using Windows.Graphics.Display;

namespace Onitor
{
    public static class TitleBarApi
    {
        public static string UserFriendlyTitle(string title)
        {
            DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
            if ((ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 2) && displayInformation.DiagonalSizeInInches < 5.0)
                || displayInformation.ResolutionScale > ResolutionScale.Scale400Percent)
            {
                if (title.Length < 11)
                {
                    return title;
                }
                else
                {
                //    return string.Format("{0} ... {1}", title.Substring(0, 6), title.Substring(title.Length - 3));
                    return string.Format(title);

                }
            }
            else
            {
                if (title.Length < 18)
                {
                    return title;
                }
                else
                {
                    //return string.Format("{0} ... {1}", title.Substring(0, 13), title.Substring(title.Length - 3));
                    return string.Format(title);

                }
            }
        }

    }
}
