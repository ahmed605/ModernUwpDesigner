using Windows.Foundation;
using Windows.UI.Xaml;

namespace XSurfUwp;

public partial class ThreadLocalApp : DependencyObject
{
	public static DependencyProperty DeviceSizeProperty = DependencyProperty.Register("DeviceSize", typeof(Windows.Foundation.Size), typeof(ThreadLocalApp), null);

	public Windows.Foundation.Size DeviceSize
	{
		get
		{
			return (Windows.Foundation.Size)GetValue(DeviceSizeProperty);
		}
		set
		{
			SetValue(DeviceSizeProperty, value);
		}
	}
}
