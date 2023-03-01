# NuGet Package Source Mapper tool

## Description

This tool helps onboarding to [package source mapping](https://devblogs.microsoft.com/nuget/introducing-package-source-mapping) feature.
It can generate package source mapping section for you from nuget.config file and restored `global packages folder`.
Here is steps to use the tool. It works for both packagereference and packages.config type projects. Please note tool doesn't map packages in fallback folder since they don't get copied to `global packages folder`.

1. Declare a new [global packages folder for your solution](https://docs.microsoft.com/en-us/nuget/reference/nuget-config-file#config-section) in nuget.config file.

```xml
<config>
  <add key="globalPackagesFolder" value="globalPackages" />
</config>
```

1. Preparation

   * Do solution restore
   * If you have any restore/build script then please run before running this tool. It applies to any test or any other sub solutions. If you happen to have packages restored in different folder due to sub project settings then please copy them to above `global packages folder`.

1. Run this tool with options suitable for you. See examples below.

1. Copy generated `nugetPackageSourceMapping.config` file content into your nuget.config file. Please make any adjustments most sutiable for your use case.

1. Clear all local cache one more time to start on clean slate `dotnet nuget locals all --clear`

1. Repeat restore step above and make sure everything still works.

## Synopsis:

```dotnetcli
packagesourcemapper generate <CONFIGPATH> [-h|--help] [--verbosity <LEVEL>] [--id-pattern-only]
```

### Commands

If no command is specified, the command will default to `help`.

#### `generate`
Generates packageSourceMapping section for nuget.config file.

### Arguments:

#### `CONFIGPATH`

Specify path to `nuget.config` used for packagesourcemapper. This is positional argument so just value after `generate` command.

### Options:

#### `-h|--help`

Show help information

#### `--verbosity <LEVEL>`

  Sets the verbosity level of the command. Allowed values are `q[uiet]`, `m[inimal]`, `n[ormal]`, `d[etailed]`, and `diag[nostic]`. The default is `minimal`. For more information, see [LoggerVerbosity](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.framework.loggerverbosity?view=msbuild-16-netcore).

#### `--id-pattern-only`

Specify this option to generate full specified pattern instead without prefix. Currently only packages starting with `Microsoft, System, Runtime, Xunit` are prefixed by default.

#### `--remove-unused-sources`

Specify this option if the packagesourcemapper should attempt to reduce the number of sources used in nuget.config by consolidating them.

### Examples

Generate packageSourceMapping section:

`PackageSourceMapper.exe generate C:\NuGetProj\NuGet.Client\NuGet.Config`

`PackageSourceMapper.exe generate C:\NuGetProj\NuGet.Client\NuGet.Config --verbosity diag`

Generate packageSourceMapping section without any prefixing:

`PackageSourceMapper.exe generate C:\NuGetProj\NuGet.Client\NuGet.Config --verbosity m --id-pattern-only`

## Feedback

File NuGet.Client bugs in the [NuGet/PackageSourceMapper](https://github.com/NuGet/PackageSourceMapper/issues)
