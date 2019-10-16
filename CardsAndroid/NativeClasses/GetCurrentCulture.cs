using System;
namespace CardsAndroid.NativeClasses
{
    public class GetCurrentCulture
    {
        public static System.Globalization.CultureInfo GetCurrentCultureInfo()
        {
            var androidLocale = Java.Util.Locale.Default;
            var netLanguage = androidLocale.ToString().Replace("_", "-");

            // DELETE THIS WHILE LOCALIZATION!!!!!!!!!!!!!!!!!!!!!
            netLanguage = "ru-RU";

            return new System.Globalization.CultureInfo(netLanguage);
        }
    }
}
