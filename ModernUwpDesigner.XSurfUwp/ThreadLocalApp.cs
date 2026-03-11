using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace XSurfUwp;

[Bindable]
public partial class ThreadLocalApp : DependencyObject
{
	public static DependencyProperty DeviceSizeProperty = DependencyProperty.Register("DeviceSize", typeof(Size), typeof(ThreadLocalApp), null);

	public Size DeviceSize
	{
		get
		{
			return (Size)GetValue(DeviceSizeProperty);
		}
		set
		{
			SetValue(DeviceSizeProperty, value);
		}
	}
}
