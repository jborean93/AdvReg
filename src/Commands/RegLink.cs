using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;

namespace AdvReg
{
    [Cmdlet(
        VerbsCommon.Remove, "RegLink",
        DefaultParameterSetName = "Path",
        SupportsShouldProcess = true
    )]
    public class RemoveRegLinkCommand : PSCmdlet
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
            List<string> regPaths = RegistryProviderHelper.ProcessPaths(this, _paths, _shouldExpandWildcards);
            foreach (string regPath in regPaths)
            {
                WriteVerbose($"Starting removal of registry key '{regPath}'");

                // The Registry PSProvider chokes on broken registry links so we cannot use it to get the handle.
                // This gets the parent key path which is used to open the subsequent handle on the link itself.
                int parentIdx = regPath.LastIndexOf('\\');
                if (parentIdx == -1)
                {
                    ArgumentException ex = new ArgumentException(
                        $"'{regPath}' is a root registry key which cannot be removed.");
                    WriteError(new ErrorRecord(ex, "RootKeyCannotBeRemoved", ErrorCategory.InvalidArgument,
                                                regPath));
                    continue;
                }
                string parentPath = regPath.Substring(0, parentIdx);
                string linkName = regPath.Substring(parentIdx + 1);

                using (RegistryKey? parentKey = RegistryProviderHelper.GetProviderKey(this, parentPath))
                {
                    if (parentKey == null)
                        continue;

                    try
                    {
                        WriteVerbose($"Attempting to get handle of link at '{regPath}'");
                        using (SafeRegistryHandle key = NativeMethods.RegOpenKeyEx(parentKey.Handle, linkName,
                            NativeHelpers.OpenKeyOptions.REG_OPTION_OPEN_LINK, KeyAccessRights.DELETE))
                        {
                            WriteVerbose($"Deleting link at '{regPath}'");
                            if (ShouldProcess(regPath, "Remove link"))
                                NativeMethods.NtDeleteKey(key);
                        }
                    }
                    catch (NativeException e)
                    {
                        WriteError(ErrorHelper.GenerateWin32Error(e, "Failed to delete registry key", regPath));
                    }
                }
            }
        }
    }

    [Cmdlet(
        VerbsCommon.New, "RegLink",
        SupportsShouldProcess = true
    )]
    public class NewRegLinkCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true
        )]
        [ValidateNotNullOrEmpty]
        public string[]? Path;

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = true
        )]
        [ValidateNotNullOrEmpty]
        public string? Target;

        [Parameter()]
        public SwitchParameter Volatile;

        protected override void ProcessRecord()
        {
            foreach (string p in Path ?? Array.Empty<String>())
            {
                ProviderInfo provider;
                PSDriveInfo drive;

                string resolvedPath = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                    p, out provider, out drive);
                if (!RegistryProviderHelper.IsRegistryPath(this, provider, resolvedPath))
                    continue;

                string target = Target ?? "";
                string ntTargetPath = "";
                if (target.StartsWith("\\REGISTRY\\", true, CultureInfo.InvariantCulture))
                    ntTargetPath = target;
                else
                {
                    target = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                        target ?? "", out provider, out drive);
                    if (!RegistryProviderHelper.IsRegistryPath(this, provider, target))
                        continue;

                    try
                    {
                        ntTargetPath = GetRegistryNTPath(target);
                    }
                    catch (NativeException e)
                    {
                        WriteError(ErrorHelper.GenerateWin32Error(
                            e, "Failed to resolve target to an NT path", target));
                        continue;
                    }
                }

                int parentIdx = resolvedPath.LastIndexOf('\\');
                if (parentIdx == -1)
                {
                    ArgumentException ex = new ArgumentException(String.Format(
                        "{0} is a root registry key which cannot be set as a link.", resolvedPath));
                    WriteError(new ErrorRecord(ex, "RootKeyCannotBeLink", ErrorCategory.InvalidArgument,
                                               resolvedPath));
                    continue;
                }
                string parentPath = resolvedPath.Substring(0, parentIdx);
                string linkName = resolvedPath.Substring(parentIdx + 1);

                Collection<PSObject> foundItems;
                try
                {
                    foundItems = this.InvokeProvider.Item.Get(new string[1] { $"Registry::{parentPath}" },
                        true, true);
                }
                catch (ItemNotFoundException e)
                {
                    ErrorRecord error = new ErrorRecord(e, "MissingParent", ErrorCategory.ObjectNotFound,
                        parentPath);
                    string errorMessage = String.Format(
                        "Parent registry key {0} is missing and must be present to create a link.", parentPath);
                    error.ErrorDetails = new ErrorDetails(errorMessage);
                    WriteError(error);
                    continue;
                }
                Debug.Assert(foundItems.Count == 1);

                using (RegistryKey parentKey = (RegistryKey)foundItems[0].BaseObject)
                {
                    try
                    {
                        string ntResolvedPath = GetRegistryNTPath(parentKey);
                        string[] sourceComponents = ntResolvedPath.Split(new char[1] { '\\' },
                            StringSplitOptions.RemoveEmptyEntries);
                        string[] targetComponents = ntTargetPath.Split(new char[1] { '\\' },
                            StringSplitOptions.RemoveEmptyEntries);

                        // Registry links cannot cross the MACHINE/USER boundary or the USER boundary if the SID is
                        // different. If this occurs write a warning as nothing will fail but the link will most
                        // likely be broken when trying to be used.
                        if ((sourceComponents[1] != targetComponents[1]) ||
                            (sourceComponents[1] == "USER" && (sourceComponents[2] != targetComponents[2])))
                        {
                            WriteWarning($"Link hive target must be in the same hive as the source to be valid.");
                        }
                    }
                    catch (NativeException) {}  // We don't care about any errors here.

                    if (ShouldProcess($"{resolvedPath} -> {target}", "Create Registry Link"))
                    {
                        WriteVerbose($"Creating Link Key at {resolvedPath} -> {target}");
                        byte[] data = Encoding.Unicode.GetBytes(ntTargetPath);
                        NativeHelpers.CreateKeyOptions createOptions =
                            NativeHelpers.CreateKeyOptions.REG_OPTION_CREATE_LINK;
                        if (Volatile)
                            createOptions |= NativeHelpers.CreateKeyOptions.REG_OPTION_VOLATILE;

                        try
                        {
                            using (SafeRegistryHandle key = NativeMethods.RegCreateKeyEx(parentKey.Handle, linkName,
                                createOptions, KeyAccessRights.KEY_WRITE, out var createDisposition))
                            {
                                WriteVerbose($"Setting Link target to {ntTargetPath}");
                                NativeMethods.RegSetValueEx(key, "SymbolicLinkValue", data,
                                    NativeHelpers.DataType.REG_LINK);
                            }
                        }
                        catch (NativeException e)
                        {
                            WriteError(ErrorHelper.GenerateWin32Error(e, "Failed to create registry link", p));
                        }
                    }
                }
            }
        }

        private string GetRegistryNTPath(string psPath)
        {
            // Gets the root of the psPath to access the RegistryKey through the PSProvider. We cannot just use the
            // full path as it may not exist.
            string[] pathComponents = psPath.Split(new char[1] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            Collection<PSObject> foundItems = this.InvokeProvider.Item.Get(
                new string[1] { $"Registry::{pathComponents[0]}" }, true, true);
            Debug.Assert(foundItems.Count == 1);

            using (RegistryKey rootKey = (RegistryKey)foundItems[0].BaseObject)
                pathComponents[0] = GetRegistryNTPath(rootKey);

            // Replace the prefix with the root NT path and rejoin the components together
            return String.Join("\\", pathComponents);
        }

        private string GetRegistryNTPath(RegistryKey key)
        {
            // The key could be one of the predefined HANDLE values which do not work with NtQueryKey. Using
            // RegCreateKeyEx will open a new handle that can be used to get the NT path.
            using (SafeRegistryHandle dup = NativeMethods.RegCreateKeyEx(
                key.Handle,
                "",
                NativeHelpers.CreateKeyOptions.REG_OPTION_NON_VOLATILE,
                KeyAccessRights.KEY_QUERY_VALUE,
                out var dis
            ))
            using (SafeMemoryBuffer buffer = NativeMethods.NtQueryKey(dup,
                NativeHelpers.KEY_INFORMATION_CLASS.KeyNameInformation))
            {
                NativeHelpers.KEY_NAME_INFORMATION nameInfo =
                    Marshal.PtrToStructure<NativeHelpers.KEY_NAME_INFORMATION>(buffer.DangerousGetHandle());
                byte[] nameBuffer = new byte[nameInfo.NameLength];
                Marshal.Copy(IntPtr.Add(buffer.DangerousGetHandle(), 4), nameBuffer, 0, nameBuffer.Length);

                return Encoding.Unicode.GetString(nameBuffer);
            }
        }
    }
}
