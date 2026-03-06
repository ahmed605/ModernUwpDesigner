using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace Microsoft.Internal.Performance;

internal sealed class CodeMarkers
{
	private static class NativeMethods
	{
		[DllImport("Microsoft.Internal.Performance.CodeMarkers.dll", EntryPoint = "PerfCodeMarker")]
		public static extern void TestDllPerfCodeMarker(IntPtr nTimerID, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] aUserParams, IntPtr cbParams);

		[DllImport("Microsoft.Internal.Performance.CodeMarkers.dll", EntryPoint = "PerfCodeMarker")]
		public static extern void TestDllPerfCodeMarkerString(IntPtr nTimerID, [MarshalAs(UnmanagedType.LPWStr)] string aUserParams, IntPtr cbParams);

		[DllImport("Microsoft.VisualStudio.CodeMarkers.dll", EntryPoint = "PerfCodeMarker")]
		public static extern void ProductDllPerfCodeMarker(IntPtr nTimerID, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] aUserParams, IntPtr cbParams);

		[DllImport("Microsoft.VisualStudio.CodeMarkers.dll", EntryPoint = "PerfCodeMarker")]
		public static extern void ProductDllPerfCodeMarkerString(IntPtr nTimerID, [MarshalAs(UnmanagedType.LPWStr)] string aUserParams, IntPtr cbParams);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern ushort FindAtom([MarshalAs(UnmanagedType.LPWStr)] string lpString);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);
	}

	private enum State
	{
		Enabled,
		Disabled,
		DisabledDueToDllImportException
	}

	public static readonly Microsoft.Internal.Performance.CodeMarkers Instance = new Microsoft.Internal.Performance.CodeMarkers();

	private const string AtomName = "VSCodeMarkersEnabled";

	private const string TestDllName = "Microsoft.Internal.Performance.CodeMarkers.dll";

	private const string ProductDllName = "Microsoft.VisualStudio.CodeMarkers.dll";

	private State state;

	private RegistryView registryView;

	private string regroot;

	private bool? shouldUseTestDll;

	private static readonly byte[] CorrelationMarkBytes = new Guid("AA10EEA0-F6AD-4E21-8865-C427DAE8EDB9").ToByteArray();

	public bool IsEnabled => state == State.Enabled;

	public bool ShouldUseTestDll
	{
		get
		{
			if (!shouldUseTestDll.HasValue)
			{
				try
				{
					if (regroot == null)
					{
						shouldUseTestDll = NativeMethods.GetModuleHandle("Microsoft.VisualStudio.CodeMarkers.dll") == IntPtr.Zero;
					}
					else
					{
						shouldUseTestDll = UsePrivateCodeMarkers(regroot, registryView);
					}
				}
				catch (Exception)
				{
					shouldUseTestDll = true;
				}
			}
			return shouldUseTestDll.Value;
		}
	}

	private CodeMarkers()
	{
		state = ((NativeMethods.FindAtom("VSCodeMarkersEnabled") == 0) ? State.Disabled : State.Enabled);
	}

	public bool CodeMarker(int nTimerID)
	{
		if (!IsEnabled)
		{
			return false;
		}
		try
		{
			if (ShouldUseTestDll)
			{
				NativeMethods.TestDllPerfCodeMarker(new IntPtr(nTimerID), null, new IntPtr(0));
			}
			else
			{
				NativeMethods.ProductDllPerfCodeMarker(new IntPtr(nTimerID), null, new IntPtr(0));
			}
		}
		catch (DllNotFoundException)
		{
			state = State.DisabledDueToDllImportException;
			return false;
		}
		return true;
	}

	public bool CodeMarkerEx(int nTimerID, byte[] aBuff)
	{
		if (!IsEnabled)
		{
			return false;
		}
		if (aBuff == null)
		{
			throw new ArgumentNullException("aBuff");
		}
		try
		{
			if (ShouldUseTestDll)
			{
				NativeMethods.TestDllPerfCodeMarker(new IntPtr(nTimerID), aBuff, new IntPtr(aBuff.Length));
			}
			else
			{
				NativeMethods.ProductDllPerfCodeMarker(new IntPtr(nTimerID), aBuff, new IntPtr(aBuff.Length));
			}
		}
		catch (DllNotFoundException)
		{
			state = State.DisabledDueToDllImportException;
			return false;
		}
		return true;
	}

	public void SetStateDLLException()
	{
		state = State.DisabledDueToDllImportException;
	}

	public bool CodeMarkerEx(int nTimerID, Guid guidData)
	{
		return CodeMarkerEx(nTimerID, guidData.ToByteArray());
	}

	public bool CodeMarkerEx(int nTimerID, string stringData)
	{
		if (!IsEnabled)
		{
			return false;
		}
		if (stringData == null)
		{
			throw new ArgumentNullException("stringData");
		}
		try
		{
			int value = Encoding.Unicode.GetByteCount(stringData) + 2;
			if (ShouldUseTestDll)
			{
				NativeMethods.TestDllPerfCodeMarkerString(new IntPtr(nTimerID), stringData, new IntPtr(value));
			}
			else
			{
				NativeMethods.ProductDllPerfCodeMarkerString(new IntPtr(nTimerID), stringData, new IntPtr(value));
			}
		}
		catch (DllNotFoundException)
		{
			state = State.DisabledDueToDllImportException;
			return false;
		}
		return true;
	}

	internal static byte[] StringToBytesZeroTerminated(string stringData)
	{
		Encoding unicode = Encoding.Unicode;
		int byteCount = unicode.GetByteCount(stringData);
		byte[] array = new byte[byteCount + 2];
		unicode.GetBytes(stringData, 0, stringData.Length, array, 0);
		return array;
	}

	public static byte[] AttachCorrelationId(byte[] buffer, Guid correlationId)
	{
		if (correlationId == Guid.Empty)
		{
			return buffer;
		}
		byte[] array = correlationId.ToByteArray();
		byte[] array2 = new byte[CorrelationMarkBytes.Length + array.Length + ((buffer != null) ? buffer.Length : 0)];
		CorrelationMarkBytes.CopyTo(array2, 0);
		array.CopyTo(array2, CorrelationMarkBytes.Length);
		buffer?.CopyTo(array2, CorrelationMarkBytes.Length + array.Length);
		return array2;
	}

	public bool CodeMarkerEx(int nTimerID, uint uintData)
	{
		return CodeMarkerEx(nTimerID, BitConverter.GetBytes(uintData));
	}

	public bool CodeMarkerEx(int nTimerID, ulong ulongData)
	{
		return CodeMarkerEx(nTimerID, BitConverter.GetBytes(ulongData));
	}

	private static bool UsePrivateCodeMarkers(string regRoot, RegistryView registryView)
	{
		if (regRoot == null)
		{
			throw new ArgumentNullException("regRoot");
		}
		using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
		{
			using RegistryKey registryKey2 = registryKey.OpenSubKey(regRoot + "\\Performance");
			if (registryKey2 != null)
			{
				string value = registryKey2.GetValue(string.Empty).ToString();
				return !string.IsNullOrEmpty(value);
			}
		}
		return false;
	}
}
