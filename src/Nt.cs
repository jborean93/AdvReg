using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace AdvReg
{
    internal partial class NativeHelpers
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_BASIC_INFORMATION
        {
            public Int64 LastWriteTime;
            public UInt32 TitleIndex;
            public Int32 NameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] public char[] Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_FLAGS_INFORMATION
        {
            // This struct isn't really documented so these fields are based on guess work and what other online
            // samples use
            public UInt32 Wow64Flags;
            public UserFlags UserFlags;
            public ControlFlags ControlFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_FULL_INFORMATION
        {
            public Int64 LastWriteTime;
            public UInt32 TitleIndex;
            public Int32 ClassOffset;
            public Int32 ClassLength;
            public Int32 SubKeys;
            public Int32 MaxNameLen;
            public Int32 MaxClassLen;
            public Int32 Values;
            public Int32 MaxValueNameLen;
            public Int32 MaxValueDataLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] public char[] Class;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_HANDLE_TAGS_INFORMATION
        {
            public UInt32 HandleTags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_LAYER_INFORMATION
        {
            public UInt32 IsTombstone;
            public UInt32 IsSupersedeLocal;
            public UInt32 IsSupersedeTree;
            public UInt32 ClassIsInherited;
            public UInt32 Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_NAME_INFORMATION
        {
            public Int32 NameLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] public char[] Class;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_TRUST_INFORMATION
        {
            public UInt32 TrustedKey;
            public UInt32 Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_VIRTUALIZATION_INFORMATION
        {
            public UInt32 VirtualizationCandidate;
            public UInt32 VirtualizationEnabled;
            public UInt32 VirtualTarget;
            public UInt32 VirtualStore;
            public UInt32 VirtualSource;
            public UInt32 Reserved;
        }

        [Flags]
        public enum ControlFlags : uint
        {
            REG_KEY_NONE = 0x00000000,
            REG_KEY_DONT_VIRTUALIZE = 0x00000002,
            REG_KEY_DONT_SILENT_FAIL = 0x00000004,
            REG_KEY_RECURSE_FLAG = 0x00000008,
        }

        [Flags]
        public enum UserFlags : uint
        {
            REG_FLAG_NONE = 0x00000000,
            REG_FLAG_VOLATILE = 0x00000001,
            REG_FLAG_LINK = 0x00000002,
        }

        public enum KEY_INFORMATION_CLASS  : uint
        {
            KeyBasicInformation = 0,
            KeyNodeInformation = 1,
            KeyFullInformation = 2,
            KeyNameInformation = 3,
            KeyCachedInformation = 4,
            KeyFlagsInformation = 5,
            KeyVirtualizationInformation = 6,
            KeyHandleTagsInformation = 7,
            KeyTrustInformation = 8,
            KeyLayerInformation = 9,
        }
    }

    internal partial class NativeMethods
    {
        [DllImport("NtDll.dll", EntryPoint = "NtQueryKey")]
        private static extern UInt32 NativeNtQueryKey(
            SafeHandle KeyHandle,
            NativeHelpers.KEY_INFORMATION_CLASS KeyInformationClass,
            IntPtr KeyInformation,
            Int32 Length,
            out Int32 ResultLength
        );

        public static SafeMemoryBuffer NtQueryKey(SafeHandle key, NativeHelpers.KEY_INFORMATION_CLASS infoClass)
        {
            int resultLength;
            UInt32 res = NativeNtQueryKey(key, infoClass, IntPtr.Zero, 0, out resultLength);
            // STATUS_BUFFER_OVERFLOW or STATUS_BUFFER_TOO_SMALL
            if (!(res == 0x80000005 || res == 0xC0000023))
                throw new NativeException("NtQueryKey", NativeMethods.RtlNtStatusToDosError(res));

            SafeMemoryBuffer buffer = new SafeMemoryBuffer(resultLength);
            try
            {
                res = NativeNtQueryKey(key, infoClass, buffer.DangerousGetHandle(), resultLength,
                    out resultLength);

                if (res != 0)
                    throw new NativeException("NtQueryKey", NativeMethods.RtlNtStatusToDosError(res));
            }
            catch
            {
                buffer.Dispose();
                throw;
            }

            return buffer;
        }

        [DllImport("NtDll.dll")]
        public static extern Int32 RtlNtStatusToDosError(
            UInt32 Status);

        [DllImport("Ntdll.dll", EntryPoint = "NtDeleteKey")]
        private static extern UInt32 NativeNtDeleteKey(
            SafeHandle KeyHandle);

        public static void NtDeleteKey(SafeHandle key)
        {
            UInt32 res = NativeNtDeleteKey(key);
            if (res != 0)
                throw new NativeException("NtDeleteKey", NativeMethods.RtlNtStatusToDosError(res));
        }
    }

    internal class SafeMemoryBuffer : SafeHandleZeroOrMinusOneIsInvalid
    {
        public int Length { get; internal set; } = 0;
        public SafeMemoryBuffer() : base(true) { }
        public SafeMemoryBuffer(int cb) : base(true)
        {
            base.SetHandle(Marshal.AllocHGlobal(cb));
            Length = cb;
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }
    }
}
