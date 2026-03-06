using System.Collections.Generic;
using System.Xml;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal static class ManifestCapabiltyAllowlist
{
	private class Capability
	{
		public string Name { get; set; }

		public string Namespace { get; set; }
	}

	private static readonly List<Capability> capabilities = new List<Capability>
	{
		new Capability
		{
			Name = "internetClient",
			Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10"
		},
		new Capability
		{
			Name = "runFullTrust",
			Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
		}
	};

	public static bool IsAllowListed(XmlNode xmlNode)
	{
		string text = xmlNode.Attributes?["Name"]?.Value;
		if (!string.IsNullOrWhiteSpace(text))
		{
			foreach (Capability capability in capabilities)
			{
				if (capability.Name == text && capability.Namespace == xmlNode.NamespaceURI)
				{
					return true;
				}
			}
		}
		return false;
	}
}
