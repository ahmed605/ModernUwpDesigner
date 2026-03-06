using System;
using System.Reflection;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace XSurfUwp;

internal static class ElementUtilities
{
	public static DependencyObject? GetVisualTreeAncestorOfType(DependencyObject element, System.Type type)
	{
		TypeInfo typeInfo = IntrospectionExtensions.GetTypeInfo(type);
		while (element != null)
		{
			if (typeInfo.IsAssignableFrom(IntrospectionExtensions.GetTypeInfo(((object)element).GetType())))
			{
				return element;
			}
			element = VisualTreeHelper.GetParent(element);
		}
		return null;
	}

	internal static bool IsFullyWithinBoundsOf(UIElement descendant, UIElement ancestorBounds)
	{
		Windows.Foundation.Rect rect = new Windows.Foundation.Rect(new Windows.Foundation.Point(0.0, 0.0), descendant.RenderSize);
		Windows.Foundation.Rect rect2 = new Windows.Foundation.Rect(new Windows.Foundation.Point(0.0, 0.0), ancestorBounds.RenderSize);
		GeneralTransform generalTransform = descendant.TransformToVisual(ancestorBounds);
		rect = generalTransform.TransformBounds(rect);
		Windows.Foundation.Rect rect3 = rect2;
		rect3.Intersect(rect);
		return rect3 == rect;
	}
}
