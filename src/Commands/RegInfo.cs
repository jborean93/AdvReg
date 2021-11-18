using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace AdvReg
{
    [Flags]
    public enum KeyControlFlags : uint
    {
        None = 0x00000000,
        DontVirtualize = 0x00000002,
        DontSilentFail = 0x00000004,
        RecurseFlag = 0x00000008,
    }

    [Flags]
    public enum KeyUserFlags : uint
    {
        None = 0x00000000,
        Volatile = 0x00000001,
        Link = 0x00000002,
    }

    public class KeyInformation
    {
        public string PSPath { get; internal set; } = "";
        public string NTPath { get; internal set; } = "";
        public DateTime LastWriteTime { get; internal set; }
        public UInt32 TitleIndex { get; internal set; }
        public string Name { get; internal set; } = "";
        public string Class { get; internal set; } = "";
        public Int32 SubKeys { get; internal set; }
        public Int32 ValueCount { get; internal set ; }
        public KeyUserFlags UserFlags { get; internal set; }
        public KeyControlFlags ControlFlags { get; internal set; }
        public string? Target { get; internal set; }
        public bool VirtualizationCandidate { get; internal set; }
        public bool VirtualizationEnabled { get; internal set; }
        public bool VirtualTarget { get; internal set; }
        public bool VirtualStore { get; internal set; }
        public bool VirtualSource { get; internal set; }
        public UInt32 HandleTags { get; internal set; }
        public bool TrustedKey { get; internal set; }

        /*  Parameter is invalid
        public bool IsTombstone { get; internal set; }
        public bool IsSupersedeLocal { get; internal set; }
        public bool IsSupersedeTree { get; internal set; }
        public bool ClassIsInherited { get; internal set; }
        */
    }

    [Cmdlet(
        VerbsCommon.Get, "RegInfo",
        DefaultParameterSetName = "Path"
    )]
    [OutputType(typeof(KeyInformation))]
    public class GetRegInfoCommand : PSCmdlet
    {
        private string[] _paths = Array.Empty<string>();
        private bool _shouldExpandWildcards;

        [Parameter(
            Mandatory = true,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "LiteralPath")
        ]
        [Alias("PSPath")]
        [ValidateNotNullOrEmpty]
        public string[] LiteralPath
        {
            get { return _paths; }
            set { _paths = value; }
        }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Path")
        ]
        [ValidateNotNullOrEmpty]
        public string[] Path
        {
            get { return _paths; }
            set
            {
                _shouldExpandWildcards = true;
                _paths = value;
            }
        }

        protected override void ProcessRecord()
        {
            ProviderInfo regProvider = RegistryProviderHelper.GetRegistryProvider(this);
            List<string> regPaths = RegistryProviderHelper.ProcessPaths(this, _paths, _shouldExpandWildcards);
            foreach (string regPath in regPaths)
            {
                int parentIdx = regPath.LastIndexOf('\\');
                string parentPath = "";
                string keyName = "";
                if (parentIdx == -1)
                {
                    parentPath = regPath;
                    keyName = "";
                }
                else
                {
                    parentPath = regPath.Substring(0, parentIdx);
                    keyName = regPath.Substring(parentIdx + 1);
                }

                using (RegistryKey? parentKey = RegistryProviderHelper.GetProviderKey(this, parentPath))
                {
                    if (parentKey == null)
                        continue;

                    try
                    {
                        WriteVerbose($"Attempting to get handle of key at '{regPath}'");
                        using (SafeRegistryHandle key = NativeMethods.RegOpenKeyEx(parentKey.Handle, keyName,
                            NativeHelpers.OpenKeyOptions.REG_OPTION_OPEN_LINK, KeyAccessRights.KEY_QUERY_VALUE))
                        {
                            KeyInformation info = GetKeyInformation(key);
                            info.PSPath = $"{regProvider.ModuleName}\\Registry::{regPath}";

                            WriteObject(info);
                        }
                    }
                    catch (NativeException e)
                    {
                        WriteError(ErrorHelper.GenerateWin32Error(e, "Failed to query registry key", regPath));
                    }
                }
            }
        }

        private KeyInformation GetKeyInformation(SafeHandle handle)
        {
            KeyInformation info = new KeyInformation();

            using (var buffer = NativeMethods.NtQueryKey(handle,
                NativeHelpers.KEY_INFORMATION_CLASS.KeyBasicInformation))
            {
                var obj = Marshal.PtrToStructure<NativeHelpers.KEY_BASIC_INFORMATION>(
                    buffer.DangerousGetHandle());

                IntPtr nameBuffer = IntPtr.Add(buffer.DangerousGetHandle(), 16);

                info.LastWriteTime = DateTime.FromFileTimeUtc(obj.LastWriteTime);
                info.TitleIndex = obj.TitleIndex;
                info.Name = Marshal.PtrToStringUni(nameBuffer, obj.NameLength / 2);
            }

            using (var buffer = NativeMethods.NtQueryKey(handle,
                NativeHelpers.KEY_INFORMATION_CLASS.KeyFullInformation))
            {
                var obj = Marshal.PtrToStructure<NativeHelpers.KEY_FULL_INFORMATION>(
                    buffer.DangerousGetHandle());

                IntPtr classBuffer = IntPtr.Add(buffer.DangerousGetHandle(), obj.ClassOffset);

                info.Class = Marshal.PtrToStringUni(classBuffer, obj.ClassLength / 2);
                info.SubKeys = obj.SubKeys;
                info.ValueCount = obj.Values;
            }

            using (var buffer = NativeMethods.NtQueryKey(handle,
                NativeHelpers.KEY_INFORMATION_CLASS.KeyNameInformation))
            {
                var obj = Marshal.PtrToStructure<NativeHelpers.KEY_NAME_INFORMATION>(
                    buffer.DangerousGetHandle());

                IntPtr nameBuffer = IntPtr.Add(buffer.DangerousGetHandle(), 4);
                info.NTPath = Marshal.PtrToStringUni(nameBuffer, obj.NameLength / 2);
            }

            using (var buffer = NativeMethods.NtQueryKey(handle,
                NativeHelpers.KEY_INFORMATION_CLASS.KeyFlagsInformation))
            {
                var obj = Marshal.PtrToStructure<NativeHelpers.KEY_FLAGS_INFORMATION>(
                    buffer.DangerousGetHandle());

                info.UserFlags = (KeyUserFlags)obj.UserFlags;
                info.ControlFlags = (KeyControlFlags)obj.ControlFlags;
            }

            if ((info.UserFlags & KeyUserFlags.Link) == KeyUserFlags.Link)
            {
                try
                {
                    using (var buffer = NativeMethods.RegQueryValueEx(handle, "SymbolicLinkValue", out var dataType))
                    {
                        if (dataType != NativeHelpers.DataType.REG_LINK)
                            throw new NativeException($"Expecting reg link of REG_LINK data type but got {dataType}",
                                (int)Win32ErrorCode.ERROR_INVALID_DATA);

                        info.Target = Marshal.PtrToStringUni(buffer.DangerousGetHandle(), buffer.Length / 2);
                    }
                }
                catch (NativeException e)
                {
                    WriteWarning($"Key was a registry link but could not retrieve the target: {e.Message}");
                }
            }

            using (var buffer = NativeMethods.NtQueryKey(handle,
                NativeHelpers.KEY_INFORMATION_CLASS.KeyVirtualizationInformation))
            {
                var obj = Marshal.PtrToStructure<NativeHelpers.KEY_VIRTUALIZATION_INFORMATION>(
                    buffer.DangerousGetHandle());

                info.VirtualizationCandidate = obj.VirtualizationCandidate == 1;
                info.VirtualizationEnabled = obj.VirtualizationEnabled == 1;
                info.VirtualTarget = obj.VirtualTarget == 1;
                info.VirtualStore = obj.VirtualStore == 1;
                info.VirtualSource = obj.VirtualSource == 1;
            }

            using (var buffer = NativeMethods.NtQueryKey(handle,
                NativeHelpers.KEY_INFORMATION_CLASS.KeyHandleTagsInformation))
            {
                var obj = Marshal.PtrToStructure<NativeHelpers.KEY_HANDLE_TAGS_INFORMATION>(
                    buffer.DangerousGetHandle());

                info.HandleTags = obj.HandleTags;
            }

            using (var buffer = NativeMethods.NtQueryKey(handle,
                NativeHelpers.KEY_INFORMATION_CLASS.KeyTrustInformation))
            {
                var obj = Marshal.PtrToStructure<NativeHelpers.KEY_TRUST_INFORMATION>(
                    buffer.DangerousGetHandle());

                info.TrustedKey = obj.TrustedKey == 1;
            }

            /*  Parameter is invalid
            using (var buffer = NativeMethods.NtQueryKey(handle,
                NativeHelpers.KEY_INFORMATION_CLASS.KeyLayerInformation))
            {
                var obj = Marshal.PtrToStructure<NativeHelpers.KEY_LAYER_INFORMATION>(
                    buffer.DangerousGetHandle());

                info.IsTombstone = obj.IsTombstone == 1;
                info.IsSupersedeLocal = obj.IsSupersedeLocal == 1;
                info.IsSupersedeTree = obj.IsSupersedeTree == 1;
                info.ClassIsInherited = obj.ClassIsInherited == 1;
            }
            */

            return info;
        }
    }
}
