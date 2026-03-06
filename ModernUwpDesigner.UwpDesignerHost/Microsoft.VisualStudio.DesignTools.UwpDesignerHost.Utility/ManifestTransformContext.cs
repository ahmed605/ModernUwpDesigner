using System.Collections.Generic;
using Microsoft.VisualStudio.DesignTools.Markup.XmlModify;
using Windows.Foundation.Metadata;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal class ManifestTransformContext : IXmlCleanerContext
{
	private string newIdentity;

	private static string SurfaceDisplayName
	{
		get
		{
			if (ApiInformation.IsApiContractPresent("Windows.UI.Xaml.Hosting.HostingContract", 2))
			{
				return "NoUIEntryPoints-DesignModeV2";
			}
			return "XSurfUwp";
		}
	}

	public ManifestTransformContext(string newIdentity)
	{
		this.newIdentity = newIdentity;
	}

	object IXmlCleanerContext.GetTagData(XmlName name, int location, int level, bool allowCreateNewData)
	{
		return null;
	}

	IEnumerable<KeyValuePair<XmlName, string>> IXmlCleanerContext.GetAttributesToAppend(IXmlCleaner cleaner, bool isValidAttributeDetected)
	{
		return null;
	}

	XmlAttributeChange IXmlCleanerContext.HandleAttribute(XmlName name, string value, IXmlCleaner cleaner)
	{
		XmlName currentTag = cleaner.CurrentTag;
		if (currentTag.LocalName == "Identity" && name.LocalName == "Name")
		{
			return new XmlAttributeChange(XmlName.None, newIdentity);
		}
		if (currentTag.LocalName == "VisualElements" && name.LocalName == "DisplayName")
		{
			return new XmlAttributeChange(XmlName.None, SurfaceDisplayName);
		}
		return XmlAttributeChange.None;
	}

	string IXmlCleanerContext.HandleText(string text, IXmlCleaner cleaner)
	{
		return null;
	}

	void IXmlCleanerContext.HandleEndTag(XmlName name, IXmlCleaner cleaner)
	{
	}

	XmlTagChange IXmlCleanerContext.HandleStartTag(XmlName name, IXmlCleaner cleaner)
	{
		return XmlTagChange.None;
	}
}
