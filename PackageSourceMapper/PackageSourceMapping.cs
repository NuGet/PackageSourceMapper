using NuGet.Common;
using NuGet.Configuration;
using NuGet.PackageSourceMapper.Common;
using NuGet.Packaging.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NuGet.PackageSourceMapper
{
    internal static partial class GenerateCommandHandler
    {
        private static void GeneratePackageSourceMappingSection(Request request, ConcurrentDictionary<PackageIdentity, PackageSource> packageSourceLookup, ILogger logger)
        {
            logger.LogMinimal(string.Empty);
            logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("StartCreatingRecommendation")));
            ConcurrentDictionary<string, List<PackageIdentity>> sources = new();

            foreach (PackageIdentity packageIdentity in packageSourceLookup.Keys)
            {
                PackageSource packageSource = packageSourceLookup[packageIdentity];
                if (!sources.ContainsKey(packageSource.Source))
                {
                    sources[packageSource.Source] = new List<PackageIdentity>();
                }

                sources[packageSource.Source].Add(packageIdentity);
            }

            // Generate recommended packageSourceMapping section.
            var packageSourceMapping = new StringBuilder("  <clear />" + Environment.NewLine);
            Dictionary<string, List<string>> patternSourceLookup = new();

            // Do we need to keep source ordering? Currently just alphabeticly ordered.
            foreach ((string sourceKey, string sourceUri) in GetDefinedSourceOrdering(request.Settings))
            {
                if (!sources.ContainsKey(sourceUri))
                {
                    logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("PackageSourceWithNoPackage"), sourceKey));
                    continue;
                }

                packageSourceMapping.AppendLine($"  <packageSource key = \"{ sourceKey}\">");

                List<string> uniquePackageIdsInSource = sources[sourceUri].Select(p => p.Id).Distinct().OrderBy(p => p).ToList();
                string currentPrefix = null;
                foreach (string packageId in uniquePackageIdsInSource)
                {
                    if (request.IdPatternOnlyOption)
                    {
                        packageSourceMapping.AppendLine($"    <package pattern=\"{packageId}\" />");
                        TrackSources(packageId, patternSourceLookup, sourceKey);
                    }
                    else
                    {
                        string packagePattern = GetPackagePattern(packageId, ref currentPrefix);

                        if (!string.IsNullOrEmpty(packagePattern))
                        {
                            packageSourceMapping.AppendLine(packagePattern);
                            TrackSources(packagePattern, patternSourceLookup, sourceKey);
                        }
                    }
                }

                packageSourceMapping.AppendLine("  </packageSource>");

                List<PackageIdentity> uniquePackagesInSource = sources[sourceUri].Distinct().OrderBy(p => p.Id).ThenBy(p => p.Version).ToList();
                logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("PackageSourceKeyCreated"), sourceKey, uniquePackagesInSource.Count));

                foreach (PackageIdentity packageIdentity in uniquePackagesInSource.OrderBy(p => p))
                {
                    logger.LogVerbose($"        - {packageIdentity}");
                }

                logger.LogVerbose(string.Empty);
            }

            var packageSourceCollision = new StringBuilder(LocalizedResourceManager.GetString("SourcesWithCollision"));

            foreach (KeyValuePair<string, List<string>> patternSource in patternSourceLookup)
            {
                if (patternSource.Value.Count > 1)
                {
                    packageSourceCollision.AppendLine($"        - {patternSource.Key} mapped to {patternSource.Value.Count} source: {string.Join(", ", patternSource.Value)}");
                }
            }

            if (packageSourceCollision.Length > 45)
            {
                logger.LogMinimal(packageSourceCollision.ToString());
            }

            var configurationFileContent = $@"
<packageSourceMapping>
{packageSourceMapping.ToString().TrimEnd()}
</packageSourceMapping>
<disabledPackageSources>
    <clear />
</disabledPackageSources>";

            File.WriteAllText("nugetPackageSourceMapping.config", configurationFileContent.TrimStart());
        }

        private static string GetPackagePattern(string packageId, ref string currentPrefix)
        {
            var pattern = string.Empty;

            if (packageId.StartsWith("microsoft.", StringComparison.OrdinalIgnoreCase))
            {
                if (currentPrefix != "microsoft.")
                {
                    currentPrefix = "microsoft.";
                    pattern = $"    <package pattern=\"{currentPrefix}*\" />";
                }
            }
            else if (packageId.StartsWith("system.", StringComparison.OrdinalIgnoreCase))
            {
                if (currentPrefix != "system.")
                {
                    currentPrefix = "system.";
                    pattern = $"    <package pattern=\"{currentPrefix}*\" />";
                }
            }
            else if (packageId.StartsWith("runtime.", StringComparison.OrdinalIgnoreCase))
            {
                if (currentPrefix != "runtime.")
                {
                    currentPrefix = "runtime.";
                    pattern = $"    <package pattern=\"{currentPrefix}*\" />";
                }
            }
            else if (packageId.StartsWith("xunit.", StringComparison.OrdinalIgnoreCase))
            {
                if (currentPrefix != "xunit.")
                {
                    currentPrefix = "xunit.";
                    pattern = $"    <package pattern=\"{currentPrefix}*\" />";
                }
            }
            else
            {
                pattern = $"    <package pattern=\"{packageId}\" />";
            }

            return pattern;
        }

        private static void TrackSources(string pattern, Dictionary<string, List<string>> patternDictionary, string sourceKey)
        {
            if (!patternDictionary.ContainsKey(pattern))
            {
                patternDictionary.Add(pattern, new List<string>());
            }

            patternDictionary[pattern].Add(sourceKey);
        }
    }
}
