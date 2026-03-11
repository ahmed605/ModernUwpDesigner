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

[Bindable]
internal partial class ContentWrapper : UserControl
{
    public FrameworkElement BackgroundElement { get; private set; }

    public FrameworkElement PanZoom { get; }

    public Border BottomAppBarHolder { get; }

    public Border TopAppBarHolder { get; }

    public Border ContentHolder { get; }

    public Rectangle HairlineBorder { get; }

    public ContentWrapper()
	{
        Resources.Source = new Uri("ms-appx:///__SurfaceResources__.xaml");

        BackgroundElement = new Rectangle
        {
            Width = Window.Current.Bounds.Width,
            Height = Window.Current.Bounds.Height
        };

		Canvas canvas = new();
        canvas.Children.Add(BackgroundElement);

		Grid grid = new()
        {
			MaxWidth = 10000.0,
			MaxHeight = 10000.0,
			RowDefinitions = 
			{
				new RowDefinition
				{
					Height = GridLength.Auto
				},
				new RowDefinition(),
				new RowDefinition
				{
					Height = GridLength.Auto
				}
			}
		};

		TopAppBarHolder = new();
		Grid.SetRow(TopAppBarHolder, 0);

		ContentHolder = new();
		Grid.SetRow(ContentHolder, 1);

		BottomAppBarHolder = new();
		Grid.SetRow(BottomAppBarHolder, 2);

		HairlineBorder = new Rectangle
		{
			Stroke = new SolidColorBrush(Colors.Black),
			StrokeThickness = 1.0
		};

		Grid.SetRowSpan(HairlineBorder, 3);

		grid.Children.Add(ContentHolder);
		grid.Children.Add(TopAppBarHolder);
		grid.Children.Add(BottomAppBarHolder);
		grid.Children.Add(HairlineBorder);

		grid.RenderTransform = new CompositeTransform();
		grid.Opacity = 0.0;

		PanZoom = grid;

		canvas.Children.Add(grid);
        Content = canvas;
	}

    [DynamicWindowsRuntimeCast(typeof(Page))]
    public void SetContent(UIElement content)
	{
		ContentHolder.Child = content;
		if (content is Page && !ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
		{
			Binding binding = new()
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
