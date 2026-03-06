using System;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost;

internal static class NativeMethods
{
	[DllImport("kernel32.dll")]
	public static extern int GetProcessId(IntPtr process);
}
