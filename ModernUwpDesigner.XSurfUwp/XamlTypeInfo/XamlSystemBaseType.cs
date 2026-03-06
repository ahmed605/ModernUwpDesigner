using System;
using System.Diagnostics;
using Windows.UI.Xaml.Markup;

namespace XSurfUwp.XSurfUwp_XamlTypeInfo;

[DebuggerDisplay("{_fullName,nq}")]
internal partial class XamlSystemBaseType : IXamlType
{
	private string _fullName;

	private System.Type _underlyingType;

	public string FullName => _fullName;

	public System.Type UnderlyingType => _underlyingType;

	public virtual IXamlType BaseType
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public virtual IXamlMember ContentProperty
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public virtual bool IsArray
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public virtual bool IsCollection
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public virtual bool IsConstructible
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public virtual bool IsDictionary
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public virtual bool IsMarkupExtension
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public virtual bool IsBindable
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public virtual bool IsReturnTypeStub
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public virtual bool IsLocalType
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public virtual IXamlType ItemType
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public virtual IXamlType KeyType
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			throw new NotImplementedException();
		}
	}

	public XamlSystemBaseType(string fullName, System.Type underlyingType)
	{
		_fullName = fullName;
		_underlyingType = underlyingType;
	}

	public virtual IXamlMember GetMember(string name)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		throw new NotImplementedException();
	}

	public virtual object ActivateInstance()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		throw new NotImplementedException();
	}

	public virtual void AddToMap(object instance, object key, object item)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		throw new NotImplementedException();
	}

	public virtual void AddToVector(object instance, object item)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		throw new NotImplementedException();
	}

	public virtual void RunInitializer()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		throw new NotImplementedException();
	}

	public virtual object CreateFromString(string input)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		throw new NotImplementedException();
	}
}
