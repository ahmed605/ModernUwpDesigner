using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.DesignTools.Markup.XmlModify;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal class PriCleaner : IXmlCleanerContext
{
	public static XmlResult Clean(string xaml)
	{
		return XmlCleaner.Clean(xaml, new PriCleaner());
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
		if (name.FullName != "NamedResource")
		{
			return XmlTagChange.None;
		}
		string attributeValue = cleaner.GetAttributeValue("name");
		if (attributeValue == null)
		{
			return XmlTagChange.None;
		}
		if (string.Equals("app.xbf", attributeValue, StringComparison.OrdinalIgnoreCase) || string.Equals("app.xaml", attributeValue, StringComparison.OrdinalIgnoreCase))
		{
			return XmlTagChange.Remove;
		}

        return XmlTagChange.None;
	}
}
