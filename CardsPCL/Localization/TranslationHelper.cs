using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace CardsPCL.Localization
{
    public static class TranslationHelper
    {
        public static string GetString(string key, CultureInfo ci)
        {
            ResourceManager temp = new ResourceManager(
                "CardsPCL.Localization.Resources.Resources",
                typeof(Resources.Resources).GetTypeInfo().Assembly);
            string result = temp.GetString(key, ci);
            return result;
        }
    }
}
