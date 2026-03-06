using System.IO;
using Microsoft.VisualStudio.DesignTools.DesignerHost.Utility;
using Microsoft.VisualStudio.DesignTools.Utility;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal class WindowsMetadataHelper
{
	public string WindowsMetaDataLocation { get; private set; }

	public string Destination => Path.Combine("WinMetadata", Path.GetFileName(WindowsMetaDataLocation));

	public WindowsMetadataHelper(PlatformIdentifier platformIdentifier)
	{
        //WindowsMetaDataLocation = Path.Combine(EnvironmentHelper.GetRuntimeWinmdLocation(platformIdentifier.GetTargetSdk(), platformIdentifier.GetTargetPlatform()), "Windows.winmd");

        // HACK: FIX ME

        var identifier = platformIdentifier;
        if (identifier.TargetPlatformIdentifier != PlatformNames.UAP)
            identifier = new PlatformIdentifier(new PlatformName(PlatformNames.UAP, platformIdentifier.TargetPlatformVersion), platformIdentifier.TargetRuntime, platformIdentifier.GetTargetFramework(), platformIdentifier.GetTargetSdk(), XamlRuntimeNames.UAP);

        this.WindowsMetaDataLocation = Path.Combine(EnvironmentHelper.GetRuntimeWinmdLocation(identifier.GetTargetSdk(), identifier.GetTargetPlatform()), "Windows.winmd");
    }
}
