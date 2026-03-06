using System.Globalization;
using System.Text;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

public static class ProjectNameHelper
{
	public static string GetSafeProjectName(string projectName)
	{
		if (projectName == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder(projectName);
		for (int i = 0; i < stringBuilder.Length; i++)
		{
			UnicodeCategory unicodeCategory = char.GetUnicodeCategory(stringBuilder[i]);
			bool flag = unicodeCategory == UnicodeCategory.UppercaseLetter || unicodeCategory == UnicodeCategory.LowercaseLetter || unicodeCategory == UnicodeCategory.TitlecaseLetter || unicodeCategory == UnicodeCategory.OtherLetter || unicodeCategory == UnicodeCategory.LetterNumber || stringBuilder[i] == '_';
			bool flag2 = unicodeCategory == UnicodeCategory.NonSpacingMark || unicodeCategory == UnicodeCategory.SpacingCombiningMark || unicodeCategory == UnicodeCategory.ModifierLetter || unicodeCategory == UnicodeCategory.DecimalDigitNumber;
			if (i == 0)
			{
				if (!flag)
				{
					stringBuilder[i] = '_';
				}
			}
			else if (!(flag || flag2))
			{
				stringBuilder[i] = '_';
			}
		}
		return stringBuilder.ToString();
	}
}
