using Windows.UI.Xaml.Controls;

namespace XSurfUwp.Fallback;

public partial class FallbackGroupStyleSelector : GroupStyleSelector, IFallbackType
{
	protected override GroupStyle? SelectGroupStyleCore(object group, uint level)
	{
		return null;
	}
}
