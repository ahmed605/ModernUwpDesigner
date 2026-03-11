using System;
using System.Runtime.InteropServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using WinRT;

namespace XSurfUwp;

[Bindable]
public sealed partial class DT
{
	private DT() { }

	public Uri DummyUriForXamlTypeInfoMetadata { get; private set; }

	public static readonly DependencyProperty RootWidthProperty = DependencyProperty.RegisterAttached("RootWidth", typeof(double), typeof(DT), new PropertyMetadata(0.0 / 0.0, WidthPropertyChangedCallback));

	public static readonly DependencyProperty DesignWidthProperty = DependencyProperty.RegisterAttached("DesignWidth", typeof(double), typeof(DT), new PropertyMetadata(0.0 / 0.0, WidthPropertyChangedCallback));

	public static readonly DependencyProperty RuntimeWidthProperty = DependencyProperty.RegisterAttached("RuntimeWidth", typeof(double), typeof(DT), new PropertyMetadata(0.0 / 0.0, WidthPropertyChangedCallback));

	public static readonly DependencyProperty RootHeightProperty = DependencyProperty.RegisterAttached("RootHeight", typeof(double), typeof(DT), new PropertyMetadata(0.0 / 0.0, HeightPropertyChangedCallback));

	public static readonly DependencyProperty DesignHeightProperty = DependencyProperty.RegisterAttached("DesignHeight", typeof(double), typeof(DT), new PropertyMetadata(0.0 / 0.0, HeightPropertyChangedCallback));

	public static readonly DependencyProperty RuntimeHeightProperty = DependencyProperty.RegisterAttached("RuntimeHeight", typeof(double), typeof(DT), new PropertyMetadata(0.0 / 0.0, HeightPropertyChangedCallback));

	public static readonly DependencyProperty RuntimeSelectedIndexProperty = DependencyProperty.RegisterAttached("RuntimeSelectedIndex", typeof(int), typeof(DT), new PropertyMetadata(-1, SelectedIndexPropertyChangedCallback));

	public static readonly DependencyProperty DesignTimeSelectedIndexProperty = DependencyProperty.RegisterAttached("DesignTimeSelectedIndex", typeof(int), typeof(DT), new PropertyMetadata(-1, SelectedIndexPropertyChangedCallback));

	public static readonly DependencyProperty IsPivotItemSelectedProperty = DependencyProperty.RegisterAttached("IsPivotItemSelected", typeof(bool), typeof(DT), new PropertyMetadata(null, IsPivotItemSelectedPropertyChangedCallback));

	public static readonly DependencyProperty IsHiddenProperty = DependencyProperty.RegisterAttached("IsHidden", typeof(bool), typeof(DT), new PropertyMetadata(false, VisibilityChangedCallback));

	public static readonly DependencyProperty RuntimeVisibilityProperty = DependencyProperty.RegisterAttached("RuntimeVisibility", typeof(Visibility), typeof(DT), new PropertyMetadata(Visibility.Visible, VisibilityChangedCallback));

	public static readonly DependencyProperty IsTextEditingProperty = DependencyProperty.RegisterAttached("IsTextEditing", typeof(bool), typeof(DT), new PropertyMetadata(false, VisibilityChangedCallback));

	public static readonly DependencyProperty RuntimeOpacityProperty = DependencyProperty.RegisterAttached("RuntimeOpacity", typeof(double), typeof(DT), new PropertyMetadata(1.0, VisibilityChangedCallback));

	public static readonly DependencyProperty RuntimeIsHitTestVisibleProperty = DependencyProperty.RegisterAttached("RuntimeIsHitTestVisible", typeof(bool), typeof(DT), new PropertyMetadata(true, VisibilityChangedCallback));

	public static readonly DependencyProperty DesignUseLayoutRoundingProperty = DependencyProperty.RegisterAttached("DesignUseLayoutRounding", typeof(bool), typeof(DT), new PropertyMetadata(false, UseLayoutRoundingChangedCallback));

	public static readonly DependencyProperty RuntimeUseLayoutRoundingProperty = DependencyProperty.RegisterAttached("RuntimeUseLayoutRounding", typeof(bool), typeof(DT), new PropertyMetadata(false, UseLayoutRoundingChangedCallback));

	public static readonly DependencyProperty CopyTokenProperty = DependencyProperty.RegisterAttached("CopyToken", typeof(string), typeof(DT), null);

	public static readonly DependencyProperty IsFlyoutOpenProperty = DependencyProperty.RegisterAttached("IsFlyoutOpen", typeof(bool), typeof(DT), null);

	public static readonly DependencyProperty IsSplitViewPaneOpenProperty = DependencyProperty.RegisterAttached("IsSplitViewPaneOpen", typeof(bool), typeof(DT), new PropertyMetadata(false, IsSplitViewPaneOpenPropertyChangedCallback));

	public static readonly DependencyProperty RuntimeIsSplitViewPaneOpenProperty = DependencyProperty.RegisterAttached("RuntimeIsSplitViewPaneOpen", typeof(bool), typeof(DT), new PropertyMetadata(false, IsSplitViewPaneOpenPropertyChangedCallback));

	public static readonly DependencyProperty RuntimeBottomAppBarProperty = DependencyProperty.RegisterAttached("RuntimeBottomAppBar", typeof(AppBar), typeof(DT), new PropertyMetadata(null, BottomAppBarChangedCallback));

	public static readonly DependencyProperty RuntimeTopAppBarProperty = DependencyProperty.RegisterAttached("RuntimeTopAppBar", typeof(AppBar), typeof(DT), new PropertyMetadata(null, TopAppBarChangedCallback));

	public static readonly DependencyProperty RuntimeIsDropDownOpenProperty = DependencyProperty.RegisterAttached("RuntimeIsDropDownOpen", typeof(bool), typeof(DT), null);

	public static readonly DependencyProperty RuntimeIsPopupOpenProperty = DependencyProperty.RegisterAttached("RuntimeIsPopupOpen", typeof(bool), typeof(DT), null);

	public static readonly DependencyProperty RuntimeIsAppBarOpenProperty = DependencyProperty.RegisterAttached("RuntimeIsAppBarOpen", typeof(bool), typeof(DT), new PropertyMetadata(false, AppBarIsOpenCallback));

	public static readonly DependencyProperty DesignIsAppBarOpenProperty = DependencyProperty.RegisterAttached("DesignIsAppBarOpen", typeof(bool), typeof(DT), new PropertyMetadata(false, AppBarIsOpenCallback));

	public static readonly DependencyProperty IsHubSectionSelectedProperty = DependencyProperty.RegisterAttached("IsHubSectionSelected", typeof(bool), typeof(DT), new PropertyMetadata(null, IsHubSectionSelectedPropertyChangedCallback));

	public static readonly DependencyProperty CustomTagProperty = DependencyProperty.RegisterAttached("CustomTag", typeof(object), typeof(DT), null);

	public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(object), typeof(DT), null);

	public static readonly DependencyProperty IsZoomedInViewActiveProperty = DependencyProperty.RegisterAttached("IsZoomedInViewActive", typeof(bool), typeof(DT), new PropertyMetadata(true));

	public static readonly DependencyProperty DesignIsZoomedInViewActiveProperty = DependencyProperty.RegisterAttached("DesignIsZoomedInViewActive", typeof(bool), typeof(DT), new PropertyMetadata(true));

	public static readonly DependencyProperty RuntimeIsZoomedInViewActiveProperty = DependencyProperty.RegisterAttached("RuntimeIsZoomedInViewActive", typeof(bool), typeof(DT), new PropertyMetadata(true));
	
	public static readonly DependencyProperty ShouldDisableImplicitStyleProperty = DependencyProperty.RegisterAttached("ShouldDisableImplicitStyle", typeof(bool), typeof(DependencyObject), new(false));
	
	public static readonly DependencyProperty ResourceDictionarySourceProperty = DependencyProperty.RegisterAttached("ResourceDictionarySource", typeof(string), typeof(DependencyObject), new(null));

	[DllImport("Microsoft.VisualStudio.DesignTools.UwpTap.dll")]
	[return: MarshalAs(UnmanagedType.Error)]
	private static extern int UpdateResourceDictionarySource(nint dependencyObject, [MarshalAs(UnmanagedType.LPWStr)] string newSource);

	public static void SetRootWidth(DependencyObject dependencyObject, double value)
	{
		dependencyObject.SetValue(RootWidthProperty, value);
	}

	public static double GetRootWidth(DependencyObject dependencyObject)
	{
		return (double)dependencyObject.GetValue(RootWidthProperty);
	}

	public static void SetDesignWidth(DependencyObject dependencyObject, double value)
	{
		dependencyObject.SetValue(DesignWidthProperty, value);
	}

	public static double GetDesignWidth(DependencyObject dependencyObject)
	{
		return (double)dependencyObject.GetValue(DesignWidthProperty);
	}

	public static void SetRuntimeWidth(DependencyObject dependencyObject, double value)
	{
		dependencyObject.SetValue(RuntimeWidthProperty, value);
	}

	public static double GetRuntimeWidth(DependencyObject dependencyObject)
	{
		return (double)dependencyObject.GetValue(RuntimeWidthProperty);
	}

	public static void SetRootHeight(DependencyObject dependencyObject, double value)
	{
		dependencyObject.SetValue(RootHeightProperty, value);
	}

	public static double GetRootHeight(DependencyObject dependencyObject)
	{
		return (double)dependencyObject.GetValue(RootHeightProperty);
	}

	public static void SetDesignHeight(DependencyObject dependencyObject, double value)
	{
		dependencyObject.SetValue(DesignHeightProperty, value);
	}

	public static double GetDesignHeight(DependencyObject dependencyObject)
	{
		return (double)dependencyObject.GetValue(DesignHeightProperty);
	}

	public static void SetRuntimeHeight(DependencyObject dependencyObject, double value)
	{
		dependencyObject.SetValue(RuntimeHeightProperty, value);
	}

	public static double GetRuntimeHeight(DependencyObject dependencyObject)
	{
		return (double)dependencyObject.GetValue(RuntimeHeightProperty);
	}

	public static void SetRuntimeSelectedIndex(DependencyObject dependencyObject, object value)
	{
		dependencyObject.SetValue(RuntimeSelectedIndexProperty, value);
	}

	public static object GetRuntimeSelectedIndex(DependencyObject dependencyObject)
	{
		return dependencyObject.GetValue(RuntimeSelectedIndexProperty);
	}

	public static void SetDesignTimeSelectedIndex(DependencyObject dependencyObject, int value)
	{
		dependencyObject.SetValue(DesignTimeSelectedIndexProperty, value);
	}

	public static int GetDesignTimeSelectedIndex(DependencyObject dependencyObject)
	{
		return (int)dependencyObject.GetValue(DesignTimeSelectedIndexProperty);
	}

	public static void SetIsPivotItemSelected(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(IsPivotItemSelectedProperty, value);
	}

	public static bool GetIsPivotItemSelected(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(IsPivotItemSelectedProperty);
	}

	public static void SetIsHidden(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(IsHiddenProperty, value);
	}

	public static bool GetIsHidden(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(IsHiddenProperty);
	}

    [DynamicWindowsRuntimeCast(typeof(Visibility))]
    public static Visibility GetRuntimeVisibility(DependencyObject dependencyObject)
	{
		return (Visibility)dependencyObject.GetValue(RuntimeVisibilityProperty);
	}

	public static void SetRuntimeVisibility(DependencyObject dependencyObject, Visibility value)
	{
		dependencyObject.SetValue(RuntimeVisibilityProperty, value);
	}

	public static void SetIsTextEditing(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(IsTextEditingProperty, value);
	}

	public static bool GetIsTextEditing(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(IsTextEditingProperty);
	}

	public static double GetRuntimeOpacity(DependencyObject dependencyObject)
	{
		return (double)dependencyObject.GetValue(RuntimeOpacityProperty);
	}

	public static void SetRuntimeOpacity(DependencyObject dependencyObject, double value)
	{
		dependencyObject.SetValue(RuntimeOpacityProperty, value);
	}

	public static bool GetRuntimeIsHitTestVisible(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(RuntimeIsHitTestVisibleProperty);
	}

	public static void SetRuntimeIsHitTestVisible(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(RuntimeIsHitTestVisibleProperty, value);
	}

	public static void SetDesignUseLayoutRounding(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(DesignUseLayoutRoundingProperty, value);
	}

	public static bool GetDesignUseLayoutRounding(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(DesignUseLayoutRoundingProperty);
	}

	public static void SetRuntimeUseLayoutRounding(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(RuntimeUseLayoutRoundingProperty, value);
	}

	public static bool GetRuntimeUseLayoutRounding(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(RuntimeUseLayoutRoundingProperty);
	}

	public static void SetCopyToken(DependencyObject dependencyObject, string value)
	{
		dependencyObject.SetValue(CopyTokenProperty, value);
	}

	public static string GetCopyToken(DependencyObject dependencyObject)
	{
		return (string)dependencyObject.GetValue(CopyTokenProperty);
	}

	public static void SetIsFlyoutOpen(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(IsFlyoutOpenProperty, value);
	}

	public static bool GetIsFlyoutOpen(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(IsFlyoutOpenProperty);
	}

	public static void SetIsSplitViewPaneOpen(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(IsSplitViewPaneOpenProperty, value);
	}

	public static bool GetIsSplitViewPaneOpen(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(IsSplitViewPaneOpenProperty);
	}

	public static void SetRuntimeIsSplitViewPaneOpen(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(RuntimeIsSplitViewPaneOpenProperty, value);
	}

	public static bool GetRuntimeIsSplitViewPaneOpen(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(RuntimeIsSplitViewPaneOpenProperty);
	}

    [DynamicWindowsRuntimeCast(typeof(AppBar))]
    public static AppBar GetRuntimeBottomAppBar(DependencyObject dependencyObject)
	{
		return (AppBar)dependencyObject.GetValue(RuntimeBottomAppBarProperty);
	}

	public static void SetRuntimeBottomAppBar(DependencyObject dependencyObject, AppBar value)
	{
		dependencyObject.SetValue(RuntimeBottomAppBarProperty, value);
	}

    [DynamicWindowsRuntimeCast(typeof(AppBar))]
    public static AppBar GetRuntimeTopAppBar(DependencyObject dependencyObject)
	{
		return (AppBar)dependencyObject.GetValue(RuntimeTopAppBarProperty);
	}

	public static void SetRuntimeTopAppBar(DependencyObject dependencyObject, AppBar value)
	{
		dependencyObject.SetValue(RuntimeTopAppBarProperty, value);
	}

	public static bool GetRuntimeIsDropDownOpen(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(RuntimeIsDropDownOpenProperty);
	}

	public static void SetRuntimeIsDropDownOpen(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(RuntimeIsDropDownOpenProperty, value);
	}

	public static bool GetRuntimeIsPopupOpen(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(RuntimeIsPopupOpenProperty);
	}

	public static void SetRuntimeIsPopupOpen(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(RuntimeIsPopupOpenProperty, value);
	}

	public static bool GetRuntimeIsAppBarOpen(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(RuntimeIsAppBarOpenProperty);
	}

	public static void SetRuntimeIsAppBarOpen(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(RuntimeIsAppBarOpenProperty, value);
	}

	public static bool GetDesignIsAppBarOpen(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(DesignIsAppBarOpenProperty);
	}

	public static void SetDesignIsAppBarOpen(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(DesignIsAppBarOpenProperty, value);
	}

	public static void SetIsHubSectionSelected(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(IsHubSectionSelectedProperty, value);
	}

	public static bool GetIsHubSectionSelected(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(IsHubSectionSelectedProperty);
	}

	public static void SetCustomTag(DependencyObject dependencyObject, object value)
	{
	}

	public static object GetCustomTag(DependencyObject dependencyObject)
	{
		return null;
	}

	public static void SetSource(DependencyObject dependencyObject, object value)
	{
		dependencyObject.SetValue(SourceProperty, value);
	}

	public static object GetSource(DependencyObject dependencyObject)
	{
		return dependencyObject.GetValue(SourceProperty);
	}

	public static void SetResourceDictionarySource(ResourceDictionary resourceDictionary, string value)
	{
		int num = UpdateResourceDictionarySource(((IWinRTObject)resourceDictionary).NativeObject.ThisPtr, value);
		if (num != 0)
		{
			Marshal.ThrowExceptionForHR(num);
		}

		resourceDictionary.SetValue(ResourceDictionarySourceProperty, value);
    }

    public static string GetResourceDictionarySource(ResourceDictionary resourceDictionary)
    {
		return (string)resourceDictionary.GetValue(ResourceDictionarySourceProperty);
    }

    public static void SetShouldDisableImplicitStyle(FrameworkElement element, bool value)
	{
		ImplicitStyleSuppressor.SuppressImplicitStyle(element, value);
		element.SetValue(ShouldDisableImplicitStyleProperty, value);
	}

    public static bool GetShouldDisableImplicitStyle(FrameworkElement element)
    {
		return (bool)element.GetValue(ShouldDisableImplicitStyleProperty);
    }

    public static void SetZoomedInViewActive(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(IsZoomedInViewActiveProperty, value);
	}

	public static bool GetZoomedInViewActive(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(IsZoomedInViewActiveProperty);
	}

	public static void SetDesignIsZoomedInViewActive(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(DesignIsZoomedInViewActiveProperty, value);
	}

	public static bool GetDesignIsZoomedInViewActive(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(DesignIsZoomedInViewActiveProperty);
	}

	public static void SetRuntimeIsZoomedInViewActive(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(RuntimeIsZoomedInViewActiveProperty, value);
	}

	public static bool GetRuntimeIsZoomedInViewActive(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(RuntimeIsZoomedInViewActiveProperty);
	}

	private static void HeightPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UpdateWidthOrHeight(d, FrameworkElement.HeightProperty, RootHeightProperty, DesignHeightProperty, RuntimeHeightProperty);
	}

	private static void WidthPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UpdateWidthOrHeight(d, FrameworkElement.WidthProperty, RootWidthProperty, DesignWidthProperty, RuntimeWidthProperty);
	}

	private static void UpdateWidthOrHeight(DependencyObject d, DependencyProperty propertyToSet, DependencyProperty rootProperty, DependencyProperty designProperty, DependencyProperty runtimeProperty)
	{
        double num = double.NaN;

        if (ReadPropertyValue(d, designProperty) != DependencyProperty.UnsetValue)
        {
            num = (double)d.GetValue(designProperty);
        }

        if (double.IsNaN(num) && ReadPropertyValue(d, runtimeProperty) != DependencyProperty.UnsetValue)
        {
            num = (double)d.GetValue(runtimeProperty);
        }

        if (double.IsNaN(num) && ReadPropertyValue(d, rootProperty) != DependencyProperty.UnsetValue)
        {
            num = (double)d.GetValue(rootProperty);
        }

        if (double.IsNaN(num))
        {
            d.ClearValue(propertyToSet);
            return;
        }

        if (string.Equals(d.GetType().FullName, "Windows.UI.Xaml.Controls.PersonPicture") && num < 0.5)
        {
            num = 0.5;
        }

        d.SetValue(propertyToSet, num);
    }

    [DynamicWindowsRuntimeCast(typeof(Selector))]
    private static void SelectedIndexPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is Selector)
		{
			ReconcileRuntimeAndDesignTimeValues(d, Selector.SelectedIndexProperty, DesignTimeSelectedIndexProperty, RuntimeSelectedIndexProperty);
		}
	}

    [DynamicWindowsRuntimeCast(typeof(Pivot))]
    [DynamicWindowsRuntimeCast(typeof(PivotItem))]
    private static void IsPivotItemSelectedPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is PivotItem pivotItem && GetIsPivotItemSelected(pivotItem) && ElementUtilities.GetVisualTreeAncestorOfType(pivotItem, typeof(Pivot)) is Pivot pivot && pivot.Items.Contains(pivotItem))
		{
			pivot.SelectedItem = pivotItem;
		}
	}

	private static void VisibilityChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UpdateVisibility(d);
	}

    [DynamicWindowsRuntimeCast(typeof(Popup))]
    [DynamicWindowsRuntimeCast(typeof(Visibility))]
    private static void UpdateVisibility(DependencyObject d)
	{
        double num = GetRuntimeOpacity(d);
        Visibility visibility = GetRuntimeVisibility(d);
        bool flag = GetRuntimeIsHitTestVisible(d);
        bool isHidden = GetIsHidden(d);

        if (GetIsTextEditing(d))
        {
            num = 0.0;
        }

        if (isHidden && visibility != Visibility.Collapsed)
        {
            if (d is Popup)
            {
                visibility = Visibility.Collapsed;
            }
            else
            {
                num = 0.0;
                flag = false;
            }
        }

        double num2 = (double)d.GetValue(UIElement.OpacityProperty);
        Visibility visibility2 = (Visibility)d.GetValue(UIElement.VisibilityProperty);
        bool flag2 = (bool)d.GetValue(UIElement.IsHitTestVisibleProperty);

        if (num2 != num)
        {
            if (num == 1.0 && ReadPropertyValue(d, RuntimeOpacityProperty) == DependencyProperty.UnsetValue)
            {
                d.ClearValue(UIElement.OpacityProperty);
            }
            else
            {
                d.SetValue(UIElement.OpacityProperty, num);
            }
        }

        if (visibility2 != visibility)
        {
            if (visibility == Visibility.Visible && ReadPropertyValue(d, RuntimeVisibilityProperty) == DependencyProperty.UnsetValue)
            {
                d.ClearValue(UIElement.VisibilityProperty);
            }
            else
            {
                d.SetValue(UIElement.VisibilityProperty, visibility);
            }
        }

        if (flag2 != flag)
        {
            if (flag && ReadPropertyValue(d, RuntimeIsHitTestVisibleProperty) == DependencyProperty.UnsetValue)
            {
                d.ClearValue(UIElement.IsHitTestVisibleProperty);
            }
            else
            {
                d.SetValue(UIElement.IsHitTestVisibleProperty, flag);
            }
        }
    }

	private static void UseLayoutRoundingChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ReconcileRuntimeAndDesignTimeValues(d, UIElement.UseLayoutRoundingProperty, DesignUseLayoutRoundingProperty, RuntimeUseLayoutRoundingProperty);
	}

	private static void ReconcileRuntimeAndDesignTimeValues(DependencyObject d, DependencyProperty propertyToSet, DependencyProperty designProperty, DependencyProperty runtimeProperty, Func<object, object> designValueTransform = null)
	{
		object obj = ReadPropertyValue(d, designProperty);
		object obj2 = ReadPropertyValue(d, runtimeProperty);
		if (obj == DependencyProperty.UnsetValue && obj2 == DependencyProperty.UnsetValue)
		{
			d.ClearValue(propertyToSet);
		}
		else if (obj != DependencyProperty.UnsetValue)
		{
			object value = d.GetValue(designProperty);
			d.SetValue(propertyToSet, (designValueTransform != null) ? designValueTransform.Invoke(value) : value);
		}
		else
		{
			d.SetValue(propertyToSet, d.GetValue(runtimeProperty));
		}
	}

    [DynamicWindowsRuntimeCast(typeof(AppBar))]
    private static void BottomAppBarChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
        Application.Current.SetBottomAppBar(e.NewValue as AppBar);
    }

    [DynamicWindowsRuntimeCast(typeof(AppBar))]
    private static void TopAppBarChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
        Application.Current.SetTopAppBar(e.NewValue as AppBar);
    }

    [DynamicWindowsRuntimeCast(typeof(SplitView))]
    private static void IsSplitViewPaneOpenPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SplitView splitView)
		{
			bool isSplitViewPaneOpen = GetIsSplitViewPaneOpen(splitView);
			bool runtimeIsSplitViewPaneOpen = GetRuntimeIsSplitViewPaneOpen(splitView);
			splitView.SetValue(SplitView.IsPaneOpenProperty, isSplitViewPaneOpen || runtimeIsSplitViewPaneOpen);
		}
	}

    [DynamicWindowsRuntimeCast(typeof(AppBar))]
    private static void AppBarIsOpenCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is AppBar appBar)
		{
			bool designIsAppBarOpen = GetDesignIsAppBarOpen(appBar);
			bool runtimeIsAppBarOpen = GetRuntimeIsAppBarOpen(appBar);
			appBar.IsOpen = designIsAppBarOpen || runtimeIsAppBarOpen;
		}
	}

    [DynamicWindowsRuntimeCast(typeof(Hub))]
    [DynamicWindowsRuntimeCast(typeof(HubSection))]
    private static void IsHubSectionSelectedPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not HubSection hubSection || e.NewValue is not bool || !(bool)e.NewValue)
		{
			return;
		}
		for (DependencyObject parent = VisualTreeHelper.GetParent(d); parent != null; parent = VisualTreeHelper.GetParent(parent))
		{
			if (parent is Hub hub)
			{
                if (!hub.SectionsInView.Contains(hubSection) || !ElementUtilities.IsFullyWithinBoundsOf(hubSection, hub))
				{
					hub.ScrollToSection(hubSection);
				}
				break;
			}
		}
	}

    [DynamicWindowsRuntimeCast(typeof(Style))]
    [DynamicWindowsRuntimeCast(typeof(Setter))]
    private static object ReadPropertyValue(DependencyObject target, DependencyProperty dp)
	{
		object obj = target.ReadLocalValue(dp);
		if (obj == DependencyProperty.UnsetValue && target.GetValue(FrameworkElement.StyleProperty) is Style style)
        {
			foreach (var setterBase in style.Setters)
			{
                if (setterBase is Setter setter && setter.Property == dp)
                {
                    return setter.Value;
                }
            }
        }

        return obj;
	}
}
