using System;
using System.IO;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;

public class AppPackageInfo
{
	private string ManifestPath
	{
		get
		{
			if (string.IsNullOrEmpty(InstallLocation))
			{
				return string.Empty;
			}
			return Path.Combine(InstallLocation, "AppxManifest.xml");
		}
	}

	public string Moniker { get; }

	public string InstallLocation { get; }

	public bool InstallFilesExist
	{
		get
		{
			if (!string.IsNullOrEmpty(ManifestPath))
			{
				return File.Exists(ManifestPath);
			}
			return false;
		}
	}

	public DateTime LastWritten
	{
		get
		{
			if (InstallFilesExist)
			{
				try
				{
					return File.GetLastWriteTimeUtc(ManifestPath);
				}
				catch (UnauthorizedAccessException)
				{
				}
			}
			return DateTime.UtcNow;
		}
	}

	public double DaysOld
	{
		get
		{
			if (!string.IsNullOrEmpty(InstallLocation))
			{
				try
				{
					if (File.Exists(ManifestPath))
					{
						return (DateTime.UtcNow - LastWritten).TotalDays;
					}
				}
				catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
				{
				}
			}
			return double.NaN;
		}
	}

	public AppPackageInfo(string fullName, string installLocation)
	{
		Moniker = fullName;
		InstallLocation = installLocation;
	}
}
