using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinRT;

namespace XSurfUwp.Fallback;

public partial class FallbackControl : ContentControl, IFallbackType
{
    [DynamicWindowsRuntimeCast(typeof(ControlTemplate))]
    public FallbackControl()
	{
		ResourceDictionary resources = Application.Current.ContentWrapper.Resources;
		base.Template = (ControlTemplate)resources["__XSurfUwp.Fallback.FallbackControl.Template__"];
	}
}
