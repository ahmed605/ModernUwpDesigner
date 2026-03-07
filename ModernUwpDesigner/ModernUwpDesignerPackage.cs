using Microsoft.VisualStudio;
using Microsoft.VisualStudio.DesignTools.DesignerHost.Platform;
using Microsoft.VisualStudio.DesignTools.Utility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ModernUwpDesigner.Shared;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace ModernUwpDesigner
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = false, AllowsBackgroundLoading = true)]
    [Guid(ModernUwpDesignerPackage.PackageGuidString)]
    public sealed class ModernUwpDesignerPackage : AsyncPackage
    {
        /// <summary>
        /// ModernUwpDesignerPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "c87560ed-02e7-4c00-8dc6-b1ac79534ce4";

        private static PlatformSpecification ModernUwpSpecification = new PlatformSpecification("Windows", "10.0-..", new string[] { "Managed", "Native" }, ".NETCoreApp", "9.0-..", null, "UAP", null);
        //private delegate void RegisterPlatformConfigurationDelegate(PlatformSpecification platformSpecification, IDictionary<string, string> platformProperties);

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            //AssemblyRedirectModuleInitializer.Init();

			await base.InitializeAsync(cancellationToken, progress);
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            unsafe
            {

                var RegisterPlatformConfiguration = (delegate*<PlatformSpecification, IDictionary<string, string>, void>)typeof(PlatformConfigurationService).GetMethod("RegisterPlatformConfiguration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).MethodHandle.GetFunctionPointer();
                RegisterPlatformConfiguration(ModernUwpSpecification, new Dictionary<string, string>
                {
                    { "PlatformCreatorAssembly", "Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner" },
                    { "PlatformCreatorType", "Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner.UwpPlatformCreator" },
                    //{ "HostPlatformAssembly", "ModernUwpDesigner.UwpDesignerHost.dll" },
                    { "HostPlatformAssembly", typeof(Microsoft.VisualStudio.DesignTools.UwpDesignerHost.UwpHostPlatform).Assembly.Location },
                    { "HostPlatformType", "Microsoft.VisualStudio.DesignTools.UwpDesignerHost.UwpHostPlatform" },
                    { "IsolationUnification", "true" },
                    { "ReferenceAssemblyMode", "None" },
                    { "DefaultTargetFramework", ".NETCore, Version=9.0" },
                    { "UserControlTemplateName", "MyUserControl.xaml" },
                    { "PlatformSurfaceIsolatedGuid", "{D617FC9B-7AE9-4219-B022-359A3D13B875}" },
                    { "SupportsToolboxAutoPopulation", "true" },
                    { "SupportsExtensionSdks", "true" },
                    { "LegacyExtensionSdkPlatformsAndRequiredVCLibs", "Windows, 10.0;Microsoft.VCLibs.120, Version=14.0" },
                    { "ToolboxPage", "{8A63BDE2-AEB9-4AF9-A00D-DBC9BD7D509C}" },
                    { "DesignerTechnology", "Microsoft:Windows.UI.Xaml" },
                    { "ClipboardFormat", "CF_WINDOWSUIXAML_TOOL" },
                    { "AppPackageType", "WindowsXaml" }
                });
            }
        }
    }
}
