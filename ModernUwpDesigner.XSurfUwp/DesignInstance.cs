using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace XSurfUwp;

[Bindable]
public partial class DesignInstance : MarkupExtension
{
    public Type Type { get; private set; }

    public bool IsDesignTimeCreatable { get; set; }

    public bool CreateList { get; set; }

    public void SetTypeValue(object value)
	{
		if (value is string text)
		{
			value = Type.GetType(text);
		}
		Type = (Type)value;
	}

	protected override object ProvideValue()
	{
		return CreateInstance();
	}

	public object CreateInstance()
	{
		if (Type == null || !IsDesignTimeCreatable)
		{
			return null;
		}

		object obj;
		if (!CreateList)
		{
			obj = Activator.CreateInstance(Type);
		}
		else
		{
            Type type = typeof(List<>).MakeGenericType([Type]);
			obj = Activator.CreateInstance(type);
			if (obj is IList list)
			{
				for (int i = 0; i < 3; i++)
				{
					list.Add(Activator.CreateInstance(Type));
				}
			}
		}

		return obj;
	}
}
