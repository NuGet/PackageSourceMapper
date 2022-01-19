using NuGet.Common;
using NuGet.Configuration;
using NuGet.PackageSourceMapper.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.PackageSourceMapper
{
    internal static partial class GenerateCommandHandler
    {
        /// <summary>
        /// Pre-populate _sourceRepositoryCache so source repository can be reused between different calls.
        /// </summary>
        /// <param name="settings">Settings file</param>
        internal static void PopulateSourceRepositoryCache(ISettings settings)
        {
            IEnumerable<PackageSource> packageSources = PackageSourceProvider.LoadPackageSources(settings);
            IEnumerable<Lazy<INuGetResourceProvider>> providers = Repository.Provider.GetCoreV3();
            foreach (PackageSource packageSource in packageSources)
            {
                SourceRepository sourceRepository = Repository.CreateSource(providers, packageSource, FeedType.Undefined);
                _sourceRepositoryCache[packageSource] = sourceRepository;
            }
        }

        private static List<(string, string)> GetDefinedSourceOrdering(ISettings settings)
        {
            List<(string, string)> orderedPackageSources = new();
            var packageSourcesSection = settings.GetSection(ConfigurationConstants.PackageSources);
            var sourcesItems = packageSourcesSection?.Items.OfType<SourceItem>();

            foreach (SourceItem sourceItem in sourcesItems)
            {
                orderedPackageSources.Add((sourceItem.Key, sourceItem.Value));
            }

            return orderedPackageSources;
        }

        private static ConcurrentDictionary<PackageIdentity, PackageSource> ProbSources(Request request, ConcurrentDictionary<string, List<PackageData>> sources, ILogger logger)
        {
            List<PackageData> allPackages = sources.Values.SelectMany(s => s).Distinct().ToList();
            ConcurrentDictionary<PackageIdentity, PackageSource> packageSourceLookup = new();

            // Prob sources for availability of packages and save hashContent from sources so we can compare later.
            packageSourceLookup = new ConcurrentDictionary<PackageIdentity, PackageSource>(allPackages.ToDictionary(p => p.PackageIdentity, p => _packageSourceObjectLookup[p.OriginalSource]));

            if (packageSourceLookup.Keys.Count != allPackages.Select(p => p.PackageIdentity).Count())
            {
                logger.LogMinimal(LocalizedResourceManager.GetString("UnresolvedPackagesDetected"));
                var unresolvedPackages = allPackages.Select(p => p.PackageIdentity).Where(p => !packageSourceLookup.Keys.Any(l => p.Id == l.Id && p.Version == l.Version));

                foreach (var unresolvedPackage in unresolvedPackages)
                {
                    logger.LogMinimal("   - " + unresolvedPackage);
                }

                logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("UnresolvedPackages")));
                Environment.Exit(1);
            }

            return packageSourceLookup;
        }
    }
}
