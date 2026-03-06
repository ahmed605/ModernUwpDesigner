using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XSurfUwp.Fallback;

public partial class FallbackStyleSelector : StyleSelector, IFallbackType
{
	protected override Style? SelectStyleCore(object item, DependencyObject container)
	{
		return null;
	}
}
