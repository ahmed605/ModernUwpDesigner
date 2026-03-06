using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.Utility.Data;
using Microsoft.VisualStudio.DesignTools.Utility.Diagnostics;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal static class PackageDependencyHelper
{
	public static HostPackageDependency GetPackageDependency(IAppPackageHelper appPackageHelper, IHostSdkReference hostSdkReference, string configuration, string runtimeArchitecture)
	{
		string path = hostSdkReference.Path;
		if (File.Exists(path))
		{
			try
			{
				XmlDocument xmlDocument = XmlUtility.CreateXmlDocument();
				using (XmlReader reader = XmlUtility.CreateXmlReader(path))
				{
					xmlDocument.Load(reader);
				}
				XmlElement documentElement = xmlDocument.DocumentElement;
				if (documentElement == null || documentElement.Name != "FileList")
				{
					return null;
				}
				string attribute = documentElement.GetAttribute(string.Format(CultureInfo.InvariantCulture, "FrameworkIdentity-{0}", configuration));
				if (string.IsNullOrEmpty(attribute))
				{
					attribute = documentElement.GetAttribute("FrameworkIdentity");
				}
				string attribute2 = documentElement.GetAttribute(string.Format(CultureInfo.InvariantCulture, "AppX-{0}-{1}", configuration, runtimeArchitecture));
				if (string.IsNullOrEmpty(attribute2))
				{
					attribute2 = documentElement.GetAttribute(string.Format(CultureInfo.InvariantCulture, "AppX-{0}", runtimeArchitecture));
				}
				if (string.IsNullOrEmpty(attribute) || string.IsNullOrEmpty(attribute2))
				{
					return null;
				}
				IDictionary<string, string> dictionary = SdkReferenceHelperBase.ParseSdkIdentifier(attribute);
				if (!dictionary.TryGetValue("Name", out var value) || !dictionary.TryGetValue("MinVersion", out var value2) || !dictionary.TryGetValue("Publisher", out var value3))
				{
					return null;
				}
				string directoryName = Path.GetDirectoryName(path);
				attribute2 = Path.Combine(directoryName, attribute2);
				return new HostPackageDependency(value, value2, value3, attribute2);
			}
			catch (XmlException)
			{
			}
		}
		return null;
	}

	public static void RegisterPackageDependency(IAppPackageHelper appPackageHelper, HostPackageDependency packageDependency, PackageManifestUpdater packageManifestUpdater, CancellationToken cancelToken)
	{
		if (packageDependency != null)
		{
			packageManifestUpdater.RemovePackageDependency(packageDependency.Name);
			packageManifestUpdater.AddPackageDependency(packageDependency.Name, packageDependency.Version, packageDependency.Publisher);
			if (string.IsNullOrEmpty(packageDependency.Path) || !File.Exists(packageDependency.Path))
			{
				Logger.Debug(string.Format(CultureInfo.InvariantCulture, "Missing package Dependency. Path:{0}", packageDependency.Path ?? "NULL"), "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\Utility\\PackageDependencyHelper.cs");
				throw new FileNotFoundException(packageDependency.Path);
			}
			EnsureDependencyRegistered(appPackageHelper, packageDependency, cancelToken);
		}
	}

	private static void EnsureDependencyRegistered(IAppPackageHelper appPackageHelper, HostPackageDependency packageDependency, CancellationToken cancelToken)
	{
		if (!appPackageHelper.IsPackageRegisteredForCurrentUser(packageDependency.Path))
		{
			Logger.Debug("Registering package " + packageDependency.Path, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\Utility\\PackageDependencyHelper.cs");
			bool removeExistingPackages = false;
			Exception ex = appPackageHelper.RegisterPackage(packageDependency.Path, cancelToken, removeExistingPackages);
			if (ex != null)
			{
				Logger.Debug($"Failed to register dependency: {ex}", "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\Utility\\PackageDependencyHelper.cs");
				throw ex;
			}
		}
		else
		{
			Logger.Debug("Package already registered: " + packageDependency.Path, "D:\\dbs\\el\\ddvsm\\src\\Xaml\\Designer\\Source\\UwpDesignerHost\\Utility\\PackageDependencyHelper.cs");
		}
	}
}
