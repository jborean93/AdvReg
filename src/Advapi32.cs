using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace AdvReg
{
    internal partial class NativeHelpers
    {
        [Flags]
        internal enum OpenKeyOptions : uint
        {
            NONE = 0x00000000,
            REG_OPTION_OPEN_LINK = 0x00000008,
        }
    }

    internal partial class NativeMethods
    {
        [DllImport("Advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern Int32 RegConnectRegistryW(
            string lpMachineName,
            SafeHandle hKey,
            out SafeRegistryHandle phkResult);

        public static SafeRegistryHandle RegConnectRegistry(string machine, SafeHandle key)
        {
            SafeRegistryHandle handle;
            Int32 res = RegConnectRegistryW(machine, key, out handle);
            if (res != 0)
                throw new NativeException("RegConnectRegistry", res);

            return handle;
        }

        [DllImport("Advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern Int32 RegOpenKeyExW(
            SafeHandle hKey,
            string lpSubKey,
            NativeHelpers.OpenKeyOptions ulOptions,
            KeyAccessRights samDesired,
            out SafeRegistryHandle phkResult);

        public static SafeRegistryHandle RegOpenKeyEx(SafeHandle key, string subKey,
            NativeHelpers.OpenKeyOptions options, KeyAccessRights desired)
        {
            SafeRegistryHandle handle;
            Int32 res = RegOpenKeyExW(key, subKey, options, desired, out handle);
            if (res != 0)
                throw new NativeException("RegOpenKeyEx", res);

            return handle;
        }

        [DllImport("Advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern Int32 RegQueryValueExW(
            SafeHandle hKey,
            string lpValueName,
            UInt32 lpReserved,
            out NativeHelpers.DataType lpType,
            IntPtr lpData,
            ref Int32 lpcbData);

        public static SafeMemoryBuffer RegQueryValueEx(SafeHandle key, string name,
            out NativeHelpers.DataType dataType)
        {
            int resultLength = 0;
            Int32 res = RegQueryValueExW(key, name, 0, out dataType, IntPtr.Zero, ref resultLength);
            if (!(res == (int)Win32ErrorCode.ERROR_MORE_DATA || res == (int)Win32ErrorCode.ERROR_SUCCESS))
                throw new NativeException("RegQueryValueEx", res);

            SafeMemoryBuffer buffer = new SafeMemoryBuffer(resultLength);
            try
            {
                res = RegQueryValueExW(key, name, 0, out dataType, buffer.DangerousGetHandle(), ref resultLength);

                if (res != (int)Win32ErrorCode.ERROR_SUCCESS)
                    throw new NativeException("RegQueryValueEx", res);
            }
            catch
            {
                buffer.Dispose();
                throw;
            }

            return buffer;
        }
    }
}
