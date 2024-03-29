﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NuGet.PackageSourceMapper.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NuGet.PackageSourceMapper.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This app creates nugetPackageSourceMapping.config section for Package Source Mapping and you can copy into your nuget.config file, for more info check https://devblogs.microsoft.com/nuget/introducing-package-source-mapping/.
        /// </summary>
        internal static string AppIntro {
            get {
                return ResourceManager.GetString("AppIntro", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Checking packages from sources.
        /// </summary>
        internal static string CheckPackageFromSources {
            get {
                return ResourceManager.GetString("CheckPackageFromSources", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to * Warning: Default global packages folder detected: {0}
        ///                For best result please use non default GPF folder. You may override it in your nuget.config file, then do restore again to populate new GPF.
        ///                    &lt;config&gt;
        ///                      &lt;add key=&quot;&quot;globalPackagesFolder&quot;&quot; value=&quot;&quot;globalPackagesFolder&quot;&quot; /&gt;
        ///                    &lt;/config&gt;.
        /// </summary>
        internal static string DefaultGPF {
            get {
                return ResourceManager.GetString("DefaultGPF", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Detect package sources from package meta data:.
        /// </summary>
        internal static string DetectPackageSources {
            get {
                return ResourceManager.GetString("DetectPackageSources", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Finished creating recommendation nugetPackageSourceMapping.config file. Please review the content then copy it to your nuget.config file..
        /// </summary>
        internal static string FinishGeneration {
            get {
                return ResourceManager.GetString("FinishGeneration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Either V3 package folder is empty, if not restored yet then please try to restore first..
        /// </summary>
        internal static string GPFEmpty {
            get {
                return ResourceManager.GetString("GPFEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Global package folder path is invalid or it doesn&apos;t exist,if not restored yet then please try to restore first. {0}.
        /// </summary>
        internal static string GPFPath {
            get {
                return ResourceManager.GetString("GPFPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to continue due to input validation error..
        /// </summary>
        internal static string InputValidationError {
            get {
                return ResourceManager.GetString("InputValidationError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: File is not found or without .config extension: {0}.
        /// </summary>
        internal static string InvalidNuGetConfigFilePath {
            get {
                return ResourceManager.GetString("InvalidNuGetConfigFilePath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Nuget.config file successfully loaded: {0}.
        /// </summary>
        internal static string NuGetConfigLoaded {
            get {
                return ResourceManager.GetString("NuGetConfigLoaded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NU7001 : Package {0} not found on source {1} it was downloaded from previously..
        /// </summary>
        internal static string PackageNotFoundOnSourceDownloaded {
            get {
                return ResourceManager.GetString("PackageNotFoundOnSourceDownloaded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to     - {0} packageSource key matching {1} packages created..
        /// </summary>
        internal static string PackageSourceKeyCreated {
            get {
                return ResourceManager.GetString("PackageSourceKeyCreated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to     - {0} package source didn&apos;t match any packages..
        /// </summary>
        internal static string PackageSourceWithNoPackage {
            get {
                return ResourceManager.GetString("PackageSourceWithNoPackage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to - Package without any source meta data detected. For those packages source denoted as {0}.
        /// </summary>
        internal static string PackagesWithoutSources {
            get {
                return ResourceManager.GetString("PackagesWithoutSources", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to     - Processing {0} ....
        /// </summary>
        internal static string ProcessingSource {
            get {
                return ResourceManager.GetString("ProcessingSource", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Following patterns have source collision:
        ///.
        /// </summary>
        internal static string SourcesWithCollision {
            get {
                return ResourceManager.GetString("SourcesWithCollision", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Start creating recommendation nugetPackageSourceMapping.config file..
        /// </summary>
        internal static string StartCreatingRecommendation {
            get {
                return ResourceManager.GetString("StartCreatingRecommendation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Unable to load nuget.config file. {0}.
        /// </summary>
        internal static string UnableLoadNuGetConfig {
            get {
                return ResourceManager.GetString("UnableLoadNuGetConfig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Not all packages resolved, see above for details..
        /// </summary>
        internal static string UnresolvedPackages {
            get {
                return ResourceManager.GetString("UnresolvedPackages", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to *** Error: Unresolved packages detected..
        /// </summary>
        internal static string UnresolvedPackagesDetected {
            get {
                return ResourceManager.GetString("UnresolvedPackagesDetected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To verify new configuration works.
        ///         -  Clear local GPF &apos;dotnet nuget locals all --clear&apos;
        ///         -  VS &gt;&gt; Build &gt;&gt; Clean Solution (remove bin/obj file/folders)
        ///         -  Close VS and reopen VS to new nuget.config to take effect.
        ///         -  Restore and build with new settings.
        ///         -  Make sure there is no error or warnings.
        ///         -  Re-run the tool with new config and --verbosity detailed to verify if packages are coming from desired sources..
        /// </summary>
        internal static string VerifyResult {
            get {
                return ResourceManager.GetString("VerifyResult", resourceCulture);
            }
        }
    }
}
