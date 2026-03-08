using Microsoft.VisualStudio;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.DesignerHost.HostServices;
using Microsoft.VisualStudio.DesignTools.DesignerHost.Platform;
using Microsoft.VisualStudio.DesignTools.SurfaceDesigner.Documents.Project;
using Microsoft.VisualStudio.DesignTools.SurfaceDesigner.Views;
using Microsoft.VisualStudio.DesignTools.Utility;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost;
using Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner.Documents;
using Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner.Views;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
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

        private static Hook _updateRuntimeArchitectureHook;
        private static Hook _incompatibleDesignerRuntimeArchitectureHook;

        private static readonly PlatformSpecification ModernUwpSpecification = new("Windows", "10.0-..", ["Managed", "Native"], ".NETCoreApp", "10.0-..", null, "UAP", null);

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            unsafe
            {

                var RegisterPlatformConfiguration = (delegate*<PlatformSpecification, IDictionary<string, string>, void>)typeof(PlatformConfigurationService).GetMethod("RegisterPlatformConfiguration", BindingFlags.NonPublic | BindingFlags.Static).MethodHandle.GetFunctionPointer();
                RegisterPlatformConfiguration(ModernUwpSpecification, new Dictionary<string, string>
                {
                    { "PlatformCreatorAssembly", "Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner" },
                    { "PlatformCreatorType", "Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner.UwpPlatformCreator" },
                    { "HostPlatformAssembly", typeof(UwpHostPlatform).Assembly.Location },
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

            if (RuntimeInformation.ProcessArchitecture is Architecture.Arm64 &&
                _updateRuntimeArchitectureHook is null &&
                _incompatibleDesignerRuntimeArchitectureHook is null)
            {
                var property = typeof(UwpSceneView).GetProperty("IncompatibleDesignerRuntimeArchitecture", BindingFlags.NonPublic | BindingFlags.Instance);
                _incompatibleDesignerRuntimeArchitectureHook = new Hook(property.GetMethod, IncompatibleDesignerRuntimeArchitectureHook);

                var method = typeof(HostPlatformBase).GetMethod("UpdateRuntimeArchitecture", BindingFlags.NonPublic | BindingFlags.Instance);
                _updateRuntimeArchitectureHook = new Hook(method, UpdateRuntimeArchitectureHook);
            }
        }

        private delegate bool IncompatibleDesignerRuntimeArchitecture(UwpSceneView instance);

        private bool IncompatibleDesignerRuntimeArchitectureHook(IncompatibleDesignerRuntimeArchitecture original, UwpSceneView instance)
        {
            if (((SceneView)(object)instance).ProjectContext is ProjectContextBase context)
            {
                var hostProject = context.HostProject;
                var platformIdentifier = hostProject.PlatformIdentifier;

                if (hostProject.BuildPlatform.Equals("ARM64", StringComparison.OrdinalIgnoreCase) &&
                    platformIdentifier.TargetFrameworkIdentifier.Equals(".NETCoreApp", StringComparison.Ordinal) &&
                    platformIdentifier.TargetFrameworkVersion.Major >= 10)
                {
                    return false;
                }
            }

            return original(instance);
        }

        private delegate void UpdateRuntimeArchitecture(HostPlatformBase instance, SurfaceProcessInfo surfaceProcessInfo, IHostProject hostProject);

        private void UpdateRuntimeArchitectureHook(UpdateRuntimeArchitecture original, HostPlatformBase instance, SurfaceProcessInfo surfaceProcessInfo, IHostProject hostProject)
        {
            if (string.Equals(surfaceProcessInfo.Architecture, "ARM64", StringComparison.OrdinalIgnoreCase))
            {
                string architecture = (surfaceProcessInfo.RuntimeArchitecture = "ARM64");
                surfaceProcessInfo.Architecture = architecture;
            }
            else
            {
                original(instance, surfaceProcessInfo, hostProject);
            }
        }
    }
}
