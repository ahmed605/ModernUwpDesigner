using System;
using Windows.UI.Xaml.Markup;

namespace XSurfUwp;

internal static class TypeHelper
{
	private class MarkupExtensionAccessor
	{
		public System.Type GetDesignInstanceType()
		{
			return typeof(DesignInstance);
		}

		public System.Type GetMarkupExtensionType()
		{
			return typeof(MarkupExtension);
		}
	}

	public static System.Type? GetDesignInstanceType()
	{
		try
		{
			return new MarkupExtensionAccessor().GetDesignInstanceType();
		}
		catch (TypeLoadException)
		{
		}
		return null;
	}

	public static System.Type? GetMarkupExtensionType()
	{
		try
		{
			return new MarkupExtensionAccessor().GetMarkupExtensionType();
		}
		catch (TypeLoadException)
		{
		}
		return null;
	}
}
