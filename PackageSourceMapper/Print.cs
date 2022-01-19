using NuGet.Common;
using NuGet.Packaging.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuGet.PackageSourceMapper
{
    internal static partial class GenerateCommandHandler
    {
        private static void PrintUndefinedSources(ConcurrentDictionary<string, List<PackageData>> sources, HashSet<string> undefinedSources, ILogger logger)
        {
            if (undefinedSources.Count > 0)
            {
                var undefinedSourcesLog = new StringBuilder();

                foreach (string undefinedSource in undefinedSources)
                {
                    if (undefinedSource == NuGetOrgApi || (undefinedSource == PACKAGES__WITHOUT__SOURCES))
                    {
                        continue;
                    }
                    else
                    {
                        if (sources.ContainsKey(undefinedSource))
                        {
                            undefinedSourcesLog.AppendLine($"       Source : {undefinedSource} with {sources[undefinedSource].Distinct().Count()} packages.");

                            foreach (PackageData packageContent in sources[undefinedSource].Distinct().OrderBy(p => p.PackageIdentity.Id))
                            {
                                undefinedSourcesLog.AppendLine($"            - {packageContent.PackageIdentity.Id} {packageContent.PackageIdentity.Version}");
                            }
                        }
                        else
                        {
                            undefinedSourcesLog.AppendLine($"       Source : {undefinedSource} with unknown number of packages.");
                            continue;
                        }

                    }
                }

                if (undefinedSources.Contains(PACKAGES__WITHOUT__SOURCES) && sources.ContainsKey(PACKAGES__WITHOUT__SOURCES))
                {
                    undefinedSourcesLog.AppendLine($"       Source : Packages don't have source meta data: {sources[PACKAGES__WITHOUT__SOURCES].Distinct().Count()} packages.");

                    foreach (PackageData packageContent in sources[PACKAGES__WITHOUT__SOURCES].Distinct().OrderBy(p => p.PackageIdentity.Id))
                    {
                        undefinedSourcesLog.AppendLine($"            - {packageContent.PackageIdentity.Id} {packageContent.PackageIdentity.Version}");
                    }
                }

                if (undefinedSourcesLog.Length > 0)
                {
                    logger.LogMinimal($"{Environment.NewLine}   Following sources detected from packages sources but not found in nuget.config:");
                    logger.LogMinimal(undefinedSourcesLog.ToString());
                }
            }
        }

        private static void PrintStatistics(ConcurrentDictionary<string, List<PackageData>> sources, ILogger logger)
        {
            List<string> uniquePackages = sources.Values.SelectMany(s => s).Select(s => s.PackageIdentity.Id).Distinct().OrderBy(s => s).ToList();
            List<PackageData> uniquePackageVersions = sources.Values.SelectMany(s => s).Distinct().OrderBy(s => s.PackageIdentity).ToList();

            logger.LogMinimal($"    Total source count: {sources.Count}, Unique packages {uniquePackages.Count}, PackageVersion count: {uniquePackageVersions.Count}");

            foreach (KeyValuePair<string, List<PackageData>> source in sources.OrderBy(s => s.Key))
            {
                List<PackageIdentity> sourceUniquePackages = source.Value.Select(s => s.PackageIdentity).Distinct().ToList();

                logger.LogMinimal($"        Source : {source.Key}, Unique packages {sourceUniquePackages.Count}, PackageVersion count: {source.Value.Count}");

                foreach (PackageIdentity package in sourceUniquePackages.OrderBy(p => p.Id))
                {
                    logger.LogVerbose($"           - {package}");
                }

                logger.LogMinimal(string.Empty);
            }

            logger.LogMinimal(string.Empty);
        }
    }
}
