using NuGet.Common;
using NuGet.Configuration;
using NuGet.PackageSourceMapper.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuGet.PackageSourceMapper
{
    internal static partial class GenerateCommandHandler
    {
        private static async Task ExecuteAsync(Request request, ILogger logger, Dictionary<PackageSource, SourceRepository> _sourceRepositoryCache)
        {
            Dictionary<string, PackageSource> definedSourcesDict = null;
            HashSet<string> undefinedSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ISettings settings = request.Settings;

            if (settings != null)
            {
                var packageSourceProvider = new PackageSourceProvider(settings);
                List<PackageSource> definedSources = packageSourceProvider.LoadPackageSources().ToList();
                definedSourcesDict = new Dictionary<string, PackageSource>(StringComparer.OrdinalIgnoreCase);
                foreach (PackageSource packageSource in definedSources)
                {
                    definedSourcesDict[packageSource.Source.ToString()] = packageSource;
                }

                PopulateSourceRepositoryCache(settings);
            }

            string[] metadataFiles = Directory.GetFileSystemEntries(request.GlobalPackagesFolder, PackagingCoreConstants.NupkgMetadataFileExtension, SearchOption.AllDirectories);

            if (metadataFiles.Length == 0)
            {
                throw new InvalidOperationException(LocalizedResourceManager.GetString("GPFEmpty"));
            }

            IEnumerable<Lazy<INuGetResourceProvider>> providers = Repository.Provider.GetCoreV3();
            ConcurrentDictionary<string, List<PackageData>> sources = new();

            logger.LogMinimal(string.Empty);
            logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("DetectPackageSources")));

            foreach (string metadataPath in metadataFiles)
            {
                try
                {
                    NupkgMetadataFile metadata = NupkgMetadataFileFormat.Read(metadataPath);
                    var pathParts = metadataPath.Split(Path.DirectorySeparatorChar);
                    string packageId = pathParts[pathParts.Length - 3];
                    var packageVersion = Versioning.NuGetVersion.Parse(pathParts[pathParts.Length - 2]);
                    string nuspecPath = Path.Combine(Path.GetDirectoryName(metadataPath), $"{packageId}.nuspec");
                    packageId = GetPackageIdWithOriginalCasing(nuspecPath, logger) ?? packageId;
                    PackageIdentity packageIdentity = new PackageIdentity(packageId, packageVersion);

                    var sourcePath = metadata.Source;
                    if (string.IsNullOrWhiteSpace(sourcePath))
                    {
                        sourcePath = PACKAGES__WITHOUT__SOURCES;
                    }

                    if (!sources.ContainsKey(sourcePath))
                    {
                        sources[sourcePath] = new List<PackageData>();
                    }

                    if (!_packageSourceObjectLookup.ContainsKey(sourcePath))
                    {
                        bool isFound = false;
                        foreach (PackageSource packagesource in _sourceRepositoryCache.Keys)
                        {
                            if (packagesource.Source.Equals(sourcePath, StringComparison.OrdinalIgnoreCase))
                            {
                                _packageSourceObjectLookup[sourcePath] = packagesource;
                                isFound = true;
                                break;
                            }
                        }

                        if (!isFound)
                        {
                            if (!definedSourcesDict.TryGetValue(sourcePath, out PackageSource packageSource))
                            {
                                if (sourcePath == PACKAGES__WITHOUT__SOURCES)
                                {
                                    logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("PackagesWithoutSources"),
                                        PACKAGES__WITHOUT__SOURCES));
                                }

                                packageSource = new PackageSource(sourcePath);
                            }

                            _packageSourceObjectLookup[sourcePath] = packageSource;
                            _sourceRepositoryCache.Add(packageSource, new SourceRepository(packageSource, providers));
                            undefinedSources.Add(sourcePath);
                        }
                    }

                    // todo: do I need to calculate actual SHA512 hash from nupkg file just in case?
                    sources[sourcePath].Add(new PackageData(packageIdentity, metadata.ContentHash, null, sourcePath));
                }
                catch (Exception ex)
                {
                    logger.LogError("    " + ex.Message);
                }
            }

            PrintUndefinedSources(sources, undefinedSources, logger);

            PrintStatistics(sources, logger);

            // Probe sources for package availability and update them.
            ConcurrentDictionary<PackageIdentity, PackageSource> packageSourceLookup = await ProbSourcesAsync(request, sources, _sourceRepositoryCache, logger);

            GeneratePackageSourceMappingSection(request, packageSourceLookup, logger);

            Console.WriteLine(string.Format(LocalizedResourceManager.GetString("FinishGeneration")));
            Console.WriteLine(string.Empty);
            logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("VerifyResult")));
        }

        private static string GetPackageIdWithOriginalCasing(string nuspecPath, ILogger logger)
        {
            if (string.IsNullOrEmpty(nuspecPath) || !File.Exists(nuspecPath))
            {
                return null;
            }

            try
            {
                NuspecReader nuspecReader = new NuspecReader(nuspecPath);
                return nuspecReader.GetId();
            }
            catch (Exception ex)
            {
                logger.LogError("    " + ex.Message);
                return null;
            }
        }
    }
}
