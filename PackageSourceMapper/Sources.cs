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
using System.Threading;
using System.Threading.Tasks;

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

        private static async Task<ConcurrentDictionary<PackageIdentity, PackageSource>> ProbSourcesAsync(
            Request request,
            ConcurrentDictionary<string, List<PackageData>> sources,
            Dictionary<PackageSource, SourceRepository> _sourceRepositoryCache,
            ILogger logger)
        {
            List<PackageData> allPackages = sources.Values.SelectMany(s => s).Distinct().ToList();
            List<PackageSource> sourcesCanBeRemoved = new();
            IOrderedEnumerable<KeyValuePair<PackageSource, HashSet<PackageIdentity>>> sourcesDescendingByPackageCount = null;

            if (request.RemoveUnusedSourcesOption)
            {
                Dictionary<PackageSource, HashSet<PackageIdentity>> sourcesToPackage = new();

                logger.LogMinimal(Environment.NewLine + "    --remove-unused-sources option requires internet connection to sources used for restore!");
                SourceCacheContext cache = new SourceCacheContext();

                foreach (SourceRepository repository in _sourceRepositoryCache.Values)
                {
                    FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();
                    sourcesToPackage[repository.PackageSource] = new HashSet<PackageIdentity>();
                    try
                    {
                        logger.LogMinimal(Environment.NewLine + $"Started probing source:{repository.PackageSource} for package availability");
                        foreach (PackageData packageData in allPackages)
                        {
                            bool exists = await resource.DoesPackageExistAsync(
                                packageData.PackageIdentity.Id,
                                packageData.PackageIdentity.Version,
                                cache,
                                logger,
                                CancellationToken.None);

                            if (exists)
                            {
                                logger.LogMinimal($"     {packageData.PackageIdentity} is found in this source.");
                                sourcesToPackage[repository.PackageSource].Add(packageData.PackageIdentity);
                            }
                            else
                            {
                                logger.LogMinimal($"     {packageData.PackageIdentity} is not found in this source.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Experienced problem with {repository.PackageSource}: {ex.Message}");
                    }
                }

                sourcesDescendingByPackageCount = from entry in sourcesToPackage orderby entry.Value.Count descending select entry;

                List<PackageIdentity> allPackageIdentities = allPackages.Select(x => x.PackageIdentity).ToList();
                // Simple greedy algorithm, it can be improved later, brute force solution which covers all possible scenario would be O(n!*m) time complexity, here n is number of sources and m is number of packages.
                // With simple greedy algorithm is O(n*m).
                int lastCount = allPackageIdentities.Count;
                foreach (KeyValuePair<PackageSource, HashSet<PackageIdentity>> sourcePackages in sourcesDescendingByPackageCount)
                {
                    if (allPackageIdentities.Count == 0)
                    {   // If all packages are already covered then we can remove this source.
                        sourcesCanBeRemoved.Add(sourcePackages.Key);
                        continue;
                    }

                    foreach (PackageIdentity packageIdentity in sourcePackages.Value)
                    {
                        allPackageIdentities.Remove(packageIdentity);
                    }

                    if (lastCount == allPackageIdentities.Count)
                    {
                        // Nothing removed with this source
                        sourcesCanBeRemoved.Add(sourcePackages.Key);
                    }
                    lastCount = allPackageIdentities.Count;
                }

                if (sourcesCanBeRemoved.Count > 0)
                {
                    logger.LogMinimal(Environment.NewLine + $"The following sources can be removed:because the packages in them are already covered by other sources.");

                    foreach (PackageSource packageSource in sourcesCanBeRemoved)
                    {
                        logger.LogMinimal($"   - {packageSource}");
                    }
                }
                else
                {
                    logger.LogMinimal(Environment.NewLine + $"The greedy algorithm was unable to find any sources that could be removed, so manual removal may be necessary.");
                }
            }

            ConcurrentDictionary<PackageIdentity, PackageSource> packageSourceLookup = new();

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

            if (sourcesCanBeRemoved.Count > 0 && sourcesDescendingByPackageCount != null)
            {
                foreach (PackageIdentity package in packageSourceLookup.Keys)
                {
                    PackageSource packageSource = packageSourceLookup[package];

                    if (sourcesCanBeRemoved.Contains(packageSource))
                    {
                        // Reassign new source to packages from source with most packages
                        foreach (KeyValuePair<PackageSource, HashSet<PackageIdentity>> sourcePackages in sourcesDescendingByPackageCount)
                        {
                            if (sourcePackages.Value.Contains(package))
                            {
                                packageSourceLookup[package] = sourcePackages.Key;
                                break;
                            }
                        }
                    }
                }
            }

            return packageSourceLookup;
        }
    }
}
