using System;
using Windows.UI.Xaml.Data;

namespace XSurfUwp.Fallback;

[Bindable]
public partial class FallbackValueConverter : IValueConverter, IFallbackType
{
	object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
	{
		return null;
	}

	object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
	{
		return null;
	}
}
