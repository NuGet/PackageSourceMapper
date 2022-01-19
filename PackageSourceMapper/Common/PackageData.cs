using NuGet.Packaging.Core;

namespace NuGet.PackageSourceMapper
{
    internal static partial class GenerateCommandHandler
    {
        private class PackageData
        {
            public PackageIdentity PackageIdentity { get; set; }
            public string PackageContentHash { get; set; }
            public string PackageRemoteHash { get; set; }
            public string OriginalSource { get; set; }

            private PackageData()
            {
            }

            public PackageData(PackageIdentity packageIdentity, string packageContentHash, string packageRemoteHash, string originalSource)
            {
                PackageIdentity = packageIdentity;
                PackageContentHash = packageContentHash;
                PackageRemoteHash = packageRemoteHash;
                OriginalSource = originalSource;
            }
        }
    }
}
