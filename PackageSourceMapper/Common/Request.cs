using NuGet.Configuration;

namespace NuGet.PackageSourceMapper
{
    internal class Request
    {
        public string GlobalPackagesFolder { get; set; }
        public ISettings Settings { get; set; }
        public bool IdPatternOnlyOption { get; set; }
    }
}
