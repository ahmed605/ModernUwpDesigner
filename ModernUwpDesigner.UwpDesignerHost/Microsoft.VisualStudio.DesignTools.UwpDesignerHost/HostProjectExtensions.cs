using Microsoft.VisualStudio.DesignTools.DesignerContract;
using System;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost;

internal static class HostProjectExtensions
{
	private static nint _fnptrGetProperty = 0;

	internal static unsafe string GetPropertyCompat(this IHostProject project, string property)
	{
		if (_fnptrGetProperty is 0)
		{
			_fnptrGetProperty = project.GetType().GetMethod(
				"GetProperty",
				System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
					?.MethodHandle.GetFunctionPointer() ?? (nint)0;
		}

		if (_fnptrGetProperty is 0)
		{
			throw new MissingMethodException(@"Cannot find method ""GetProperty"" in class ""HostProject"", please open an issue at https://github.com/ahmed605/ModernUwpDesigner");
		}

		return ((delegate*<IHostProject, string, string>)_fnptrGetProperty)(project, property);
	}

	internal static bool GetBoolProperty(this IHostProject project, string property, bool defaultValue = false)
	{
		var prop = project.GetPropertyCompat(property);
		if (string.IsNullOrEmpty(prop))
		{
			return defaultValue;
        }

		return prop.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
