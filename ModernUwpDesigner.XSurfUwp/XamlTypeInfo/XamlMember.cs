#nullable disable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Markup;

namespace XSurfUwp.XSurfUwp_XamlTypeInfo;

[DebuggerDisplay("{_name,nq}:{_targetTypeName,nq} ({_typeName,nq})")]
internal partial class XamlMember : IXamlMember
{
	private XamlTypeInfoProvider _provider;

	private string _name;

	private bool _isAttachable;

	private bool _isDependencyProperty;

	private bool _isReadOnly;

	private string _typeName;

	private string _targetTypeName;

	public string Name => _name;

	public IXamlType Type => _provider.GetXamlTypeByName(_typeName);

	public IXamlType TargetType => _provider.GetXamlTypeByName(_targetTypeName);

	public bool IsAttachable => _isAttachable;

	public bool IsDependencyProperty => _isDependencyProperty;

	public bool IsReadOnly => _isReadOnly;

	[field: CompilerGenerated]
	public Getter Getter
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		set;
	}

	[field: CompilerGenerated]
	public Setter Setter
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		set;
	}

	public XamlMember(XamlTypeInfoProvider provider, string name, string typeName)
	{
		_name = name;
		_typeName = typeName;
		_provider = provider;
	}

	public void SetTargetTypeName(string targetTypeName)
	{
		_targetTypeName = targetTypeName;
	}

	public void SetIsAttachable()
	{
		_isAttachable = true;
	}

	public void SetIsDependencyProperty()
	{
		_isDependencyProperty = true;
	}

	public void SetIsReadOnly()
	{
		_isReadOnly = true;
	}

	public object GetValue(object instance)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (Getter != null)
		{
			return Getter(instance);
		}
		throw new InvalidOperationException("GetValue");
	}

	public void SetValue(object instance, object value)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (Setter != null)
		{
			Setter(instance, value);
			return;
		}
		throw new InvalidOperationException("SetValue");
	}
}
