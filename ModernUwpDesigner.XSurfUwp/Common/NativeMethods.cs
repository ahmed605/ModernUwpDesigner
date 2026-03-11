using System;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.XSurface.Common;

internal static class NativeMethods
{
	public enum PROCESS_DPI_AWARENESS
	{
		PROCESS_DPI_UNAWARE,
		PROCESS_SYSTEM_DPI_AWARE,
		PROCESS_PER_MONITOR_DPI_AWARE
	}

	public const int S_OK = 0;

	public const int E_FAIL = -2147467259;

	public const int E_INVALIDARG = -2147024809;

	public const int HWND_MESSAGE = -3;

	public const uint WS_CHILD = 1073741824u;

	public const uint WS_POPUP = 2147483648u;

	public const uint WS_VISIBLE = 268435456u;

	public const uint INFINITE = 4294967295u;

	public const uint COWAIT_ENABLECALLREENTRANCY = 8u;

	public static readonly nint HWND_TOP = new(0);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetProcessDpiAwarenessContext(nint dpiContext);

	[DllImport("shcore.dll")]
	public static extern int SetProcessDpiAwareness(PROCESS_DPI_AWARENESS value);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetProcessDPIAware();

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	[DllImport("User32")]
	public static extern nint SetParent(nint hWnd, nint hWndParent);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}
