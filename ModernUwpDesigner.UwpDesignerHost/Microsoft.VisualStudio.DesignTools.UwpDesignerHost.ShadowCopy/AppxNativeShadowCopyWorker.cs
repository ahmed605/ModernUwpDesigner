using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.DesignerHost.Utility;
using Microsoft.VisualStudio.DesignTools.Utility;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.ShadowCopy;

internal class AppxNativeShadowCopyWorker : AppxRecipeShadowCopyWorker
{
	private PackageRuntimeAssemblyHelper runtimeAssemblyHelper;

	private CoreRuntimeRegistrationHelper coreRuntimeRegistrationHelper;

	public IEnumerable<string> ProjectReferences { get; private set; } = Enumerable.Empty<string>();

	public override bool Initialize(IHostPlatform platform, IHostProject hostProject, IAppPackageHelper appPackageHelper, SurfaceProcessInfo surfaceInfo, IHostTelemetryService hostTelemetry)
	{
		if (!base.Initialize(platform, hostProject, appPackageHelper, surfaceInfo, hostTelemetry))
		{
			return false;
		}
		string a = base.HostProject.GetProperty("UseDotNetNativeToolchain") ?? "";
		if (!string.Equals(a, "true", StringComparison.InvariantCultureIgnoreCase))
		{
			return false;
		}
		if (!PlatformVersionHelper.IsAtLeastRelease(base.HostProject.PlatformIdentifier.TargetPlatformMinVersion, PlatformVersionHelper.MajorRelease.RS1))
		{
			return false;
		}
		coreRuntimeRegistrationHelper = new CoreRuntimeRegistrationHelper(base.HostProject, base.SurfaceInfo);
		IEnumerable<string> runtimeAssemblyPaths = AppxPlatformOnlyShadowCopyWorker.InitializeFrameworkRuntime(base.AppPackageHelper, surfaceInfo, hostProject, coreRuntimeRegistrationHelper);
		ProjectReferences = CollectStagedProjectReferences(base.HostProject, Path.GetDirectoryName(base.HostProject.TargetAssemblyPath));
		runtimeAssemblyHelper = new PackageRuntimeAssemblyHelper(runtimeAssemblyPaths, base.SurfaceInfo.RuntimeArchitecture);
		return true;
	}

	protected override bool HasValidFramework(string versionFromRecipe)
	{
		return true;
	}

	protected override void UpdateManifest(PackageManifestUpdater manifestUpdater, CancellationToken cancelToken)
	{
		base.UpdateManifest(manifestUpdater, cancelToken);
		CoreRuntimeRegistrationHelper coreRuntimeRegistrationHelper = new CoreRuntimeRegistrationHelper(base.HostProject, base.SurfaceInfo);
		coreRuntimeRegistrationHelper.EnsureCoreRuntimeRegistered(base.AppPackageHelper, manifestUpdater, cancelToken);
		IEnumerable<IHostSdkReference> sdkReferences = base.Platform.GetSdkReferences(base.HostProject);
		foreach (IHostSdkReference item in sdkReferences)
		{
			HostPackageDependency packageDependency = PackageDependencyHelper.GetPackageDependency(base.AppPackageHelper, item, "Debug", base.SurfaceInfo.RuntimeArchitecture);
			HostPackageDependency packageDependency2 = PackageDependencyHelper.GetPackageDependency(base.AppPackageHelper, item, "Retail", base.SurfaceInfo.RuntimeArchitecture);
			PackageDependencyHelper.RegisterPackageDependency(base.AppPackageHelper, packageDependency, manifestUpdater, cancelToken);
			PackageDependencyHelper.RegisterPackageDependency(base.AppPackageHelper, packageDependency2, manifestUpdater, cancelToken);
		}
		manifestUpdater.Save();
	}

	protected override void ExecuteCopy(CancellationToken cancelToken)
	{
		string[] packageRuntimeAssemblyPaths = runtimeAssemblyHelper.PackageRuntimeAssemblyPaths;
		foreach (string text in packageRuntimeAssemblyPaths)
		{
			base.SurfaceInfo.ShadowCacheContent.AddItem(text, Path.GetFileName(text));
		}
		foreach (string projectReference in ProjectReferences)
		{
			base.SurfaceInfo.ShadowCacheContent.AddItem(projectReference, Path.GetFileName(projectReference));
		}
		base.ExecuteCopy(cancelToken);
	}

	protected override List<FileCopyToken> FixRecipeCopyTokens(List<FileCopyToken> copyList, string targetAssemblyPath)
	{
		List<FileCopyToken> list = new List<FileCopyToken>();
		foreach (FileCopyToken copy in copyList)
		{
			if (ShouldIncludeToken(copy))
			{
				string text = copy.SourcePath;
				if (!string.IsNullOrEmpty(targetAssemblyPath) && string.Equals(Path.GetFileName(text), Path.GetFileName(targetAssemblyPath), StringComparison.OrdinalIgnoreCase))
				{
					text = targetAssemblyPath;
				}
				list.Add(new FileCopyToken(text, Uri.UnescapeDataString(copy.DestinationPath)));
			}
		}
		return list;
	}

	private static List<string> CollectStagedProjectReferences(IHostProject referencingProject, string topProjectDir)
	{
		List<string> list = new List<string>();
		foreach (IHostProject item in referencingProject.EnumerateProjectGraphPreorder())
		{
			if (item == referencingProject)
			{
				continue;
			}
			string text = item.TargetAssemblyPath;
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = Path.Combine(topProjectDir, Path.GetFileName(text));
				if (File.Exists(text2))
				{
					list.Add(text2);
				}
			}
		}
		return list;
	}
}
