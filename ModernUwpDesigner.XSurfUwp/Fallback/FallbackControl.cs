using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using WinRT;

namespace XSurfUwp.Fallback;

[Bindable]
public partial class FallbackControl : ContentControl, IFallbackType
{
    [DynamicWindowsRuntimeCast(typeof(ControlTemplate))]
    public FallbackControl()
	{
		ResourceDictionary resources = Application.ContentWrapper.Resources;
        Template = (ControlTemplate)resources["__XSurfUwp.Fallback.FallbackControl.Template__"];
	}
}
