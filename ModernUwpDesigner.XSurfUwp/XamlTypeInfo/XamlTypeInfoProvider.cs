#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using WinRT;
using XSurfUwp.Fallback;

namespace XSurfUwp.XSurfUwp_XamlTypeInfo;

internal class XamlTypeInfoProvider
{
	private Dictionary<string, IXamlType> _xamlTypeCacheByName = new Dictionary<string, IXamlType>();

	private Dictionary<System.Type, IXamlType> _xamlTypeCacheByType = new Dictionary<System.Type, IXamlType>();

	private Dictionary<string, IXamlMember> _xamlMembers = new Dictionary<string, IXamlMember>();

	private List<string> _typeNameTable;

	private List<System.Type> _typeTable;

	public IXamlType GetXamlTypeByType(System.Type type)
	{
		IXamlType xamlType = default(IXamlType);
		if (_xamlTypeCacheByType.TryGetValue(type, out xamlType))
		{
			return xamlType;
		}
		int num = LookupTypeIndexByType(type);
		if (num != -1)
		{
			xamlType = CreateXamlType(num);
		}
		if (xamlType != null)
		{
			_xamlTypeCacheByName.Add(xamlType.FullName, xamlType);
			_xamlTypeCacheByType.Add(xamlType.UnderlyingType, xamlType);
		}
		return xamlType;
	}

	public IXamlType GetXamlTypeByName(string typeName)
	{
		if (string.IsNullOrEmpty(typeName))
		{
			return null;
		}
		IXamlType xamlType = default(IXamlType);
		if (_xamlTypeCacheByName.TryGetValue(typeName, out xamlType))
		{
			return xamlType;
		}
		int num = LookupTypeIndexByName(typeName);
		if (num != -1)
		{
			xamlType = CreateXamlType(num);
		}
		if (xamlType != null)
		{
			_xamlTypeCacheByName.Add(xamlType.FullName, xamlType);
			_xamlTypeCacheByType.Add(xamlType.UnderlyingType, xamlType);
		}
		return xamlType;
	}

	public IXamlMember GetMemberByLongName(string longMemberName)
	{
		if (string.IsNullOrEmpty(longMemberName))
		{
			return null;
		}
		IXamlMember result = default(IXamlMember);
		if (_xamlMembers.TryGetValue(longMemberName, out result))
		{
			return result;
		}
		result = CreateXamlMember(longMemberName);
		if (result != null)
		{
			_xamlMembers.Add(longMemberName, result);
		}
		return result;
	}

	private void InitTypeTables()
	{
		System.Type markupExtensionType = TypeHelper.GetMarkupExtensionType();
		System.Type designInstanceType = TypeHelper.GetDesignInstanceType();
		_typeNameTable = new List<string>();
		_typeNameTable.Add("Windows.UI.Xaml.FrameworkElement");
		_typeNameTable.Add("Windows.UI.Xaml.ResourceDictionary");
		_typeNameTable.Add("XSurfUwp.Fallback.FallbackControl");
		_typeNameTable.Add("XSurfUwp.AdaptiveTrigger");
		_typeNameTable.Add("XSurfUwp.DT");
		_typeNameTable.Add("System.Uri");
		_typeNameTable.Add("Object");
		_typeNameTable.Add("Double");
		_typeNameTable.Add("Boolean");
		_typeNameTable.Add("Int32");
		_typeNameTable.Add("String");
		_typeNameTable.Add("Windows.UI.Xaml.Visibility");
		_typeNameTable.Add("System.Type");
		_typeNameTable.Add((markupExtensionType != (System.Type)null) ? "Windows.UI.Xaml.Markup.MarkupExtension" : string.Empty);
		_typeNameTable.Add((designInstanceType != (System.Type)null) ? "XSurfUwp.DesignInstance" : string.Empty);
		_typeNameTable.Add("XSurfUwp.Application");
		_typeNameTable.Add("Windows.UI.Xaml.Controls.AppBar");
		_typeNameTable.Add("XSurfUwp.ThreadLocalApp");
		_typeNameTable.Add("Windows.UI.Xaml.Application");
		_typeNameTable.Add("Windows.UI.Xaml.StateTriggerBase");
		_typeNameTable.Add("XSurfUwp.Fallback.FallbackValueConverter");
		_typeNameTable.Add("XSurfUwp.Fallback.FallbackDataTemplateSelector");
		_typeNameTable.Add("XSurfUwp.Fallback.FallbackGroupStyleSelector");
		_typeNameTable.Add("XSurfUwp.Fallback.FallbackStyleSelector");
		_typeNameTable.Add("XSurfUwp.Fallback.LayoutFaultFallbackControl");
		_typeTable = new List<System.Type>();
		_typeTable.Add(typeof(FrameworkElement));
		_typeTable.Add(typeof(ResourceDictionary));
		_typeTable.Add(typeof(FallbackControl));
		_typeTable.Add(typeof(AdaptiveTrigger));
		_typeTable.Add(typeof(DT));
		_typeTable.Add(typeof(Uri));
		_typeTable.Add(typeof(object));
		_typeTable.Add(typeof(double));
		_typeTable.Add(typeof(bool));
		_typeTable.Add(typeof(int));
		_typeTable.Add(typeof(string));
		_typeTable.Add(typeof(Visibility));
		_typeTable.Add(typeof(System.Type));
		_typeTable.Add(markupExtensionType);
		_typeTable.Add(designInstanceType);
		_typeTable.Add(typeof(Application));
		_typeTable.Add(typeof(AppBar));
		_typeTable.Add(typeof(ThreadLocalApp));
		_typeTable.Add(typeof(Windows.UI.Xaml.Application));
		_typeTable.Add(typeof(StateTriggerBase));
		_typeTable.Add(typeof(FallbackValueConverter));
		_typeTable.Add(typeof(FallbackDataTemplateSelector));
		_typeTable.Add(typeof(FallbackGroupStyleSelector));
		_typeTable.Add(typeof(FallbackStyleSelector));
		_typeTable.Add(typeof(LayoutFaultFallbackControl));
	}

	private void EnsureTypeTables()
	{
		if (_typeNameTable == null)
		{
			InitTypeTables();
		}
	}

	private int LookupTypeIndexByName(string typeName)
	{
		EnsureTypeTables();
		return _typeNameTable.IndexOf(typeName);
	}

	private int LookupTypeIndexByType(System.Type type)
	{
		EnsureTypeTables();
		return _typeTable.IndexOf(type);
	}

	private object Activate_XSurfUwp_Application()
	{
		ResourceDictionary resources = Application.Current.Resources;
		if (resources != null)
		{
			((System.Collections.Generic.ICollection<ResourceDictionary>)resources.MergedDictionaries).Clear();
			((System.Collections.Generic.ICollection<KeyValuePair<object, object>>)resources.ThemeDictionaries).Clear();
			((System.Collections.Generic.ICollection<KeyValuePair<object, object>>)resources).Clear();
		}
		return Application.Current;
	}

	private object Activate_XSurfUwp_FallbackControl()
	{
		return new FallbackControl();
	}

	private object Activate_XSurfUwp_LayoutFaultFallbackControl()
	{
		return new LayoutFaultFallbackControl();
	}

	private object Activate_XSurfUwp_FallbackValueConverter()
	{
		return new FallbackValueConverter();
	}

	private object Activate_XSurfUwp_FallbackDataTemplateSelector()
	{
		return new FallbackDataTemplateSelector();
	}

	private object Activate_XSurfUwp_FallbackGroupStyleSelector()
	{
		return new FallbackGroupStyleSelector();
	}

	private object Activate_XSurfUwp_FallbackStyleSelector()
	{
		return new FallbackStyleSelector();
	}

	private object Activate_XSurfUwp_AdaptiveTrigger()
	{
		return new AdaptiveTrigger();
	}

	private IXamlType CreateXamlType(int typeIndex)
	{
		XamlSystemBaseType result = null;
		string fullName = _typeNameTable[typeIndex];
		System.Type type = _typeTable[typeIndex];
		switch (typeIndex)
		{
		case 0:
			result = new XamlSystemBaseType(fullName, type);
			break;
		case 1:
			result = new XamlSystemBaseType(fullName, type);
			break;
		case 2:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Windows.UI.Xaml.FrameworkElement"));
			xamlUserType.Activator = Activate_XSurfUwp_FallbackControl;
			xamlUserType.SetIsLocalType();
			result = xamlUserType;
			break;
		}
		case 3:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Windows.UI.Xaml.StateTriggerBase"));
			xamlUserType.Activator = Activate_XSurfUwp_AdaptiveTrigger;
			xamlUserType.AddMemberName("MinWindowWidth");
			xamlUserType.AddMemberName("MinWindowHeight");
			xamlUserType.SetIsLocalType();
			result = xamlUserType;
			break;
		}
		case 4:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Object"));
			System.Collections.Generic.IEnumerator<MethodInfo> enumerator = IntrospectionExtensions.GetTypeInfo(typeof(DT)).DeclaredMethods.GetEnumerator();
			try
			{
				while (((System.Collections.IEnumerator)enumerator).MoveNext())
				{
					MethodInfo current = enumerator.Current;
					if (((MethodBase)current).IsStatic && ((MemberInfo)current).Name.StartsWith("Get"))
					{
						xamlUserType.AddMemberName(((MemberInfo)current).Name.Substring("Get".Length));
					}
				}
			}
			finally
			{
				((System.IDisposable)enumerator)?.Dispose();
			}
			xamlUserType.AddMemberName("ResourceDictionarySource");
			xamlUserType.AddMemberName("ShouldDisableImplicitStyle");
			xamlUserType.SetIsLocalType();
			result = xamlUserType;
			break;
		}
		case 5:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Object"));
			xamlUserType.SetIsReturnTypeStub();
			result = xamlUserType;
			break;
		}
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
			result = new XamlSystemBaseType(fullName, type);
			break;
		case 12:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Object"));
			xamlUserType.SetIsReturnTypeStub();
			result = xamlUserType;
			break;
		}
		case 13:
			result = new XamlSystemBaseType(fullName, type);
			break;
		case 14:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Windows.UI.Xaml.Markup.MarkupExtension"));
			xamlUserType.SetIsLocalType();
			xamlUserType.SetIsMarkupExtension();
			xamlUserType.AddMemberName("Type");
			xamlUserType.AddMemberName("IsDesignTimeCreatable");
			xamlUserType.AddMemberName("CreateList");
			xamlUserType.Activator = () => new DesignInstance();
			result = xamlUserType;
			break;
		}
		case 15:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Windows.UI.Xaml.Application"));
			xamlUserType.Activator = Activate_XSurfUwp_Application;
			xamlUserType.SetIsLocalType();
			result = xamlUserType;
			break;
		}
		case 16:
			result = new XamlSystemBaseType(fullName, type);
			break;
		case 17:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Windows.UI.Xaml.DependencyObject"));
			xamlUserType.AddMemberName("DeviceSize");
			xamlUserType.Activator = () => new ThreadLocalApp();
			xamlUserType.SetIsLocalType();
			result = xamlUserType;
			break;
		}
		case 18:
			result = new XamlSystemBaseType(fullName, type);
			break;
		case 19:
			result = new XamlSystemBaseType(fullName, type);
			break;
		case 20:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Object"));
			xamlUserType.Activator = Activate_XSurfUwp_FallbackValueConverter;
			xamlUserType.SetIsLocalType();
			result = xamlUserType;
			break;
		}
		case 21:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Windows.UI.Xaml.Controls.DataTemplateSelector"));
			xamlUserType.Activator = Activate_XSurfUwp_FallbackDataTemplateSelector;
			xamlUserType.SetIsLocalType();
			result = xamlUserType;
			break;
		}
		case 22:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Windows.UI.Xaml.Controls.GroupStyleSelector"));
			xamlUserType.Activator = Activate_XSurfUwp_FallbackGroupStyleSelector;
			xamlUserType.SetIsLocalType();
			result = xamlUserType;
			break;
		}
		case 23:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Windows.UI.Xaml.Controls.StyleSelector"));
			xamlUserType.Activator = Activate_XSurfUwp_FallbackStyleSelector;
			xamlUserType.SetIsLocalType();
			result = xamlUserType;
			break;
		}
		case 24:
		{
			XamlUserType xamlUserType = new XamlUserType(this, fullName, type, GetXamlTypeByName("Windows.UI.Xaml.FrameworkElement"));
			xamlUserType.Activator = Activate_XSurfUwp_LayoutFaultFallbackControl;
			xamlUserType.SetIsLocalType();
			result = xamlUserType;
			break;
		}
		}
		return result;
	}

    [DynamicWindowsRuntimeCast(typeof(FrameworkElement))]
    [DynamicWindowsRuntimeCast(typeof(ResourceDictionary))]
    private IXamlMember CreateXamlMember(string longMemberName)
	{
		XamlMember xamlMember = null;
		if (longMemberName == null)
		{
			goto IL_043b;
		}
		switch (longMemberName.Length)
		{
		case 34:
			break;
		case 28:
			goto IL_005b;
		case 45:
			goto IL_0070;
		case 36:
			goto IL_00af;
		case 38:
			goto IL_00c4;
		case 39:
			goto IL_00d9;
		case 40:
			goto IL_00ee;
		default:
			goto IL_043b;
		}
		char c = longMemberName[9];
		if (c != 'D')
		{
			if (c != 'T' || !(longMemberName == "XSurfUwp.ThreadLocalApp.DeviceSize"))
			{
				goto IL_043b;
			}
			xamlMember = new XamlMember(this, "DeviceSize", "Size");
			xamlMember.Getter = (object instance) => ((ThreadLocalApp)instance).DeviceSize;
			xamlMember.Setter = delegate(object instance, object value)
			{
				((ThreadLocalApp)instance).DeviceSize = (Windows.Foundation.Size)value;
			};
		}
		else
		{
			if (!(longMemberName == "XSurfUwp.DesignInstance.CreateList"))
			{
				goto IL_043b;
			}
			xamlMember = new XamlMember(this, "CreateList", "Boolean");
			xamlMember.SetTargetTypeName("Object");
			xamlMember.Getter = (object instance) => ((DesignInstance)instance).CreateList;
			xamlMember.Setter = delegate(object instance, object value)
			{
				((DesignInstance)instance).CreateList = (bool)value;
			};
		}
		goto IL_0533;
		IL_00d9:
		if (!(longMemberName == "XSurfUwp.AdaptiveTrigger.MinWindowWidth"))
		{
			goto IL_043b;
		}
		xamlMember = new XamlMember(this, "MinWindowWidth", "Double");
		xamlMember.Getter = (object instance) => ((AdaptiveTrigger)instance).MinWindowWidth;
		xamlMember.Setter = delegate(object instance, object value)
		{
			((AdaptiveTrigger)instance).MinWindowWidth = (double)value;
		};
		goto IL_0533;
		IL_005b:
		if (!(longMemberName == "XSurfUwp.DesignInstance.Type"))
		{
			goto IL_043b;
		}
		xamlMember = new XamlMember(this, "Type", "System.Type");
		xamlMember.Getter = (object instance) => ((DesignInstance)instance).Type;
		xamlMember.Setter = delegate(object instance, object value)
		{
			((DesignInstance)instance).SetTypeValue(value);
		};
		goto IL_0533;
		IL_00ee:
		if (!(longMemberName == "XSurfUwp.AdaptiveTrigger.MinWindowHeight"))
		{
			goto IL_043b;
		}
		xamlMember = new XamlMember(this, "MinWindowHeight", "Double");
		xamlMember.Getter = (object instance) => ((AdaptiveTrigger)instance).MinWindowHeight;
		xamlMember.Setter = delegate(object instance, object value)
		{
			((AdaptiveTrigger)instance).MinWindowHeight = (double)value;
		};
		goto IL_0533;
		IL_00c4:
		if (!(longMemberName == "XSurfUwp.DT.ShouldDisableImplicitStyle"))
		{
			goto IL_043b;
		}
		xamlMember = new XamlMember(this, "ShouldDisableImplicitStyle", "Boolean");
		xamlMember.SetTargetTypeName("Windows.UI.Xaml.FrameworkElement");
		xamlMember.SetIsAttachable();
		xamlMember.Getter = (object instance) => (object)null;
		xamlMember.Setter = delegate(object instance, object value)
		{
			DT.SetShouldDisableImplicitStyle((FrameworkElement)instance, (bool)value);
		};
		goto IL_0533;
		IL_0070:
		if (!(longMemberName == "XSurfUwp.DesignInstance.IsDesignTimeCreatable"))
		{
			goto IL_043b;
		}
		xamlMember = new XamlMember(this, "IsDesignTimeCreatable", "Boolean");
		xamlMember.SetTargetTypeName("Object");
		xamlMember.Getter = (object instance) => ((DesignInstance)instance).IsDesignTimeCreatable;
		xamlMember.Setter = delegate(object instance, object value)
		{
			((DesignInstance)instance).IsDesignTimeCreatable = (bool)value;
		};
		goto IL_0533;
		IL_0533:
		return xamlMember;
		IL_043b:
		if (longMemberName.StartsWith("XSurfUwp.DT."))
		{
			string text = longMemberName.Substring("XSurfUwp.DT.".Length);
			MethodInfo setter = IntrospectionExtensions.GetTypeInfo(typeof(DT)).GetDeclaredMethod("Set" + text);
			MethodInfo getter = IntrospectionExtensions.GetTypeInfo(typeof(DT)).GetDeclaredMethod("Get" + text);
			if (setter == (MethodInfo)null || getter == (MethodInfo)null || ((MethodBase)setter).GetParameters().Length != 2)
			{
				return null;
			}
			xamlMember = new XamlMember(this, text, GetWinrtProjectionName(((MethodBase)setter).GetParameters()[1].ParameterType));
			xamlMember.SetTargetTypeName("Windows.UI.Xaml.DependencyObject");
			xamlMember.SetIsAttachable();
			xamlMember.Getter = (object instance) => ((MethodBase)getter).Invoke((object)null, new object[1] { instance });
			xamlMember.Setter = delegate(object instance, object value)
			{
				((MethodBase)setter).Invoke((object)null, new object[2] { instance, value });
			};
		}
		goto IL_0533;
		IL_00af:
		if (!(longMemberName == "XSurfUwp.DT.ResourceDictionarySource"))
		{
			goto IL_043b;
		}
		xamlMember = new XamlMember(this, "ResourceDictionarySource", "String");
		xamlMember.SetTargetTypeName("Windows.UI.Xaml.ResourceDictionary");
		xamlMember.SetIsAttachable();
		xamlMember.Getter = (object instance) => (object)null;
		xamlMember.Setter = delegate(object instance, object value)
		{
			DT.SetResourceDictionarySource((ResourceDictionary)instance, (string)value);
		};
		goto IL_0533;
	}

	private string GetWinrtProjectionName(System.Type type)
	{
		string text = type.FullName;
		if (type.IsNested)
		{
			text = text.Replace('+', '.');
		}
		if (text.StartsWith("System."))
		{
			text = text.Substring("System.".Length);
		}
		return text;
	}
}
