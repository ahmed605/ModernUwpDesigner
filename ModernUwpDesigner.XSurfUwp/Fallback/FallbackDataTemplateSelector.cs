using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace XSurfUwp.Fallback;

[Bindable]
public partial class FallbackDataTemplateSelector : DataTemplateSelector, IFallbackType
{
	protected override DataTemplate SelectTemplateCore(object item)
	{
		return null;
	}

	protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
	{
		return null;
	}
}
