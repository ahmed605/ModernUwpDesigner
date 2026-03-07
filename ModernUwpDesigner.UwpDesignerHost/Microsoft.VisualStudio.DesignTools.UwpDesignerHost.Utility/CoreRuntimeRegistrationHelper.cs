using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.DesignerHost.Utility;
using Microsoft.VisualStudio.DesignTools.Utility;
using Microsoft.VisualStudio.DesignTools.Utility.Diagnostics;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal class CoreRuntimeRegistrationHelper
{
	internal const string CoreRuntimeIdentifier = "Microsoft.NET.CoreRuntime";

	private SurfaceProcessInfo info;

	private IHostDesignerProject hostProject;

	public bool IsCPlusPlus => info.ProjectLanguage.IsCPlusPlus();

	public CoreRuntimeRegistrationHelper(IHostProject hostProject, SurfaceProcessInfo info)
	{
		this.hostProject = (IHostDesignerProject)hostProject;
		this.info = info;
	}

	public void EnsureCoreRuntimeRegistered(IAppPackageHelper appPackageHelper, PackageManifestUpdater packageManifestUpdater, CancellationToken cancelToken)
	{
		IEnumerable<HostPackageDependency> coreRuntimeForProject = GetCoreRuntimeForProject(appPackageHelper);
		if (!coreRuntimeForProject.Any())
		{
			return;
		}
		packageManifestUpdater.RemovePackageDependency("Microsoft.NET.CoreRuntime.*");
		foreach (HostPackageDependency item in coreRuntimeForProject)
		{
			PackageDependencyHelper.RegisterPackageDependency(appPackageHelper, item, packageManifestUpdater, cancelToken);
		}
	}

	public string FindCoreRuntimeVersion(IAppPackageHelper appPackageHelper)
	{
		IEnumerable<HostPackageDependency> coreRuntimeForProject = GetCoreRuntimeForProject(appPackageHelper);
		foreach (HostPackageDependency item in coreRuntimeForProject)
		{
			if (item != null && item.Name.StartsWith("Microsoft.NET.CoreRuntime") && Version.TryParse(item.Version, out var result))
			{
				return result.ToString();
			}
		}
		return string.Empty;
	}

	private IEnumerable<HostPackageDependency> GetCoreRuntimeForProject(IAppPackageHelper appPackageHelper)
	{
		string property = VS2026CompatibleGetProperty.GetPropertyCompat(hostProject, "CoreRuntimeSDKName");
		if (!string.IsNullOrEmpty(property) && !IsCPlusPlus)
		{
			SdkName sdkName = new SdkName(property);
			if (sdkName.Identifier == "Microsoft.NET.CoreRuntime" && sdkName.Version < new Version("2.0"))
			{
				IHostSdkReference hostSdkReference = hostProject.ResolveSdkReference(property);
				if (hostSdkReference != null)
				{
					return new List<HostPackageDependency> { PackageDependencyHelper.GetPackageDependency(appPackageHelper, hostSdkReference, "", info.RuntimeArchitecture) };
				}
				Logger.Debug("CoreRuntime package not found for .NET Core 1.x project", "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\Utility\\CoreRuntimeRegistrationHelper.cs");
				return Enumerable.Empty<HostPackageDependency>();
			}
		}
		if (IsCPlusPlus)
		{
			return GenerateHostPackageDependencyFromRegisteredPackaged();
		}
		Logger.Debug("Probing for DesignTimeAppxPackageRegistration packages.", "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\Utility\\CoreRuntimeRegistrationHelper.cs");
		return new List<HostPackageDependency>(hostProject.GetPackageDependencies(info.RuntimeArchitecture));
	}

	private IEnumerable<(string name, string version, string publisher, string installLocation)> ListNetCoreRegisteredPackage(string appxPackageName)
	{
		using System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create();
		powerShell.AddCommand("get-AppxPackage");
		powerShell.AddParameter("name", appxPackageName);
		Collection<PSObject> collection = powerShell.Invoke();
		List<(string, string, string, string)> list = new List<(string, string, string, string)>();
		foreach (PSObject item in collection)
		{
			if (item.Properties["Architecture"].Value.ToString().Equals(info.RuntimeArchitecture, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(((string)item.Properties["Name"].Value, (string)item.Properties["Version"].Value, (string)item.Properties["Publisher"].Value, (string)item.Properties["InstallLocation"].Value));
			}
		}
		return list;
	}

	private IEnumerable<HostPackageDependency> GenerateHostPackageDependencyFromRegisteredPackaged()
	{
		List<HostPackageDependency> list = new List<HostPackageDependency>();
		if (info.ProjectLanguage != ProjectLanguage.CPlusPlusCX && info.ProjectLanguage != ProjectLanguage.CPlusPlusWinRT)
		{
			return list;
		}
		IEnumerable<string> enumerable = EnvironmentHelper.FindAppxPackageLocation(info.Architecture);
		if (!enumerable.Any())
		{
			return list;
		}
		AddDependencies(list, enumerable, "Microsoft.NET.CoreRuntime.2.2");
		AddDependencies(list, enumerable, "Microsoft.NET.CoreFramework.Debug.2.2");
		return list;
	}

	private void AddDependencies(List<HostPackageDependency> dependencies, IEnumerable<string> appxFiles, string packageName)
	{
		IEnumerable<(string, string, string, string)> enumerable = ListNetCoreRegisteredPackage(packageName);
		string appxFileName = packageName + ".appx";
		string text = appxFiles.First(delegate(string item)
		{
			string fileName = Path.GetFileName(item);
			return fileName.Equals(appxFileName, StringComparison.OrdinalIgnoreCase);
		});
		if (!enumerable.Any())
		{
			RegisterAppX(text);
			enumerable = ListNetCoreRegisteredPackage(packageName);
		}
		foreach (var item in enumerable)
		{
			dependencies.Add(new HostPackageDependency(item.Item1, item.Item2, item.Item3, text));
		}
	}

	private void RegisterAppX(string appxPath)
	{
		using System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create();
		powerShell.AddCommand("add-AppxPackage");
		powerShell.AddParameter("path", appxPath);
		powerShell.Invoke();
	}
}
