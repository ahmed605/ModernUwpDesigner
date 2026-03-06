using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.DesignerHost.HostServices;
using Microsoft.VisualStudio.DesignTools.Utility.Telemetry;
using Microsoft.VisualStudio.Telemetry;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;

public class AppRegistrationCleaner
{
	private class DatedPackagePath
	{
		public DateTime LastWritten { get; }

		public string ManifestPath { get; }

		public double DaysOld { get; }

		public DatedPackagePath(DateTime lastWritten, string manifestPath)
		{
			LastWritten = lastWritten;
			ManifestPath = manifestPath;
			DaysOld = (DateTime.UtcNow - LastWritten).TotalDays;
		}
	}

	private readonly IAppPackageHelper appPackageHelper;

	private readonly int removeBatchSize = 5;

	private readonly int packageCountLimit = 20;

	private readonly IHostTelemetryService telemetryService;

	private readonly string surfaceProcessIdentifier;

	public AppRegistrationCleaner(IHostTelemetryService telemetryService, string surfaceProcessIdentifier, IAppPackageHelper appPackageHelper)
	{
		this.telemetryService = telemetryService;
		this.appPackageHelper = appPackageHelper;
		this.surfaceProcessIdentifier = surfaceProcessIdentifier;
	}

	public AppRegistrationCleaner(int removeBatchSize, int packageCountLimit, IHostTelemetryService telemetryService, IAppPackageHelper appPackageHelper)
		: this(telemetryService, string.Empty, appPackageHelper)
	{
		this.removeBatchSize = removeBatchSize;
		this.packageCountLimit = packageCountLimit;
	}

	public void CleanOldPackages()
	{
		IEnumerable<AppPackageInfo> installedPackageLocationsForUser = appPackageHelper.GetInstalledPackageLocationsForUser();
		DataPointCollection dataPointCollection = new DataPointCollection();
		dataPointCollection.Add("PackagesInstalledCount", installedPackageLocationsForUser.Count());
		dataPointCollection.Add("SurfaceProcessIdentifier", surfaceProcessIdentifier);
		using ITelemetryScope telemetryScope = telemetryService.TelemetryService?.StartOperation(TelemetryArea.DesignSurface, "Clean-AppRegistration", dataPointCollection);
		IEnumerable<AppPackageInfo> manifestsForPackagesToRemove = GetManifestsForPackagesToRemove(installedPackageLocationsForUser);
		foreach (AppPackageInfo item in manifestsForPackagesToRemove)
		{
			UninstallApp(item, telemetryScope);
		}
	}

	private IEnumerable<AppPackageInfo> GetManifestsForPackagesToRemove(IEnumerable<AppPackageInfo> packages)
	{
		string shadowCacheFolderName = Path.DirectorySeparatorChar + HostPlatformBase.SurfaceProcessShadowCache + Path.DirectorySeparatorChar;
		IEnumerable<AppPackageInfo> source = packages.Where((AppPackageInfo p) => !p.InstallFilesExist || p.InstallLocation.Contains(shadowCacheFolderName));
		if (source.Count() > packageCountLimit)
		{
			return (from p in source
				where !p.InstallFilesExist || p.DaysOld > 1.0
				orderby p.LastWritten
				select p).Take(removeBatchSize);
		}
		return Enumerable.Empty<AppPackageInfo>();
	}

	private void UninstallApp(AppPackageInfo packageInfo, ITelemetryScope telemetryScope)
	{
		DateTime now = DateTime.Now;
		appPackageHelper.UnregisterPackage(packageInfo.Moniker, null, waitForCompletion: true);
		DataPointCollection dataPointCollection = new DataPointCollection();
		dataPointCollection.Add("SurfaceProcessIdentifier", surfaceProcessIdentifier);
		dataPointCollection.Add("PackageMoniker", packageInfo.Moniker);
		dataPointCollection.Add("ProjectId", GetProjectId(packageInfo.InstallLocation));
		dataPointCollection.Add("UnregisterTime", TelemetryHelper.TimeSince(now));
		dataPointCollection.Add("DaysSinceUsed", packageInfo.DaysOld);
		dataPointCollection.Add(new DataPoint("PackageInstallationPath", packageInfo.InstallLocation, isPersonallyIdentifiable: true));
		if (packageInfo.InstallFilesExist && !appPackageHelper.IsPackageMonikerRegisteredToCurrentUser(packageInfo.Moniker))
		{
			try
			{
				Directory.Delete(packageInfo.InstallLocation, recursive: true);
				telemetryScope?.PostOperation(TelemetryArea.DesignSurface, "Uninstall-AppPackage", TelemetryResult.Success, dataPointCollection);
				return;
			}
			catch (UnauthorizedAccessException exception)
			{
				telemetryScope?.PostFault("Uninstall-AppPackage-DeleteDirectory-Failure", exception, dataPointCollection);
				return;
			}
			catch (IOException exception2)
			{
				telemetryScope?.PostFault("Uninstall-AppPackage-DeleteDirectory-Failure", exception2, dataPointCollection);
				return;
			}
		}
		telemetryScope?.PostFault("Uninstall-AppPackage-Unregistration-Failure", null, dataPointCollection);
	}

	private static string GetProjectId(string packagePath)
	{
		string result = string.Empty;
		string path = Path.Combine(packagePath, "__ProjectInfo__.txt");
		if (File.Exists(path))
		{
			try
			{
				string projectInfoContent = File.ReadAllText(path);
				result = SurfaceProcessInfo.GetProjectGuidFromString(projectInfoContent);
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
		return result;
	}
}
