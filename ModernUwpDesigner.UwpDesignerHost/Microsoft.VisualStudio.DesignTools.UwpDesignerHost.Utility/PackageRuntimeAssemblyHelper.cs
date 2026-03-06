using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal class PackageRuntimeAssemblyHelper
{
	public string[] PackageRuntimeAssemblyPaths { get; }

	public PackageRuntimeAssemblyHelper(IEnumerable<string> runtimeAssemblyPaths, string runtimeArchitecture)
	{
		string coreClrPackage = string.Format(CultureInfo.InvariantCulture, "\\runtime.win7-{0}.Microsoft.NETCore.Runtime.CoreCLR\\", runtimeArchitecture);
		PackageRuntimeAssemblyPaths = runtimeAssemblyPaths.Where((string path) => path.IndexOf(coreClrPackage, StringComparison.OrdinalIgnoreCase) == -1).ToArray();
	}
}
