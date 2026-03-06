using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost;

[GeneratedCode("Microsoft.Build.Tasks.StronglyTypedResourceBuilder", "15.1.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class StringTable
{
	private static ResourceManager resourceMan;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("Microsoft.VisualStudio.DesignTools.UwpDesignerHost.StringTable", typeof(StringTable).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	internal static string CompatibleRuntimeCustomSdkDetailsFormat => ResourceManager.GetString("CompatibleRuntimeCustomSdkDetailsFormat");

	internal static string CompatibleRuntimeCustomSdkSummary => ResourceManager.GetString("CompatibleRuntimeCustomSdkSummary");

	internal static string CompatibleRuntimeDownlevelOSDetailsFormat => ResourceManager.GetString("CompatibleRuntimeDownlevelOSDetailsFormat");

	internal static string CompatibleRuntimeDownlevelOSSummary => ResourceManager.GetString("CompatibleRuntimeDownlevelOSSummary");

	internal static string CompatibleRuntimeVersionDescription => ResourceManager.GetString("CompatibleRuntimeVersionDescription");

	internal static string CompatibleRuntimeVersionLink => ResourceManager.GetString("CompatibleRuntimeVersionLink");

	internal static string CompatibleRuntimeVersionTitle => ResourceManager.GetString("CompatibleRuntimeVersionTitle");

	internal static string ExceptionMissingWindowsKitsInstallation => ResourceManager.GetString("ExceptionMissingWindowsKitsInstallation");

	internal static string RegisterPackageTimeoutMessage => ResourceManager.GetString("RegisterPackageTimeoutMessage");

	internal static string RemoveExistingPackageTimeoutMessage => ResourceManager.GetString("RemoveExistingPackageTimeoutMessage");

	internal static string ResourcePriGenerationFailed => ResourceManager.GetString("ResourcePriGenerationFailed");

	internal static string WindowsFallCreatorsUpdate => ResourceManager.GetString("WindowsFallCreatorsUpdate");

	internal StringTable()
	{
	}
}
