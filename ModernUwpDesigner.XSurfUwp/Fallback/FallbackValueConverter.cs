using System;
using Windows.UI.Xaml.Data;

namespace XSurfUwp.Fallback;

public partial class FallbackValueConverter : IValueConverter, IFallbackType
{
	object? IValueConverter.Convert(object value, System.Type targetType, object parameter, string language)
	{
		return null;
	}

	object? IValueConverter.ConvertBack(object value, System.Type targetType, object parameter, string language)
	{
		return null;
	}
}
