using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WinRT;
using WinRT.Interop;
using XSurfUwp;

namespace XSurfUwp
{
    [global::WinRT.WindowsRuntimeType]
    [Guid("59242773-1CA0-4254-8447-F462C161F839")]
    [global::WinRT.WindowsRuntimeHelperType(typeof(ABI.XSurfUwp.ITapBridgeProjectedABI))]
    public interface ITapBridge
    {
        void RegisterContent(object content);
        void ClearContent();
        object GetHitTestRoot();
        void SetPanZoomTransform(double scale, double offsetX, double offsetY);
        void SetFlatBackground(string backgroundColor);
        void SetContentBorder(string borderColor);
        void SetRootSize(object obj, double width, double height);
        bool TrySerialize(object obj, out string typeName, out string value, out bool isValueType);
    }
}

namespace ABI.XSurfUwp
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DynamicInterfaceCastableImplementation]
    [Guid("59242773-1CA0-4254-8447-F462C161F839")]
    internal unsafe interface ITapBridgeProjectedABI : ITapBridge
    {
        public static readonly IntPtr AbiToProjectionVftablePtr;
        static unsafe ITapBridgeProjectedABI()
        {
            AbiToProjectionVftablePtr = ComWrappersSupport.AllocateVtableMemory(typeof(ITapBridgeProjectedABI), sizeof(IInspectable.Vftbl) + sizeof(IntPtr) * 8);
            *(IInspectable.Vftbl*)AbiToProjectionVftablePtr = IInspectable.Vftbl.AbiToProjectionVftable;
            ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int>*)AbiToProjectionVftablePtr)[6] = &Do_Abi_RegisterContent_0;
            ((delegate* unmanaged[Stdcall]<IntPtr, int>*)AbiToProjectionVftablePtr)[7] = &Do_Abi_ClearContent_1;
            ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>*)AbiToProjectionVftablePtr)[8] = &Do_Abi_GetHitTestRoot_2;
            ((delegate* unmanaged[Stdcall]<IntPtr, double, double, double, int>*)AbiToProjectionVftablePtr)[9] = &Do_Abi_SetPanZoomTransform_3;
            ((delegate* unmanaged[Stdcall]<IntPtr, void*, int>*)AbiToProjectionVftablePtr)[10] = &Do_Abi_SetFlatBackground_4;
            ((delegate* unmanaged[Stdcall]<IntPtr, void*, int>*)AbiToProjectionVftablePtr)[11] = &Do_Abi_SetContentBorder_5;
            ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, double, double, int>*)AbiToProjectionVftablePtr)[12] = &Do_Abi_SetRootSize_6;
            ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr*, IntPtr*, byte*, byte*, int>*)AbiToProjectionVftablePtr)[13] = &Do_Abi_TrySerialize_7;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static unsafe int Do_Abi_RegisterContent_0(IntPtr thisPtr, IntPtr content)
        {
            try
            {
                global::WinRT.ComWrappersSupport.FindObject<ITapBridge>(thisPtr).RegisterContent(MarshalInspectable<object>.FromAbi(content));

            }
            catch (Exception __exception__)
            {
                global::WinRT.ExceptionHelpers.SetErrorInfo(__exception__);
                return global::WinRT.ExceptionHelpers.GetHRForException(__exception__);
            }
            return 0;
        }
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static unsafe int Do_Abi_ClearContent_1(IntPtr thisPtr)
        {
            try
            {
                global::WinRT.ComWrappersSupport.FindObject<ITapBridge>(thisPtr).ClearContent();

            }
            catch (Exception __exception__)
            {
                global::WinRT.ExceptionHelpers.SetErrorInfo(__exception__);
                return global::WinRT.ExceptionHelpers.GetHRForException(__exception__);
            }
            return 0;
        }
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static unsafe int Do_Abi_GetHitTestRoot_2(IntPtr thisPtr, IntPtr* __returnValue)
        {
            object? ____returnValue = default;

            *__returnValue = default;

            try
            {
                ____returnValue = global::WinRT.ComWrappersSupport.FindObject<ITapBridge>(thisPtr).GetHitTestRoot();
                *__returnValue = MarshalInspectable<object>.FromManaged(____returnValue);

            }
            catch (Exception __exception__)
            {
                global::WinRT.ExceptionHelpers.SetErrorInfo(__exception__);
                return global::WinRT.ExceptionHelpers.GetHRForException(__exception__);
            }
            return 0;
        }
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static unsafe int Do_Abi_SetPanZoomTransform_3(IntPtr thisPtr, double scale, double offsetX, double offsetY)
        {
            try
            {
                global::WinRT.ComWrappersSupport.FindObject<ITapBridge>(thisPtr).SetPanZoomTransform(scale, offsetX, offsetY);

            }
            catch (Exception __exception__)
            {
                global::WinRT.ExceptionHelpers.SetErrorInfo(__exception__);
                return global::WinRT.ExceptionHelpers.GetHRForException(__exception__);
            }
            return 0;
        }
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static unsafe int Do_Abi_SetFlatBackground_4(IntPtr thisPtr, void* backgroundColor)
        {
            try
            {
                global::WinRT.ComWrappersSupport.FindObject<ITapBridge>(thisPtr).SetFlatBackground(new string((char*)backgroundColor));

            }
            catch (Exception __exception__)
            {
                global::WinRT.ExceptionHelpers.SetErrorInfo(__exception__);
                return global::WinRT.ExceptionHelpers.GetHRForException(__exception__);
            }
            return 0;
        }
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static unsafe int Do_Abi_SetContentBorder_5(IntPtr thisPtr, void* borderColor)
        {
            try
            {
                global::WinRT.ComWrappersSupport.FindObject<ITapBridge>(thisPtr).SetContentBorder(new string((char*)borderColor));

            }
            catch (Exception __exception__)
            {
                global::WinRT.ExceptionHelpers.SetErrorInfo(__exception__);
                return global::WinRT.ExceptionHelpers.GetHRForException(__exception__);
            }
            return 0;
        }
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static unsafe int Do_Abi_SetRootSize_6(IntPtr thisPtr, IntPtr obj, double width, double height)
        {
            try
            {
                global::WinRT.ComWrappersSupport.FindObject<ITapBridge>(thisPtr).SetRootSize(MarshalInspectable<object>.FromAbi(obj), width, height);

            }
            catch (Exception __exception__)
            {
                global::WinRT.ExceptionHelpers.SetErrorInfo(__exception__);
                return global::WinRT.ExceptionHelpers.GetHRForException(__exception__);
            }
            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static unsafe int Do_Abi_TrySerialize_7(IntPtr thisPtr, IntPtr obj, IntPtr* typeName, IntPtr* value, byte* isValueType, byte* __returnValue)
        {
            bool ____returnValue = default;

            *typeName = default;
            *value = default;
            *isValueType = default;
            *__returnValue = default;
            string? __typeName = default;
            string? __value = default;
            bool __isValueType = default;

            try
            {
                ____returnValue = global::WinRT.ComWrappersSupport.FindObject<ITapBridge>(thisPtr).TrySerialize(MarshalInspectable<object>.FromAbi(obj), out __typeName, out __value, out __isValueType);
                *typeName = MarshalString.FromManaged(__typeName);
                *value = MarshalString.FromManaged(__value);
                *isValueType = (byte)(__isValueType ? 1 : 0);
                *__returnValue = (byte)(____returnValue ? 1 : 0);

            }
            catch (Exception __exception__)
            {
                global::WinRT.ExceptionHelpers.SetErrorInfo(__exception__);
                return global::WinRT.ExceptionHelpers.GetHRForException(__exception__);
            }
            return 0;
        }

        unsafe void ITapBridge.RegisterContent(object content)
        {
            var _obj = ((IObjectReference)((IWinRTObject)this).GetObjectReferenceForType(typeof(ITapBridge).TypeHandle));
            ITapBridgeMethods.RegisterContent(_obj, content);
        }

        unsafe void ITapBridge.ClearContent()
        {
            var _obj = ((IObjectReference)((IWinRTObject)this).GetObjectReferenceForType(typeof(ITapBridge).TypeHandle));
            ITapBridgeMethods.ClearContent(_obj);
        }

        unsafe object ITapBridge.GetHitTestRoot()
        {
            var _obj = ((IObjectReference)((IWinRTObject)this).GetObjectReferenceForType(typeof(ITapBridge).TypeHandle));
            return ITapBridgeMethods.GetHitTestRoot(_obj);
        }

        unsafe void ITapBridge.SetPanZoomTransform(double scale, double offsetX, double offsetY)
        {
            var _obj = ((IObjectReference)((IWinRTObject)this).GetObjectReferenceForType(typeof(ITapBridge).TypeHandle));
            ITapBridgeMethods.SetPanZoomTransform(_obj, scale, offsetX, offsetY);
        }

        unsafe void ITapBridge.SetFlatBackground(string backgroundColor)
        {
            var _obj = ((IObjectReference)((IWinRTObject)this).GetObjectReferenceForType(typeof(ITapBridge).TypeHandle));
            ITapBridgeMethods.SetFlatBackground(_obj, backgroundColor);
        }

        unsafe void ITapBridge.SetContentBorder(string borderColor)
        {
            var _obj = ((IObjectReference)((IWinRTObject)this).GetObjectReferenceForType(typeof(ITapBridge).TypeHandle));
            ITapBridgeMethods.SetContentBorder(_obj, borderColor);
        }

        unsafe void ITapBridge.SetRootSize(object obj, double width, double height)
        {
            var _obj = ((IObjectReference)((IWinRTObject)this).GetObjectReferenceForType(typeof(ITapBridge).TypeHandle));
            ITapBridgeMethods.SetRootSize(_obj, obj, width, height);
        }

        unsafe bool ITapBridge.TrySerialize(object obj, out string typeName, out string value, out bool isValueType)
        {
            var _obj = ((IObjectReference)((IWinRTObject)this).GetObjectReferenceForType(typeof(ITapBridge).TypeHandle));
            return ITapBridgeMethods.TrySerialize(_obj, obj, out typeName, out value, out isValueType);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ITapBridgeMethods
    {
        public static unsafe void RegisterContent(IObjectReference _obj, object content)
        {
            var ThisPtr = _obj.ThisPtr;

            ObjectReferenceValue __content = default;
            try
            {
                __content = MarshalInspectable<object>.CreateMarshaler2(content);
                global::WinRT.ExceptionHelpers.ThrowExceptionForHR((*(delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int>**)ThisPtr)[6](ThisPtr, MarshalInspectable<object>.GetAbi(__content)));
            }
            finally
            {
                MarshalInspectable<object>.DisposeMarshaler(__content);
            }
        }

        public static unsafe void ClearContent(IObjectReference _obj)
        {
            var ThisPtr = _obj.ThisPtr;

            global::WinRT.ExceptionHelpers.ThrowExceptionForHR((*(delegate* unmanaged[Stdcall]<IntPtr, int>**)ThisPtr)[7](ThisPtr));
        }

        public static unsafe object GetHitTestRoot(IObjectReference _obj)
        {
            var ThisPtr = _obj.ThisPtr;

            IntPtr __retval = default;
            try
            {
                global::WinRT.ExceptionHelpers.ThrowExceptionForHR((*(delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>**)ThisPtr)[8](ThisPtr, &__retval));
                return MarshalInspectable<object>.FromAbi(__retval);
            }
            finally
            {
                MarshalInspectable<object>.DisposeAbi(__retval);
            }
        }

        public static unsafe void SetPanZoomTransform(IObjectReference _obj, double scale, double offsetX, double offsetY)
        {
            var ThisPtr = _obj.ThisPtr;

            global::WinRT.ExceptionHelpers.ThrowExceptionForHR((*(delegate* unmanaged[Stdcall]<IntPtr, double, double, double, int>**)ThisPtr)[9](ThisPtr, scale, offsetX, offsetY));
        }

        public static unsafe void SetFlatBackground(IObjectReference _obj, string backgroundColor)
        {
            var ThisPtr = _obj.ThisPtr;

            fixed (char* backgroundColorPtr = backgroundColor)
                global::WinRT.ExceptionHelpers.ThrowExceptionForHR((*(delegate* unmanaged[Stdcall]<IntPtr, void*, int>**)ThisPtr)[10](ThisPtr, backgroundColorPtr));
        }

        public static unsafe void SetContentBorder(IObjectReference _obj, string borderColor)
        {
            var ThisPtr = _obj.ThisPtr;

            fixed (char* borderColorPtr = borderColor)
                global::WinRT.ExceptionHelpers.ThrowExceptionForHR((*(delegate* unmanaged[Stdcall]<IntPtr, void*, int>**)ThisPtr)[11](ThisPtr, borderColorPtr));
        }

        public static unsafe void SetRootSize(IObjectReference _obj, object obj, double width, double height)
        {
            var ThisPtr = _obj.ThisPtr;

            ObjectReferenceValue __obj = default;
            try
            {
                __obj = MarshalInspectable<object>.CreateMarshaler2(obj);
                global::WinRT.ExceptionHelpers.ThrowExceptionForHR((*(delegate* unmanaged[Stdcall]<IntPtr, IntPtr, double, double, int>**)ThisPtr)[12](ThisPtr, MarshalInspectable<object>.GetAbi(__obj), width, height));
            }
            finally
            {
                MarshalInspectable<object>.DisposeMarshaler(__obj);
            }
        }

        public static unsafe bool TrySerialize(IObjectReference _obj, object obj, out string typeName, out string value, out bool isValueType)
        {
            var ThisPtr = _obj.ThisPtr;

            ObjectReferenceValue __obj = default;
            IntPtr __typeName = default;
            IntPtr __value = default;
            byte __isValueType = default;
            byte __retval = default;
            try
            {
                __obj = MarshalInspectable<object>.CreateMarshaler2(obj);
                global::WinRT.ExceptionHelpers.ThrowExceptionForHR((*(delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr*, IntPtr*, byte*, byte*, int>**)ThisPtr)[13](ThisPtr, MarshalInspectable<object>.GetAbi(__obj), &__typeName, &__value, &__isValueType, &__retval));
                typeName = MarshalString.FromAbi(__typeName);
                value = MarshalString.FromAbi(__value);
                isValueType = __isValueType != 0;
                return __retval != 0;
            }
            finally
            {
                MarshalInspectable<object>.DisposeMarshaler(__obj);
                MarshalString.DisposeAbi(__typeName);
                MarshalString.DisposeAbi(__value);
            }
        }


        public static ref readonly global::System.Guid IID
        {
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get
            {
                global::System.ReadOnlySpan<byte> data = new byte[] { 0x73, 0x27, 0x24, 0x59, 0xA0, 0x1C, 0x54, 0x42, 0x84, 0x47, 0xF4, 0x62, 0xC1, 0x61, 0xF8, 0x39 };
                return ref global::System.Runtime.CompilerServices.Unsafe.As<byte, global::System.Guid>(ref global::System.Runtime.InteropServices.MemoryMarshal.GetReference(data));
            }
        }

        public static global::System.IntPtr AbiToProjectionVftablePtr => ABI.XSurfUwp.ITapBridgeProjectedABI.AbiToProjectionVftablePtr;

    }
}