using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WinRT;
using XSurfUwp.Fallback;

namespace XSurfUwp.Fallback
{
    [global::WinRT.WindowsRuntimeType]
    [Guid("732EA77E-3430-47DF-9F0A-F8EED39E115A")]
    [global::WinRT.WindowsRuntimeHelperType(typeof(ABI.XSurfUwp.Fallback.IFallbackTypeProjectedABI))]
    public interface IFallbackType
    {

    }
}

namespace ABI.XSurfUwp.Fallback
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DynamicInterfaceCastableImplementation]
    [Guid("59242773-1CA0-4254-8447-F462C161F839")]
    internal unsafe interface IFallbackTypeProjectedABI : IFallbackType
    {
        public static readonly IntPtr AbiToProjectionVftablePtr;
        static unsafe IFallbackTypeProjectedABI()
        {
            AbiToProjectionVftablePtr = ComWrappersSupport.AllocateVtableMemory(typeof(IFallbackTypeProjectedABI), sizeof(IInspectable.Vftbl));
            *(IInspectable.Vftbl*)AbiToProjectionVftablePtr = IInspectable.Vftbl.AbiToProjectionVftable;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IFallbackTypeMethods
    {
        public static ref readonly global::System.Guid IID
        {
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get
            {
                global::System.ReadOnlySpan<byte> data = [0x7e, 0xa7, 0x2e, 0x73, 0x30, 0x34, 0xdf, 0x47, 0x9f, 0x0a, 0xf8, 0xee, 0xd3, 0x9e, 0x11, 0x5a];
                return ref global::System.Runtime.CompilerServices.Unsafe.As<byte, global::System.Guid>(ref global::System.Runtime.InteropServices.MemoryMarshal.GetReference(data));
            }
        }

        public static global::System.IntPtr AbiToProjectionVftablePtr => ABI.XSurfUwp.Fallback.IFallbackTypeProjectedABI.AbiToProjectionVftablePtr;
    }
}