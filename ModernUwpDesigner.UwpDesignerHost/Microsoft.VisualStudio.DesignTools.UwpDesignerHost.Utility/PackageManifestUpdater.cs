using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.VisualStudio.DesignTools.Utility.Data;
using Microsoft.VisualStudio.DesignTools.Utility.IO;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal class PackageManifestUpdater
{
	public static readonly string originalManifestExtension = ".original";

	private string manifestPath;

	private string currentManifestContent;

	private XmlDocument manifestDocument;

	public PackageManifestUpdater(string manifestPath)
	{
		this.manifestPath = manifestPath;
		if (File.Exists(this.manifestPath))
		{
			manifestDocument = XmlUtility.CreateXmlDocument();
			using (XmlReader reader = XmlUtility.CreateXmlReader(this.manifestPath))
			{
				manifestDocument.Load(reader);
			}
			using StringWriter stringWriter = new StringWriter();
			manifestDocument.Save(stringWriter);
			currentManifestContent = stringWriter.ToString();
		}
	}

	public void Save()
	{
		if (manifestDocument == null)
		{
			return;
		}
		using StringWriter stringWriter = new StringWriter();
		manifestDocument.Save(stringWriter);
		string b = stringWriter.ToString();
		if (!string.Equals(currentManifestContent, b, StringComparison.Ordinal))
		{
			manifestDocument.Save(manifestPath);
			currentManifestContent = stringWriter.ToString();
		}
	}

	public void UpdateTargetDeviceFamily(string name, Version version, Version minVersion)
	{
		XmlNodeList elementsByTagName = manifestDocument.GetElementsByTagName("TargetDeviceFamily");
		if (elementsByTagName.Count > 0 && IsValidTargetVersion(version) && IsValidTargetVersion(minVersion) && minVersion <= version)
		{
			XmlElement element = (XmlElement)elementsByTagName[0];
			SetAttribute(element, "Name", name);
			SetAttribute(element, "MaxVersionTested", version.ToString());
			SetAttribute(element, "MinVersion", minVersion.ToString());
		}
	}

	public bool CheckTargetDeviceFamily(string name, Version minVersion)
	{
		if (manifestDocument != null)
		{
			XmlNodeList elementsByTagName = manifestDocument.GetElementsByTagName("TargetDeviceFamily");
			if (elementsByTagName.Count > 0)
			{
				XmlElement xmlElement = (XmlElement)elementsByTagName[0];
				string attribute = xmlElement.GetAttribute("Name");
				if (!string.Equals(name, attribute, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				string attribute2 = xmlElement.GetAttribute("MinVersion");
				if (minVersion != null && !string.IsNullOrEmpty(attribute2) && Version.TryParse(attribute2, out var result) && result > minVersion)
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool IsValidTargetVersion(Version version)
	{
		if (version != null)
		{
			return version.Major > 0;
		}
		return false;
	}

	public void ImportDependenciesAndExtensionsFromOriginal()
	{
		if (manifestDocument == null)
		{
			return;
		}
		XmlDocument xmlDocument = null;
		try
		{
			string text = manifestPath + originalManifestExtension;
			if (File.Exists(text))
			{
				using XmlReader reader = XmlUtility.CreateXmlReader(text);
				xmlDocument = XmlUtility.CreateXmlDocument();
				xmlDocument.Load(reader);
			}
		}
		catch (FileNotFoundException)
		{
		}
		catch (XmlException)
		{
		}
		if (xmlDocument != null)
		{
			ImportNodeByName(xmlDocument, "Dependencies", (XmlNode x) => !IsMainPackageDependency(x));
			ImportNodeByName(xmlDocument, "Extensions");
			ImportNodeByName(xmlDocument, "Capabilities", (XmlNode x) => ManifestCapabiltyAllowlist.IsAllowListed(x));
			Save();
		}
	}

	private static bool IsMainPackageDependency(XmlNode node)
	{
		if (node.NamespaceURI.Equals("http://schemas.microsoft.com/appx/manifest/uap/windows10/3"))
		{
			return node.LocalName.Equals("MainPackageDependency");
		}
		return false;
	}

	public void CreateInProcessServer(string exePath, string defaultNamespace, string projectName)
	{
		IAccessService accessService = AccessHelper.AccessService;
		string text = exePath + ".dll";
		if (!File.Exists(exePath))
		{
			return;
		}
		try
		{
			DateTime lastWriteTimeUtc = accessService.FileGetLastWriteTimeUtc(exePath);
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			accessService.FileMove(exePath, text);
			accessService.MiscConvertExeToDll(text);
			accessService.FileSetLastWriteTimeUtc(text, lastWriteTimeUtc);
			string className = defaultNamespace + "." + projectName + "_XamlTypeInfo.XamlMetaDataProvider";
			AddActivatableClass(Path.GetFileName(text), className);
		}
		catch (IOException)
		{
		}
	}

	internal void AddActivatableClass(string dllReference, string className)
	{
		if (manifestDocument != null && !ContainsActivatableClass(dllReference, className))
		{
			XmlElement element = GetElement("Extensions", tryCreate: true);
			if (element != null)
			{
				XmlElement newChild = CreateActivatableClassNode(dllReference, className);
				element.AppendChild(newChild);
				Save();
			}
		}
	}

	private bool ContainsActivatableClass(string pathName, string className)
	{
		XmlNodeList elementsByTagName = manifestDocument.DocumentElement.GetElementsByTagName("InProcessServer");
		foreach (XmlNode item in elementsByTagName)
		{
			if (item.ChildNodes.Count == 2 && item["Path"].InnerText.Equals(pathName) && item["ActivatableClass"].GetAttribute("ActivatableClassId").Contains(className))
			{
				return true;
			}
		}
		return false;
	}

	private XmlElement CreateActivatableClassNode(string dllReference, string className)
	{
		string namespaceURI = manifestDocument.DocumentElement.NamespaceURI;
		XmlElement xmlElement = manifestDocument.CreateElement("Path", namespaceURI);
		xmlElement.InnerText = dllReference;
		XmlElement xmlElement2 = manifestDocument.CreateElement("ActivatableClass", namespaceURI);
		xmlElement2.SetAttribute("ActivatableClassId", className);
		xmlElement2.SetAttribute("ThreadingModel", "both");
		XmlElement xmlElement3 = manifestDocument.CreateElement("InProcessServer", namespaceURI);
		xmlElement3.AppendChild(xmlElement);
		xmlElement3.AppendChild(xmlElement2);
		XmlElement xmlElement4 = manifestDocument.CreateElement("Extension", namespaceURI);
		xmlElement4.SetAttribute("Category", "windows.activatableClass.inProcessServer");
		xmlElement4.AppendChild(xmlElement3);
		return xmlElement4;
	}

	private XmlElement GetElement(string elementName, bool tryCreate)
	{
		XmlNode xmlNode = FindFirstChildWithName(manifestDocument, "Package");
		if (xmlNode != null)
		{
			XmlNode xmlNode2 = FindFirstChildWithName(xmlNode, elementName);
			if (xmlNode2 == null && tryCreate)
			{
				xmlNode2 = manifestDocument.CreateElement(elementName, xmlNode.NamespaceURI);
				manifestDocument.DocumentElement.AppendChild(xmlNode2);
			}
			return xmlNode2 as XmlElement;
		}
		return null;
	}

	private void RemoveElement(string elementName)
	{
		XmlNodeList elementsByTagName = manifestDocument.GetElementsByTagName(elementName);
		for (int num = elementsByTagName.Count - 1; num >= 0; num--)
		{
			elementsByTagName[num].ParentNode.RemoveChild(elementsByTagName[num]);
		}
	}

	private void SetAttribute(XmlElement element, string name, string value)
	{
		if (!string.Equals(element.GetAttribute(name), value, StringComparison.Ordinal))
		{
			element.SetAttribute(name, value);
		}
	}

	private void ImportNodeByName(XmlDocument importDocument, string nodeName, Func<XmlNode, bool> inclusionFilter = null)
	{
		RemoveElement(nodeName);
		foreach (XmlNode item in importDocument.GetElementsByTagName(nodeName))
		{
			if (!item.ParentNode.Equals(importDocument.DocumentElement))
			{
				continue;
			}
			if (inclusionFilter != null)
			{
				List<XmlNode> list = new List<XmlNode>();
				foreach (XmlNode childNode in item.ChildNodes)
				{
					if (!inclusionFilter(childNode))
					{
						list.Add(childNode);
					}
				}
				foreach (XmlNode item2 in list)
				{
					item.RemoveChild(item2);
				}
			}
			XmlNode newChild = manifestDocument.ImportNode(item, deep: true);
			manifestDocument.DocumentElement.AppendChild(newChild);
		}
	}

	private static XmlNode FindFirstChildWithName(XmlNode parent, string nodeName)
	{
		foreach (XmlNode childNode in parent.ChildNodes)
		{
			if (childNode.Name.Equals(nodeName))
			{
				return childNode;
			}
		}
		return null;
	}

	public void RemovePackageDependency(string pattern)
	{
		XmlElement element = GetElement("Dependencies", tryCreate: false);
		if (element == null)
		{
			return;
		}
		for (int num = element.ChildNodes.Count - 1; num >= 0; num--)
		{
			if (element.ChildNodes[num] is XmlElement { Name: "PackageDependency" } xmlElement)
			{
				string attribute = xmlElement.GetAttribute("Name");
				if (!string.IsNullOrEmpty(attribute) && Regex.IsMatch(attribute, pattern.Replace("*", "(.*)") + "$"))
				{
					element.RemoveChild(xmlElement);
				}
			}
		}
	}

	public string FindDependencyVersion(string namePrefix)
	{
		XmlElement element = GetElement("Dependencies", tryCreate: false);
		if (element != null)
		{
			foreach (XmlElement item in element)
			{
				if (item != null && item.Name == "PackageDependency")
				{
					string attribute = item.GetAttribute("Name");
					if (!string.IsNullOrEmpty(attribute) && attribute.StartsWith(namePrefix))
					{
						return item.GetAttribute("MinVersion");
					}
				}
			}
		}
		return string.Empty;
	}

	public IEnumerable<XmlElement> EnumerateDependencies()
	{
		XmlElement element = GetElement("Dependencies", tryCreate: false);
		if (element == null)
		{
			yield break;
		}
		foreach (XmlElement item in element)
		{
			if (item != null && item.Name == "PackageDependency")
			{
				yield return item;
			}
		}
	}

	public void AddPackageDependency(string name, string minVersion, string publisher)
	{
		XmlElement element = GetElement("Dependencies", tryCreate: true);
		if (element != null)
		{
			XmlElement xmlElement = manifestDocument.CreateElement("PackageDependency", element.NamespaceURI);
			xmlElement.SetAttribute("Name", name);
			xmlElement.SetAttribute("MinVersion", minVersion);
			xmlElement.SetAttribute("Publisher", publisher);
			element.AppendChild(xmlElement);
		}
	}
}
