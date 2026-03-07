using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.DesignerHost.HostServices;
using Microsoft.VisualStudio.DesignTools.DesignerHost.Platform;
using Microsoft.VisualStudio.DesignTools.DesignerHost.ShadowCopy;
using Microsoft.VisualStudio.DesignTools.DesignerHost.Utility;
using Microsoft.VisualStudio.DesignTools.Utility;
using Microsoft.VisualStudio.DesignTools.Utility.Diagnostics;
using Microsoft.VisualStudio.DesignTools.Utility.IO;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;
using Microsoft.VisualStudio.DesignTools.Xaml.LanguageService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.ShadowCopy;

internal sealed class AppxPlatformOnlyShadowCopyWorker : UwpHostShadowCopyWorker
{
	private WindowsMetadataHelper winmdHelper;

	private PackageRuntimeAssemblyHelper runtimeAssemblyHelper;

	private CoreRuntimeRegistrationHelper coreRuntimeRegistrationHelper;

	public override bool HasAppXbf => false;

	public override bool Initialize(IHostPlatform platform, IHostProject hostProject, IAppPackageHelper appPackageHelper, SurfaceProcessInfo surfaceInfo, IHostTelemetryService hostTelemetry)
	{
		base.Initialize(platform, hostProject, appPackageHelper, surfaceInfo, hostTelemetry);
		base.SurfaceInfo.ShadowCopyType = HostShadowCopyType.PlatformOnly;
		coreRuntimeRegistrationHelper = new CoreRuntimeRegistrationHelper(base.HostProject, base.SurfaceInfo);
		IEnumerable<string> runtimeAssemblyPaths = InitializeFrameworkRuntime(base.AppPackageHelper, base.SurfaceInfo, base.HostProject, coreRuntimeRegistrationHelper);
		winmdHelper = new WindowsMetadataHelper(base.HostProject.PlatformIdentifier);
		runtimeAssemblyHelper = new PackageRuntimeAssemblyHelper(runtimeAssemblyPaths, base.SurfaceInfo.RuntimeArchitecture);
		return true;
	}

	public override unsafe Task CopyProjectContentAsync(CancellationToken cancelToken)
	{
		return Task.Run(delegate
		{
			base.SurfaceInfo.ShadowCacheContent.AddItem(winmdHelper.WindowsMetaDataLocation, winmdHelper.Destination);
			if (base.SurfaceInfo.FrameworkRuntimeName == ".NETCore")
			{
				string[] packageRuntimeAssemblyPaths = runtimeAssemblyHelper.PackageRuntimeAssemblyPaths;
				foreach (string text in packageRuntimeAssemblyPaths)
				{
					cancelToken.ThrowIfCancellationRequested();
					base.SurfaceInfo.ShadowCacheContent.AddItem(text, Path.GetFileName(text));
				}
			}

			// TODO: probably also need to check if the project is executable but has no publish profiles?
			if (!HostProject.IsExecutable && HostProject is HostProject hostProject)
			{
				var GetReferenceResolver = (delegate*<HostProject, ReferenceResolver>)typeof(HostProject).GetProperty("ReferenceResolver",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
						.GetGetMethod(true).MethodHandle.GetFunctionPointer();

				if (GetReferenceResolver(hostProject) is DTEReferenceResolver resolver)
				{
					var refs = resolver.References.Where(x => x.CopyLocal is not false);
					foreach (var reference in refs)
					{
						cancelToken.ThrowIfCancellationRequested();

						var path = reference.Path.LocalPath;
                        base.SurfaceInfo.ShadowCacheContent.AddItem(path, Path.GetFileName(path));
                    }
                }

                SurfaceInfo.ShadowCacheContent.CacheFileFromText(Path.Combine(SurfaceInfo.ShadowCacheContent.ShadowCacheFolder, "UserApplicationInfo.txt"), GetApplicationActivationArguments());
            }

			TrackProjectItems(base.SurfaceInfo.ShadowCacheContent, base.HostProject, cancelToken);
			base.SurfaceInfo.ShadowCacheContent.SyncFolder(cancelToken);
			if (string.IsNullOrEmpty(base.SurfaceInfo.FrameworkRuntimeVersion) && base.SurfaceInfo.ShadowCacheContent != null)
			{
				base.SurfaceInfo.FrameworkRuntimeAssemblyVersion = base.SurfaceInfo.ShadowCacheContent.FindAssemblyVersionFromSourceDirectory("System.Runtime.dll");
			}
			UpdateDependencies(cancelToken);
		});
	}

    private string GetApplicationActivationArguments()
    {
        if (!string.IsNullOrEmpty(HostProject.TargetAssemblyPath) && File.Exists(HostProject.TargetAssemblyPath) && !string.IsNullOrEmpty(HostProject.DefaultNamespaceName))
        {
            return string.Format(CultureInfo.InvariantCulture, "{0};{1};{2}", ProjectNameHelper.GetSafeProjectName(HostProject.ProjectName), AssemblyName.GetAssemblyName(HostProject.TargetAssemblyPath).FullName, HostProject.DefaultNamespaceName);
        }
        return string.Empty;
    }

    private void UpdateDependencies(CancellationToken cancelToken)
	{
		cancelToken.ThrowIfCancellationRequested();
		string manifestPath = base.SurfaceInfo.ShadowCacheContent.FindCachedItem("AppxManifest.xml");
		PackageManifestUpdater packageManifestUpdater = new PackageManifestUpdater(manifestPath);
		PlatformIdentifier platformIdentifier = base.HostProject.PlatformIdentifier;
		packageManifestUpdater.UpdateTargetDeviceFamily("Windows.Universal", platformIdentifier.TargetPlatformVersion, platformIdentifier.TargetPlatformMinVersion);
		if (base.SurfaceInfo.FrameworkRuntimeName == ".NETCore")
		{
			coreRuntimeRegistrationHelper.EnsureCoreRuntimeRegistered(base.AppPackageHelper, packageManifestUpdater, cancelToken);
		}
		IEnumerable<IHostSdkReference> sdkReferences = base.Platform.GetSdkReferences(base.HostProject);
		foreach (IHostSdkReference item in sdkReferences)
		{
			cancelToken.ThrowIfCancellationRequested();
			HostPackageDependency packageDependency = PackageDependencyHelper.GetPackageDependency(base.AppPackageHelper, item, "Debug", base.SurfaceInfo.RuntimeArchitecture);
			HostPackageDependency packageDependency2 = PackageDependencyHelper.GetPackageDependency(base.AppPackageHelper, item, "Retail", base.SurfaceInfo.RuntimeArchitecture);
			Logger.Debug("Ensuring package " + packageDependency?.Path, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxPlatformOnlyShadowCopyWorker.cs");
			PackageDependencyHelper.RegisterPackageDependency(base.AppPackageHelper, packageDependency, packageManifestUpdater, cancelToken);
			Logger.Debug("Ensuring package " + packageDependency2?.Path, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxPlatformOnlyShadowCopyWorker.cs");
			PackageDependencyHelper.RegisterPackageDependency(base.AppPackageHelper, packageDependency2, packageManifestUpdater, cancelToken);
		}
		packageManifestUpdater.Save();
	}

	public override Task<bool> TryEvictAppXbfAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(result: false);
	}

	public static void TrackProjectItems(IHostShadowCacheContent shadowCacheItems, IHostProject hostProject, CancellationToken cancelToken)
	{
        if (hostProject.Items == null)
		{
			return;
		}
		MrtShadowCacheHelper mrtShadowCacheHelper = new MrtShadowCacheHelper();
		foreach (IHostSourceItem item in hostProject.Items)
		{
			cancelToken.ThrowIfCancellationRequested();
			if (MediaFileExtensions.IsMediaFile(item.Path))
			{
				mrtShadowCacheHelper.UpdateMrtCache(item);
				string deploymentRelativePath = UwpUriResolver.GetDeploymentRelativePath(hostProject, item.RelativePath, isForRuntime: false);
				shadowCacheItems.AddItem(item.Path, deploymentRelativePath);
			}
		}
		foreach (KeyValuePair<string, ProjectItemWithScore> item2 in mrtShadowCacheHelper.MrtItemsCache)
		{
			shadowCacheItems.AddItem(item2.Value.Item.Path, item2.Key);
		}
	}

	public static IEnumerable<string> InitializeFrameworkRuntime(IAppPackageHelper appPackageHelper, SurfaceProcessInfo surfaceProcessInfo, IHostProject hostProject, CoreRuntimeRegistrationHelper coreRuntimeRegistrationHelper)
	{
		IEnumerable<string> enumerable = (hostProject as IHostDesignerProject)?.GetPackageRuntimeAssemblyPaths() ?? Array.Empty<string>();
		if (!enumerable.Any() && coreRuntimeRegistrationHelper.IsCPlusPlus)
		{
			enumerable = EnvironmentHelper.FindRuntimeAssemblyPaths(surfaceProcessInfo.Architecture);
		}
		bool flag = enumerable.Any((string path) => string.Equals(Path.GetFileNameWithoutExtension(path), "System.Runtime", StringComparison.OrdinalIgnoreCase));
		if (flag)
		{
			CheckCoreRuntimeMismatch(appPackageHelper, surfaceProcessInfo, hostProject, coreRuntimeRegistrationHelper, enumerable);
		}
		if (flag && !surfaceProcessInfo.IsCoreRuntimeMismatch)
		{
			string text = coreRuntimeRegistrationHelper.FindCoreRuntimeVersion(appPackageHelper);
			if (string.IsNullOrEmpty(text))
			{
				Logger.Debug("Falling back to desktop runtime because core CLR version is unknown", "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\ShadowCopy\\AppxPlatformOnlyShadowCopyWorker.cs");
			}
			else
			{
				surfaceProcessInfo.FrameworkRuntimeName = ".NETCore";
				surfaceProcessInfo.FrameworkRuntimeVersion = text;
			}
		}
		if (string.IsNullOrEmpty(surfaceProcessInfo.FrameworkRuntimeName))
		{
			surfaceProcessInfo.FrameworkRuntimeName = ".NETFramework";
			enumerable = Array.Empty<string>();
			if (!surfaceProcessInfo.IsCoreRuntimeMismatch)
			{
				surfaceProcessInfo.FrameworkRuntimeVersion = Environment.Version.ToString();
			}
		}
		surfaceProcessInfo.PackageRuntimeAssemblyCount = enumerable.Count();
		return enumerable;
	}

	private static void CheckCoreRuntimeMismatch(IAppPackageHelper appPackageHelper, SurfaceProcessInfo surfaceProcessInfo, IHostProject hostProject, CoreRuntimeRegistrationHelper coreRuntimeRegistrationHelper, IEnumerable<string> runtimeAssemblyPaths)
	{
		try
		{
			PlatformIdentifier platformIdentifier = hostProject.PlatformIdentifier;
			if (platformIdentifier == null || !PlatformVersionHelper.IsAtLeastRelease(platformIdentifier.TargetPlatformVersion, PlatformVersionHelper.MajorRelease.RS3))
			{
				return;
			}
			string property = VS2026CompatibleGetProperty.GetPropertyCompat(hostProject, "CoreRuntimeSDKName");
			if (string.IsNullOrEmpty(property))
			{
				return;
			}
			SdkName sdkName = new SdkName(property);
			if (sdkName.Identifier != "Microsoft.NET.CoreRuntime" || coreRuntimeRegistrationHelper.IsCPlusPlus)
			{
				return;
			}
			string text = HostShadowCacheContent.FindAssemblyVersion("System.Runtime.dll", runtimeAssemblyPaths);
			string[] array = text.Split(';');
			foreach (string text2 in array)
			{
				Version version = HostShadowCacheContent.ParseAssemblyVersion(text2);
				if (version != null && ((sdkName.Version.Major >= 2 && version.Major < 6) || (sdkName.Version.Major < 2 && version.Major >= 6)))
				{
					surfaceProcessInfo.FrameworkRuntimeVersion = coreRuntimeRegistrationHelper.FindCoreRuntimeVersion(appPackageHelper);
					surfaceProcessInfo.FrameworkRuntimeAssemblyVersion = text;
					surfaceProcessInfo.IsCoreRuntimeMismatch = true;
				}
			}
		}
		catch (Exception)
		{
		}
	}
}
