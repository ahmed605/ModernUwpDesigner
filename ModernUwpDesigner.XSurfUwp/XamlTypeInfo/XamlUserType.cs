#nullable disable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Markup;

namespace XSurfUwp.XSurfUwp_XamlTypeInfo;

internal partial class XamlUserType : XamlSystemBaseType
{
	private XamlTypeInfoProvider _provider;

	private IXamlType _baseType;

	private bool _isArray;

	private bool _isMarkupExtension;

	private bool _isBindable;

	private bool _isReturnTypeStub;

	private bool _isLocalType;

	private string _contentPropertyName;

	private string _itemTypeName;

	private string _keyTypeName;

	private Dictionary<string, string> _memberNames;

	private Dictionary<string, object> _enumValues;

	public override IXamlType BaseType => _baseType;

	public override bool IsArray => _isArray;

	public override bool IsCollection => CollectionAdd != null;

	public override bool IsConstructible => Activator != null;

	public override bool IsDictionary => DictionaryAdd != null;

	public override bool IsMarkupExtension => _isMarkupExtension;

	public override bool IsBindable => _isBindable;

	public override bool IsReturnTypeStub => _isReturnTypeStub;

	public override bool IsLocalType => _isLocalType;

	public override IXamlMember ContentProperty => _provider.GetMemberByLongName(_contentPropertyName);

	public override IXamlType ItemType => _provider.GetXamlTypeByName(_itemTypeName);

	public override IXamlType KeyType => _provider.GetXamlTypeByName(_keyTypeName);

	[field: CompilerGenerated]
	public Activator Activator
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		set;
	}

	[field: CompilerGenerated]
	public AddToCollection CollectionAdd
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		set;
	}

	[field: CompilerGenerated]
	public AddToDictionary DictionaryAdd
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		set;
	}

	public XamlUserType(XamlTypeInfoProvider provider, string fullName, System.Type fullType, IXamlType baseType)
		: base(fullName, fullType)
	{
		_provider = provider;
		_baseType = baseType;
	}

	public override IXamlMember GetMember(string name)
	{
		if (_memberNames == null)
		{
			return null;
		}
		string longMemberName = default(string);
		if (_memberNames.TryGetValue(name, out longMemberName))
		{
			return _provider.GetMemberByLongName(longMemberName);
		}
		return null;
	}

	public override object ActivateInstance()
	{
		return Activator();
	}

	public override void AddToMap(object instance, object key, object item)
	{
		DictionaryAdd(instance, key, item);
	}

	public override void AddToVector(object instance, object item)
	{
		CollectionAdd(instance, item);
	}

	public override void RunInitializer()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.RunClassConstructor(base.UnderlyingType.TypeHandle);
	}

	public override object CreateFromString(string input)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		if (_enumValues != null)
		{
			int num = 0;
			string[] array = input.Split(',', (StringSplitOptions)0);
			string[] array2 = array;
			object obj = default(object);
			foreach (string text in array2)
			{
				int num2 = 0;
				try
				{
					if (_enumValues.TryGetValue(text.Trim(), out obj))
					{
						num2 = Convert.ToInt32(obj);
					}
					else
					{
						try
						{
							num2 = Convert.ToInt32(text.Trim());
						}
						catch (FormatException)
						{
							var enumerator = _enumValues.Keys.GetEnumerator();
							try
							{
								while (enumerator.MoveNext())
								{
									string current = enumerator.Current;
									if (string.Compare(text.Trim(), current, (StringComparison)5) == 0 && _enumValues.TryGetValue(current.Trim(), out obj))
									{
										num2 = Convert.ToInt32(obj);
										break;
									}
								}
							}
							finally
							{
								((System.IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
							}
						}
					}
					num |= num2;
				}
				catch (FormatException)
				{
					throw new ArgumentException(input, base.FullName);
				}
			}
			return num;
		}
		throw new ArgumentException(input, base.FullName);
	}

	public void SetContentPropertyName(string contentPropertyName)
	{
		_contentPropertyName = contentPropertyName;
	}

	public void SetIsArray()
	{
		_isArray = true;
	}

	public void SetIsMarkupExtension()
	{
		_isMarkupExtension = true;
	}

	public void SetIsBindable()
	{
		_isBindable = true;
	}

	public void SetIsReturnTypeStub()
	{
		_isReturnTypeStub = true;
	}

	public void SetIsLocalType()
	{
		_isLocalType = true;
	}

	public void SetItemTypeName(string itemTypeName)
	{
		_itemTypeName = itemTypeName;
	}

	public void SetKeyTypeName(string keyTypeName)
	{
		_keyTypeName = keyTypeName;
	}

	public void AddMemberName(string shortName)
	{
		if (_memberNames == null)
		{
			_memberNames = new Dictionary<string, string>();
		}
		_memberNames.Add(shortName, base.FullName + "." + shortName);
	}

	public void AddEnumValue(string name, object value)
	{
		if (_enumValues == null)
		{
			_enumValues = new Dictionary<string, object>();
		}
		_enumValues.Add(name, value);
	}
}
