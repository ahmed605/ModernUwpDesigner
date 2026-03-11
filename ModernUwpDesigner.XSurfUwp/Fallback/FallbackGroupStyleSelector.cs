using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace XSurfUwp.Fallback;

[Bindable]
public partial class FallbackGroupStyleSelector : GroupStyleSelector, IFallbackType
{
	protected override GroupStyle SelectGroupStyleCore(object group, uint level)
	{
		return null;
	}
}
