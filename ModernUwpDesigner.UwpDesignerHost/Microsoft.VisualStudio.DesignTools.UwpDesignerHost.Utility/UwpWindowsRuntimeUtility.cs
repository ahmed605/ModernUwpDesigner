using System;
using Windows.System.Profile;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal class UwpWindowsRuntimeUtility
{
	public static Version GetRuntimePlatformVersion()
	{
		string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
		if (!string.IsNullOrEmpty(deviceFamilyVersion) && long.TryParse(deviceFamilyVersion, out var result))
		{
			int major = (int)((result >> 48) & 0xFFFF);
			int minor = (int)((result >> 32) & 0xFFFF);
			int build = (int)((result >> 16) & 0xFFFF);
			int revision = (int)(result & 0xFFFF);
			return new Version(major, minor, build, revision);
		}
		return null;
	}
}
