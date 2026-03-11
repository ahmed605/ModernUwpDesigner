using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace XSurfUwp.Fallback;

[Bindable]
public partial class FallbackStyleSelector : StyleSelector, IFallbackType
{
	protected override Style SelectStyleCore(object item, DependencyObject container)
	{
		return null;
	}
}
