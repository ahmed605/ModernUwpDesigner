using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.Utility;
using Microsoft.VisualStudio.DesignTools.Utility.IO;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

public class MrtShadowCacheHelper
{
	private const string theme = "Light";

	private Dictionary<string, ProjectItemWithScore> mrtItemsCache;

	public UwpMrtResolver uwpMrtResolver;

	public Dictionary<string, ProjectItemWithScore> MrtItemsCache => mrtItemsCache;

	public MrtShadowCacheHelper()
	{
		mrtItemsCache = new Dictionary<string, ProjectItemWithScore>(StringComparer.OrdinalIgnoreCase);
		uwpMrtResolver = UwpMrtResolver.GetResolver();
	}

	public void UpdateMrtCache(IHostSourceItem item)
	{
		string relativePath = item.RelativePath;
		string fileOrDirectoryName = PathHelperBase.GetFileOrDirectoryName(item.Path);
		bool flag = uwpMrtResolver.FilePathContainsQualifiers(relativePath);
		bool flag2 = uwpMrtResolver.FileNameContainsQualifiers(fileOrDirectoryName);
		int num = 0;
		if (flag || flag2)
		{
			int num2 = (int)Math.Round(DpiHelper.GetPixelsPerDip(null) * 100.0);
			if (!flag && flag2)
			{
				num = UwpMrtResolver.CalculateResourceScore(fileOrDirectoryName, num2.ToString(CultureInfo.InvariantCulture), "Light");
			}
			else
			{
				string resourceFileName = UpdateFileNameWhenFolderIsQualifier(item.RelativePath, flag2);
				num = UwpMrtResolver.CalculateResourceScore(resourceFileName, num2.ToString(CultureInfo.InvariantCulture), "Light");
			}
			string key = uwpMrtResolver.GenerateRelativeDestinationPath(item.RelativePath);
			if (!mrtItemsCache.TryGetValue(key, out var value))
			{
				mrtItemsCache.Add(key, new ProjectItemWithScore(num, item));
			}
			else if (value.Score < num)
			{
				mrtItemsCache[key] = new ProjectItemWithScore(num, item);
			}
		}
	}

	public string UpdateFileNameWhenFolderIsQualifier(string itemRelativePath, bool fileNameHasQualifier)
	{
		string empty = string.Empty;
		StringBuilder stringBuilder = new StringBuilder();
		List<string> list = new List<string>();
		string extension = PathHelperBase.GetExtension(itemRelativePath);
		string[] array = itemRelativePath.Split('\\');
		for (int i = 0; i < array.Length; i++)
		{
			if (uwpMrtResolver.IsValidQualifier(array[i]))
			{
				list.Add(array[i]);
			}
			else if (i == array.Length - 1)
			{
				stringBuilder.Append(PathHelperBase.TrimExtension(array[i]));
			}
			else
			{
				stringBuilder.Append(array[i]).Append('\\');
			}
		}
		if (fileNameHasQualifier)
		{
			for (int j = 0; j < list.Count; j++)
			{
				stringBuilder.Append('_').Append(list[j]);
			}
		}
		else
		{
			for (int k = 0; k < list.Count; k++)
			{
				if (k == 0)
				{
					stringBuilder.Append('.').Append(list[k]);
				}
				else
				{
					stringBuilder.Append('_').Append(list[k]);
				}
			}
		}
		return stringBuilder.Append(extension).ToString();
	}
}
