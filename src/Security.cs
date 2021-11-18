using System;

namespace AdvReg
{
    [Flags]
    internal enum KeyAccessRights : uint
    {
        KEY_QUERY_VALUE = 0x00000001,
        KEY_SET_VALUE = 0x00000002,
        KEY_CREATE_SUB_KEY = 0x00000004,
        KEY_ENUMERATE_SUB_KEYS = 0x00000008,
        KEY_NOTIFY = 0x00000010,
        KEY_CREATE_LINK = 0x00000020,
        KEY_WOW64_64KEY = 0x00000100,
        KEY_WOW64_32KEY = 0x00000200,

        DELETE = 0x00010000,
        READ_CONTROL = 0x00020000,
        WRITE_DAC = 0x00040000,
        WRITE_OWNER = 0x00080000,
        STANDARD_RIGHTS_ALL = DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER,
        STANDARD_RIGHTS_EXECUTE = READ_CONTROL,
        STANDARD_RIGHTS_READ = READ_CONTROL,
        STANDARD_RIGHTS_REQUIRED = DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER,
        STANDARD_RIGHTS_WRITE = READ_CONTROL,
        ACCESS_SYSTEM_SECURITY = 0x01000000,

        KEY_READ = STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY,
        KEY_EXECUTE = KEY_READ,
        KEY_WRITE = STANDARD_RIGHTS_WRITE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY,
        KEY_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | KEY_QUERY_VALUE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY | KEY_CREATE_LINK,
    }
}
