using NuGet.Common;
using NuGet.Configuration;
using NuGet.PackageSourceMapper.Common;
using NuGet.Protocol.Core.Types;
using PackageSourceMapper.Logging;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;
using System.Threading.Tasks;

namespace NuGet.PackageSourceMapper
{
    internal static partial class GenerateCommandHandler
    {
        private const string NuGetOrgApi = "https://api.nuget.org/v3/index.json";
        private const string PACKAGES__WITHOUT__SOURCES = nameof(PACKAGES__WITHOUT__SOURCES);
        // Help with loading credentials from nuget.config file.
        internal static Dictionary<PackageSource, SourceRepository> _sourceRepositoryCache = new();
        private static Dictionary<string, PackageSource> _packageSourceObjectLookup = new();

        // This signature must be exactly same as Generate method, including var names, and Option names.
        delegate Task<int> GenerateDelegateAsync(string configPath, string verbosity, bool fullySpecified, bool reduceUnusedSources);

        private static async Task<int> GenerateAsync(string configPath, string verbosity, bool fullySpecified, bool reduceUnusedSources)
        {
            int ret = ReturnCode.Ok;
            Logger logger = new Logger();
            logger.VerbosityLevel = MSBuildVerbosityToNuGetLogLevel(verbosity);

            Console.WriteLine("================================================ Package Source Map Generator ===============================================");
            Console.WriteLine(LocalizedResourceManager.GetString("AppIntro"));

#if DEBUG
            Console.WriteLine("Parameters:");
            Console.WriteLine($"    configPath : {configPath}");
            Console.WriteLine($"    --verbosity : {verbosity}");
            Console.WriteLine($"    --fully-specified : {fullySpecified}");
            Console.WriteLine($"    --reduce-unused-sources : {reduceUnusedSources}");
            Console.WriteLine(string.Empty);
#else
            logger.LogVerbose("Parameters:");
            logger.LogVerbose($"    configPath : {configPath}");
            logger.LogVerbose($"    --verbosity : {verbosity}");
            logger.LogVerbose($"    --fully-specified : {fullySpecified}");
            logger.LogVerbose($"    --reduce-unused-sources : {reduceSources}");
            logger.LogVerbose(string.Empty);
#endif

            // Default GPF
            string defaultGpf = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages") + Path.DirectorySeparatorChar;
            string globalPackageFolder = defaultGpf;

            ISettings settings = null;
            var validInput = true;

            if (!string.IsNullOrEmpty(configPath))
            {
                string nugetConfigpath = configPath;

                if (File.Exists(nugetConfigpath) && nugetConfigpath.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
                {
                    settings = Settings.LoadSettingsGivenConfigPaths(new string[] { nugetConfigpath });

                    if (settings != null)
                    {
                        // Add the v3 global packages folder from nuget.config
                        globalPackageFolder = SettingsUtility.GetGlobalPackagesFolder(settings);
                        logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("NuGetConfigLoaded"),
                            nugetConfigpath));
                    }
                    else
                    {
                        logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("UnableLoadNuGetConfig"),
                            nugetConfigpath));
                        validInput = false;
                    }
                }
                else
                {
                    logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("InvalidNuGetConfigFilePath"),
                            nugetConfigpath));
                    validInput = false;
                }
            }
            else
            {
                validInput = false;
            }

            // to do: Take packages.config file instead of GPF folder location. Even possibly take both nuget.config + packages.config

            if (string.IsNullOrEmpty(globalPackageFolder) || !Directory.Exists(globalPackageFolder))
            {
                logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("GPFPath"),
                    globalPackageFolder));
                validInput = false;
            }

            if (!validInput)
            {
                logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("InputValidationError")));
                return ReturnCode.ArgumentError;
            }

            logger.LogMinimal(string.Format($"Global package folder path: {globalPackageFolder}"));

            if (globalPackageFolder == defaultGpf)
            {
                logger.LogMinimal(string.Empty);
                logger.LogMinimal(string.Format(LocalizedResourceManager.GetString("DefaultGPF"),
                    defaultGpf));
            }

            var request = new Request
            {
                GlobalPackagesFolder = globalPackageFolder,
                Settings = settings,
                IdPatternOnlyOption = fullySpecified,
                ReduceUnusedSourcesOption = reduceUnusedSources,
            };

            await ExecuteAsync(request, logger, _sourceRepositoryCache);

            return ret;
        }

        public static Command GenerateCommand()
        {
            var generateCommand = new Command(
                name: "generate",
                description: "This command generate package source mapping section for package source mapping feature from solution/project's nuget.config file or global packages folder. Please run this tool after NuGet package restore, it'll genereate nugetPackageSourceMapping.config file if successfull, also it detects when a NuGet package id is on more than one feeds and if there is any content discrepency between source and file on disc. For more info check https://devblogs.microsoft.com/nuget/introducing-package-source-mapping/")
            {
                Handler = HandlerDescriptor.FromDelegate((GenerateDelegateAsync)GenerateAsync).GetCommandHandler()
            };

            generateCommand.AddArgument(config());
            generateCommand.AddOption(Verbosity());
            generateCommand.AddOption(FullySpecifiedOption());
            generateCommand.AddOption(ReduceUnusedSourcesOption());
            return generateCommand;
        }

        private static Argument<string> config() =>
            new Argument<string>(
                "configpath",
                description: "The path to solution nuget.config file.");

        private static Option Verbosity() =>
            new Option(
                alias: "--verbosity",
                description: @"Verbosity of action log.")
            {
                Argument = new Argument<string>(name: "verbosity", getDefaultValue: () => string.Empty)
            };

        private static Option FullySpecifiedOption() =>
            new Option(
                aliases: new[] { "--fully-specified" },
                description: "Specify this option to generate full specified pattern instead without prefix.");

        private static Option ReduceUnusedSourcesOption() =>
            new Option(
                aliases: new[] { "--reduce-unused-sources" },
                description: "Specify this option if the packagesourcemapper should attempt to reduce the number of sources used in nuget.config by consolidating them");

        /// <summary>
        /// Note that the .NET CLI itself has parameter parsing which limits the values that will be passed here by the
        /// user. In other words, the default case should only be hit with <c>m</c> or <c>minimal</c> but we use <see cref="Common.LogLevel.Minimal"/>
        /// as the default case to avoid errors.
        /// </summary>
        public static LogLevel MSBuildVerbosityToNuGetLogLevel(string verbosity)
        {
            switch (verbosity?.ToUpperInvariant())
            {
                case "Q":
                case "QUIET":
                    return LogLevel.Warning;
                case "N":
                case "NORMAL":
                    return LogLevel.Information;
                case "D":
                case "DETAILED":
                case "DIAG":
                case "DIAGNOSTIC":
                    return LogLevel.Debug;
                default:
                    return LogLevel.Minimal;
            }
        }
    }
}
