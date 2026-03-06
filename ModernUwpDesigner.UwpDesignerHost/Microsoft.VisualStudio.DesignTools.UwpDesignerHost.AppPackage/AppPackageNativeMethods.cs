using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32.SafeHandles;
using Windows.System;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.AppPackage;

internal static class AppPackageNativeMethods
{
	[Flags]
	internal enum CLSCTX : uint
	{
		CLSCTX_INPROC_SERVER = 1u,
		CLSCTX_INPROC_HANDLER = 2u,
		CLSCTX_LOCAL_SERVER = 4u,
		CLSCTX_INPROC_SERVER16 = 8u,
		CLSCTX_REMOTE_SERVER = 0x10u,
		CLSCTX_INPROC_HANDLER16 = 0x20u,
		CLSCTX_RESERVED1 = 0x40u,
		CLSCTX_RESERVED2 = 0x80u,
		CLSCTX_RESERVED3 = 0x100u,
		CLSCTX_RESERVED4 = 0x200u,
		CLSCTX_NO_CODE_DOWNLOAD = 0x400u,
		CLSCTX_RESERVED5 = 0x800u,
		CLSCTX_NO_CUSTOM_MARSHAL = 0x1000u,
		CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000u,
		CLSCTX_NO_FAILURE_LOG = 0x4000u,
		CLSCTX_DISABLE_AAA = 0x8000u,
		CLSCTX_ENABLE_AAA = 0x10000u,
		CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000u,
		CLSCTX_INPROC = 3u,
		CLSCTX_SERVER = 0x15u,
		CLSCTX_ALL = 0x17u
	}

	internal enum ActivateOptions
	{
		None,
		DesignMode
	}

	[ComImport]
	[Guid("2e941141-7f97-4756-ba1d-9decde894a3d")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IApplicationActivationManager
	{
		void ActivateApplication([In][MarshalAs(UnmanagedType.LPWStr)] string appUserModelId, [In][MarshalAs(UnmanagedType.LPWStr)] string activationContext, [In][MarshalAs(UnmanagedType.I4)] ActivateOptions options, out int processId);
	}

	[ComImport]
	[Guid("FC4801A3-2BA9-11CF-A229-00AA003D7352")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IObjectWithSite
	{
		void SetSite([In][MarshalAs(UnmanagedType.IUnknown)] object site);

		void GetSite([In] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object site);
	}

	[ComImport]
	[Guid("5842a140-ff9f-4166-8f5c-62f5b7b0c781")]
	internal class AppxFactory
	{
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("beb94909-e451-438b-b5a7-d79e767b75d8")]
	internal interface IAppxFactory
	{
		void CreatePackageWriter();

		void CreatePackageReader([In] IStream inputStream, out IAppxPackageReader packageReader);

		void CreateManifestReader([In] IStream inputStream, out IAppxManifestReader manifestReader);

		void CreateBlockMapReader();

		void CreateValidatedBlockMapReader();
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("b5c49650-99bc-481c-9a34-3d53a4106708")]
	internal interface IAppxPackageReader
	{
		void GetBlockMap();

		void GetFootprintFile();

		void GetPayloadFile();

		void GetPayloadFiles();

		void GetManifest(out IAppxManifestReader manifestReader);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("4e1bd148-55a0-4480-a3d1-15544710637c")]
	internal interface IAppxManifestReader
	{
		void GetPackageId(out IAppxManifestPackageId packageId);

		void GetProperties();

		void GetPackageDependencies();

		void GetCapabilities();

		void GetResources();

		void GetDeviceCapabilities();

		void GetPrerequisite();

		void GetApplications(out IAppxManifestApplicationsEnumerator applications);

		void GetStream();
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("283ce2d7-7153-4a91-9649-7a0f7240945f")]
	internal interface IAppxManifestPackageId
	{
		void GetName([MarshalAs(UnmanagedType.LPWStr)] out string name);

		void GetArchitecture(out ProcessorArchitecture architecture);

		void GetPublisher([MarshalAs(UnmanagedType.LPWStr)] out string publisher);

		void GetVersion([MarshalAs(UnmanagedType.U8)] out ulong packageVersion);

		void GetResourceId([MarshalAs(UnmanagedType.LPWStr)] out string resourceId);

		void ComparePublisher([In][MarshalAs(UnmanagedType.LPWStr)] string other, [MarshalAs(UnmanagedType.Bool)] out bool isSame);

		void GetPackageFullName([MarshalAs(UnmanagedType.LPWStr)] out string packageFullName);

		void GetPackageFamilyName([MarshalAs(UnmanagedType.LPWStr)] out string packageFamilyName);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("9eb8a55a-f04b-4d0d-808d-686185d4847a")]
	internal interface IAppxManifestApplicationsEnumerator
	{
		int GetCurrent(out IAppxManifestApplication application);

		int GetHasCurrent([MarshalAs(UnmanagedType.Bool)] out bool hasCurrent);

		int MoveNext([MarshalAs(UnmanagedType.Bool)] out bool hasNext);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("5da89bf4-3773-46be-b650-7e744863b7e8")]
	internal interface IAppxManifestApplication
	{
		int GetStringValue([In][MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] out string value);

		int GetAppUserModelId([MarshalAs(UnmanagedType.LPWStr)] out string appUserModelId);
	}

	[ComImport]
	[Guid("B1AEC16F-2383-4852-B0E9-8F0B1DC66B4D")]
	internal class PackageDebugSettings
	{
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("1BB12A62-2AD8-432B-8CCF-0C2C52AFCD5B")]
	internal interface IPackageExecutionStateChangeNotification
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void OnStateChanged([In][MarshalAs(UnmanagedType.LPWStr)] string pszPackageFullName, [In] PACKAGE_EXECUTION_STATE pesNewState);
	}

	[ComImport]
	[Guid("F27C3930-8029-4AD1-94E3-3DBA417810C1")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComConversionLoss]
	internal interface IPackageDebugSettings
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void EnableDebugging([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName, [In][MarshalAs(UnmanagedType.LPWStr)] string debuggerCommandLine, [In] string environment);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void DisableDebugging([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Suspend([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Resume([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void TerminateAllProcesses([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetTargetSessionId([In] uint sessionId);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void EnumerateBackgroundTasks([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName, out uint taskCount, [MarshalAs(UnmanagedType.LPStruct)] out Guid taskIds, [Out] IntPtr taskNames);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ActivateBackgroundTask([In] ref Guid taskId);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void StartServicing([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void StopServicing([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void StartSessionRedirection([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName, [In] uint sessionId);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void StopSessionRedirection([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetPackageExecutionState([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName, out PACKAGE_EXECUTION_STATE packageExecutionState);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void RegisterForPackageStateChanges([In][MarshalAs(UnmanagedType.LPWStr)] string packageFullName, [In][MarshalAs(UnmanagedType.Interface)] IPackageExecutionStateChangeNotification pPackageExecutionStateChangeNotification, out uint pdwCookie);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void UnregisterForPackageStateChanges([In] uint dwCookie);
	}

	internal enum PACKAGE_EXECUTION_STATE
	{
		PES_UNKNOWN,
		PES_RUNNING,
		PES_SUSPENDING,
		PES_SUSPENDED,
		PES_TERMINATED
	}

	[Flags]
	internal enum StgmConstants
	{
		STGM_READ = 0,
		STGM_WRITE = 1,
		STGM_READWRITE = 2,
		STGM_SHARE_DENY_NONE = 0x40,
		STGM_SHARE_DENY_READ = 0x30,
		STGM_SHARE_DENY_WRITE = 0x20,
		STGM_SHARE_EXCLUSIVE = 0x10,
		STGM_PRIORITY = 0x40000,
		STGM_CREATE = 0x1000,
		STGM_CONVERT = 0x20000,
		STGM_FAILIFTHERE = 0,
		STGM_DIRECT = 0,
		STGM_TRANSACTED = 0x10000,
		STGM_NOSCRATCH = 0x100000,
		STGM_NOSNAPSHOT = 0x200000,
		STGM_SIMPLE = 0x8000000,
		STGM_DIRECT_SWMR = 0x400000,
		STGM_DELETEONRELEASE = 0x4000000
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class SECURITY_ATTRIBUTES
	{
		internal int nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));

		internal IntPtr lpSecurityDescriptor = IntPtr.Zero;

		internal bool bInheritHandle;
	}

	internal const string SID_EVERYONE = "S-1-1-0";

	internal const string SID_ALL_APPLICATION_PACKAGES = "S-1-15-2-1";

	internal const string SID_NETWORK = "S-1-5-2";

	internal const int RPC_E_SERVERCALL_RETRYLATER = -2147417846;

	internal const string ApplicationActivationManagerGuid = "45BA127D-10A8-46EA-8AB7-56EA9078943C";

	internal const string IApplicationActivationManagerGuid = "2e941141-7f97-4756-ba1d-9decde894a3d";

	internal const uint PIPE_ACCESS_DUPLEX = 3u;

	internal const uint FILE_FLAG_FIRST_PIPE_INSTANCE = 524288u;

	internal const uint FILE_FLAG_OVERLAPPED = 1073741824u;

	internal const uint PIPE_TYPE_BYTE = 0u;

	internal const uint PIPE_TYPE_MESSAGE = 4u;

	internal const uint PIPE_READMODE_BYTE = 0u;

	internal const uint PIPE_READMODE_MESSAGE = 2u;

	internal const uint PIPE_UNLIMITED_INSTANCES = 255u;

	internal const uint PIPE_WAIT = 0u;

	internal const uint PIPE_NOWAIT = 1u;

	internal const uint NMPWAIT_WAIT_FOREVER = uint.MaxValue;

	[DllImport("dwmapi.dll")]
	internal static extern int DwmIsCompositionEnabled(out bool enabled);

	[DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false)]
	[return: MarshalAs(UnmanagedType.Interface)]
	internal static extern object CoCreateInstance([In][MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, [MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, CLSCTX dwClsContext, [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid);

	[DllImport("shlwapi.dll", CharSet = CharSet.Unicode, EntryPoint = "SHCreateStreamOnFileW", ExactSpelling = true, PreserveSig = false)]
	internal static extern void SHCreateStreamOnFile(string fileName, StgmConstants mode, ref IStream stream);

	[DllImport("User32", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool SetProcessRestrictionExemption([In][MarshalAs(UnmanagedType.Bool)] bool isExempt);

	[DllImport("USER32.DLL")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool SetForegroundWindow(IntPtr windowHandle);

	[DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false)]
	internal static extern void CoAllowSetForegroundWindow([MarshalAs(UnmanagedType.IUnknown)] object pUnk, IntPtr lpvReserved);

	[DllImport("kernel32", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool CloseHandle(IntPtr hObject);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern SafePipeHandle CreateNamedPipe([MarshalAs(UnmanagedType.LPWStr)] string lpName, uint dwOpenMode, uint dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut, SECURITY_ATTRIBUTES pipeSecurityDescriptor);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool DisconnectNamedPipe(SafePipeHandle handle);
}
