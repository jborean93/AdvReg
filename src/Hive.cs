using Microsoft.Win32.SafeHandles;
using System;

namespace AdvReg
{
    internal static class Hive
    {
        public readonly static SafeRegistryHandle HKEY_CLASES_ROOT = new SafeRegistryHandle((IntPtr)0x80000000, false);
        public readonly static SafeRegistryHandle HKEY_CURRENT_USER = new SafeRegistryHandle((IntPtr)0x80000001, false);
        public readonly static SafeRegistryHandle HKEY_LOCAL_MACHINE = new SafeRegistryHandle((IntPtr)0x80000002, false);
        public readonly static SafeRegistryHandle HKEY_USERS = new SafeRegistryHandle((IntPtr)0x80000003, false);
        public readonly static SafeRegistryHandle HKEY_PERFORMANCE_DATA = new SafeRegistryHandle((IntPtr)0x80000004, false);
        public readonly static SafeRegistryHandle HKEY_PERFORMANCE_TEXT = new SafeRegistryHandle((IntPtr)0x80000050, false);
        public readonly static SafeRegistryHandle HKEY_PERFORMANCE_NLSTEXT = new SafeRegistryHandle((IntPtr)0x80000060, false);
        public readonly static SafeRegistryHandle HKEY_CURRENT_CONFIG = new SafeRegistryHandle((IntPtr)0x80000005, false);
        public readonly static SafeRegistryHandle HKEY_DYN_DATA = new SafeRegistryHandle((IntPtr)0x80000006, false);
        public readonly static SafeRegistryHandle HKEY_CURRENT_USER_LOCAL_SETTINGS = new SafeRegistryHandle((IntPtr)0x80000007, false);
    }
}
