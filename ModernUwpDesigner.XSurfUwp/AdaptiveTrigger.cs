using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace XSurfUwp;

public partial class AdaptiveTrigger : StateTriggerBase
{
	public static DependencyProperty MinWindowWidthProperty = DependencyProperty.Register("MinWindowWidth", typeof(double), typeof(AdaptiveTrigger), new PropertyMetadata(-1.0, OnPropertyChanged));

	public static DependencyProperty MinWindowHeightProperty = DependencyProperty.Register("MinWindowHeight", typeof(double), typeof(AdaptiveTrigger), new PropertyMetadata(-1.0, OnPropertyChanged));

	public static DependencyProperty DeviceSizeProperty = DependencyProperty.Register("DeviceSize", typeof(Windows.Foundation.Size), typeof(AdaptiveTrigger), new PropertyMetadata(new Windows.Foundation.Size(0.0, 0.0), OnPropertyChanged));

	public double MinWindowWidth
	{
		get
		{
			return (double)GetValue(MinWindowWidthProperty);
		}
		set
		{
			SetValue(MinWindowWidthProperty, value);
		}
	}

	public double MinWindowHeight
	{
		get
		{
			return (double)GetValue(MinWindowHeightProperty);
		}
		set
		{
			SetValue(MinWindowHeightProperty, value);
		}
	}

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

	public AdaptiveTrigger()
	{
		Binding binding = new Binding
		{
			Mode = BindingMode.OneWay,
			Source = Application.Current.Local,
			Path = new PropertyPath("DeviceSize")
		};
		BindingOperations.SetBinding(this, DeviceSizeProperty, binding);
	}

	private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((AdaptiveTrigger)d).UpdateActive();
	}

	private void UpdateActive()
	{
		Windows.Foundation.Size deviceSize = DeviceSize;
		bool active = (MinWindowWidth >= 0.0 && deviceSize.Width >= MinWindowWidth) || (MinWindowHeight >= 0.0 && deviceSize.Height >= MinWindowHeight);
		SetActive(active);
	}
}
