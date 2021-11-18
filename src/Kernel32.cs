using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace AdvReg
{
    internal partial class NativeHelpers
    {
        [Flags]
        internal enum CreateKeyOptions : uint
        {
            REG_OPTION_NON_VOLATILE = 0x00000000,
            REG_OPTION_VOLATILE = 0x00000001,
            REG_OPTION_CREATE_LINK = 0x00000002,
            REG_OPTION_BACKUP_RESTORE = 0x00000004,
        }

        internal enum CreateKeyDisposition : uint
        {
            REG_CREATED_NEW_KEY = 0x00000001,
            REG_OPENED_EXISTING_KEY = 0x00000002,
        }

        internal enum DataType : uint
        {
            REG_NONE = 0,
            REG_SZ = 1,
            REG_EXPAND_SZ = 2,
            REG_BINARY = 3,
            REG_DWORD = 4,
            REG_DWORD_LITTLE_ENDIAN = REG_DWORD,
            REG_DWORD_BIG_ENDIAN = 5,
            REG_LINK = 6,
            REG_MULTI_SZ = 7,
            REG_RESOURCE_LIST = 8,
            REG_FULL_RESOURCE_DESCRIPTOR = 9,
            REG_RESOURCE_REQUIREMENTS_LIST = 10,
            REG_QWORD = 11,
            REG_QWORD_LITTLE_ENDIAN = REG_QWORD,
        }
    }
    internal partial class NativeMethods
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern Int32 RegCreateKeyExW(
            SafeHandle hKey,
            string lpSubKey,
            UInt32 Reserved,
            IntPtr lpClass,
            NativeHelpers.CreateKeyOptions dwOptions,
            KeyAccessRights samDesired,
            IntPtr lpSecurityAttributes,
            out SafeRegistryHandle phkResult,
            out NativeHelpers.CreateKeyDisposition lpdwDisposition);

        public static SafeRegistryHandle RegCreateKeyEx(SafeHandle key, string subKey,
            NativeHelpers.CreateKeyOptions options, KeyAccessRights desired,
            out NativeHelpers.CreateKeyDisposition disposition)
        {
            SafeRegistryHandle handle;

            Int32 res = RegCreateKeyExW(key, subKey, 0, IntPtr.Zero, options, desired, IntPtr.Zero,
                out handle, out disposition);
            if (res != 0)
                throw new NativeException("RegCreateKeyEx", res);

            return handle;
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern Int32 RegSetValueExW(
            SafeHandle hKey,
            string lpValueName,
            UInt32 Reserved,
            NativeHelpers.DataType dwType,
            IntPtr lpData,
            Int32 cbData);

        public static void RegSetValueEx(SafeHandle key, string name, byte[] data, NativeHelpers.DataType dataType)
        {
            IntPtr buffer = Marshal.AllocHGlobal(data.Length);
            try
            {
                Marshal.Copy(data, 0, buffer, data.Length);
                int res = RegSetValueExW(key, name, 0, dataType, buffer, data.Length);
                if (res != 0)
                    throw new NativeException("RegSetValueEx", res);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
