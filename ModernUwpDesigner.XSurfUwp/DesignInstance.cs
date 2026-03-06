using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Markup;

namespace XSurfUwp;

public partial class DesignInstance : MarkupExtension
{
	[field: CompilerGenerated]
	public System.Type? Type
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	}

	[field: CompilerGenerated]
	public bool IsDesignTimeCreatable
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		set;
	}

	[field: CompilerGenerated]
	public bool CreateList
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		set;
	}

	public void SetTypeValue(object? value)
	{
		if (value is string text)
		{
			value = System.Type.GetType(text);
		}
		Type = (System.Type?)value;
	}

	protected override object? ProvideValue()
	{
		return CreateInstance();
	}

	public object? CreateInstance()
	{
		if (Type == (System.Type?)null || !IsDesignTimeCreatable)
		{
			return null;
		}
		object? obj;
		if (!CreateList)
		{
			obj = Activator.CreateInstance(Type);
		}
		else
		{
			System.Type type = typeof(List<>).MakeGenericType(new System.Type[1] { Type });
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
