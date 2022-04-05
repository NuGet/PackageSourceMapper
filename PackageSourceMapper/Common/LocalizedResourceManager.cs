using System.Globalization;
using System.Resources;
using System.Threading;

namespace NuGet.PackageSourceMapper.Common
{
    internal static class LocalizedResourceManager
    {
        private static readonly ResourceManager _resourceManager = new ResourceManager("NuGet.PackageSourceMapper.Properties.Resources", typeof(LocalizedResourceManager).Assembly);

        public static string GetString(string resourceName)
        {
            var culture = GetLanguageName();
            return _resourceManager.GetString(resourceName + '_' + culture, CultureInfo.InvariantCulture) ??
                   _resourceManager.GetString(resourceName, CultureInfo.InvariantCulture);
        }
        public static string GetLanguageName()
        {
            var culture = Thread.CurrentThread.CurrentUICulture;
            while (!culture.IsNeutralCulture)
            {
                if (culture.Parent == culture)
                {
                    break;
                }

                culture = culture.Parent;
            }

            return culture.ThreeLetterWindowsLanguageName.ToLowerInvariant();
        }
    }
}
