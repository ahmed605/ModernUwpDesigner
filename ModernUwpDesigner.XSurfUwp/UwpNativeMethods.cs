using System.Runtime.InteropServices;
using System.Security;

namespace XSurfUwp;

internal class UwpNativeMethods
{
	//[DllImport("api-ms-win-core-winrt-l1-1-0.dll", CharSet = (CharSet)3)]
	//[SecurityCritical]
	//public static extern int RoActivateInstance([MarshalAs((UnmanagedType)47)] string activatableClassId, [MarshalAs((UnmanagedType)46)] out object instance);

	public static bool Succeeded(int hr)
	{
		return hr >= 0;
	}
}
