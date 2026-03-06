using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using WinRT;

namespace XSurfUwp;

internal partial class ContentWrapper : UserControl
{
	[field: CompilerGenerated]
	public FrameworkElement BackgroundElement
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	}

	[field: CompilerGenerated]
	public FrameworkElement PanZoom
	{
		[CompilerGenerated]
		get;
	}

	[field: CompilerGenerated]
	public Border BottomAppBarHolder
	{
		[CompilerGenerated]
		get;
	}

	[field: CompilerGenerated]
	public Border TopAppBarHolder
	{
		[CompilerGenerated]
		get;
	}

	[field: CompilerGenerated]
	public Border ContentHolder
	{
		[CompilerGenerated]
		get;
	}

	[field: CompilerGenerated]
	public Rectangle HairlineBorder
	{
		[CompilerGenerated]
		get;
	}

	public ContentWrapper()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		base.Resources.Source = new Uri("ms-appx:///__SurfaceResources__.xaml");
		Canvas canvas = new Canvas();
		BackgroundElement = new Rectangle();
		BackgroundElement.Width = Window.Current.Bounds.Width;
		BackgroundElement.Height = Window.Current.Bounds.Height;
		((System.Collections.Generic.ICollection<UIElement>)(object)canvas.Children).Add((UIElement)BackgroundElement);
		Grid grid = new Grid
		{
			MaxWidth = 10000.0,
			MaxHeight = 10000.0,
			RowDefinitions = 
			{
				new RowDefinition
				{
					Height = Windows.UI.Xaml.GridLength.Auto
				},
				new RowDefinition(),
				new RowDefinition
				{
					Height = Windows.UI.Xaml.GridLength.Auto
				}
			}
		};
		TopAppBarHolder = new Border();
		Grid.SetRow(TopAppBarHolder, 0);
		ContentHolder = new Border();
		Grid.SetRow(ContentHolder, 1);
		BottomAppBarHolder = new Border();
		Grid.SetRow(BottomAppBarHolder, 2);
		HairlineBorder = new Rectangle
		{
			Stroke = new SolidColorBrush(Colors.Black),
			StrokeThickness = 1.0
		};
		Grid.SetRowSpan(HairlineBorder, 3);
		((System.Collections.Generic.ICollection<UIElement>)(object)grid.Children).Add((UIElement)ContentHolder);
		((System.Collections.Generic.ICollection<UIElement>)(object)grid.Children).Add((UIElement)TopAppBarHolder);
		((System.Collections.Generic.ICollection<UIElement>)(object)grid.Children).Add((UIElement)BottomAppBarHolder);
		((System.Collections.Generic.ICollection<UIElement>)(object)grid.Children).Add((UIElement)HairlineBorder);
		grid.RenderTransform = new CompositeTransform();
		grid.Opacity = 0.0;
		PanZoom = grid;
		((System.Collections.Generic.ICollection<UIElement>)(object)canvas.Children).Add((UIElement)grid);
		base.Content = canvas;
	}

    [DynamicWindowsRuntimeCast(typeof(Page))]
    public void SetContent(UIElement content)
	{
		ContentHolder.Child = content;
		if (content is Page && !ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
		{
			Binding binding = new Binding
			{
				Mode = BindingMode.OneWay,
				Source = content,
				Path = new PropertyPath("(Page.Background)")
			};
			ContentHolder.SetBinding(Border.BackgroundProperty, binding);
		}
	}

	public void ClearContent()
	{
		ContentHolder.Child = null;
		ContentHolder.ClearValue(Border.BackgroundProperty);
	}
}
