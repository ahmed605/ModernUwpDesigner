using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WinRT;

namespace XSurfUwp;

[Bindable]
public sealed partial class ImplicitStyleSuppressor : DependencyObject, IDisposable
{
	private static readonly DependencyProperty styleSuppressorProperty = DependencyProperty.RegisterAttached("StyleSuppressor", typeof(ImplicitStyleSuppressor), typeof(ImplicitStyleSuppressor), new PropertyMetadata(null));

	public static readonly DependencyProperty StyleSinkProperty = DependencyProperty.Register("StyleSink", typeof(Style), typeof(ImplicitStyleSuppressor), new PropertyMetadata(null, OnStyleChanged));

	private FrameworkElement element;

	private Style emptyStyle;

	private bool ignoreStyleChange;

    public Style StyleSink
    {
        [DynamicWindowsRuntimeCast(typeof(Style))]
        get
        {
            return (Style)GetValue(StyleSinkProperty);
        }
    }

    public static void SuppressImplicitStyle(FrameworkElement element, bool shouldSuppress)
	{
		ImplicitStyleSuppressor implicitStyleSuppressor = (ImplicitStyleSuppressor)element.GetValue(styleSuppressorProperty);
		if (implicitStyleSuppressor != null && !shouldSuppress)
		{
			implicitStyleSuppressor.Dispose();
		}
		else if (implicitStyleSuppressor == null && shouldSuppress)
		{
			implicitStyleSuppressor = new ImplicitStyleSuppressor(element);
			element.SetValue(styleSuppressorProperty, implicitStyleSuppressor);
		}
	}

	private ImplicitStyleSuppressor(FrameworkElement element)
	{
		this.element = element;
		emptyStyle = new Style
		{
			TargetType = typeof(FrameworkElement)
		};
		Binding binding = new Binding
		{
			Source = element,
			Path = new PropertyPath("Style")
		};
		BindingOperations.SetBinding(this, StyleSinkProperty, binding);
		OnElementStyleChanged();
	}

    [DynamicWindowsRuntimeCast(typeof(Style))]
    public void Dispose()
	{
		if (element != null)
		{
			ignoreStyleChange = true;
			element.ClearValue(StyleSinkProperty);
			element.ClearValue(styleSuppressorProperty);
			if (element.ReadLocalValue(FrameworkElement.StyleProperty) is Style style && style == emptyStyle)
			{
				element.ClearValue(FrameworkElement.StyleProperty);
			}
			element = null;
		}
	}

    [DynamicWindowsRuntimeCast(typeof(Style))]
    private void OnElementStyleChanged()
	{
		if (ignoreStyleChange)
		{
			return;
		}
		ignoreStyleChange = true;
		try
		{
			object obj = element?.ReadLocalValue(FrameworkElement.StyleProperty);
			if (obj is not Style)
			{
				element?.SetValue(FrameworkElement.StyleProperty, emptyStyle);
			}
		}
		catch
		{
		}
		ignoreStyleChange = false;
	}

	private static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ImplicitStyleSuppressor suppressor = (ImplicitStyleSuppressor)d;
        suppressor?.element?.Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, suppressor.OnElementStyleChanged);
	}
}
