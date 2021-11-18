using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;

namespace AdvReg
{
    internal class RegistryProviderHelper
    {
        private static ProviderInfo? _regProvider = null;

        public static ProviderInfo GetRegistryProvider(PSCmdlet cmdlet)
        {
            if (_regProvider == null)
                _regProvider = cmdlet.SessionState.Provider.GetOne("Registry");

            return _regProvider;
        }

        public static RegistryKey? GetProviderKey(PSCmdlet cmdlet, string path)
        {
            ProviderInfo regProvider = GetRegistryProvider(cmdlet);
            string psPath = $"{regProvider.ModuleName}\\{regProvider.Name}::{path}";

            Collection<PSObject> foundItems;
            try
            {
                cmdlet.WriteVerbose($"Attempting to get handle of key at '{psPath}'");
                foundItems = cmdlet.InvokeProvider.Item.Get(new string[1] { psPath }, true, true);
            }
            catch (ItemNotFoundException e)
            {
                // If the parent is missing report the error on the input key itself.
                ErrorRecord error = new ErrorRecord(e, "MissingKey", ErrorCategory.ObjectNotFound,
                    path);
                string errorMessage = $"Cannot find path '{path}' because it does not exist.";
                error.ErrorDetails = new ErrorDetails(errorMessage);
                cmdlet.WriteError(error);
                return null;
            }
            Debug.Assert(foundItems.Count == 1);
            return (RegistryKey)foundItems[0].BaseObject;
        }

        public static List<string> ProcessPaths(PSCmdlet cmdlet, string[] paths, bool expandWildcards)
        {
            ProviderInfo provider;
            PSDriveInfo drive;
            List<string> regPaths = new List<string>();

            foreach (string path in paths)
            {
                if (expandWildcards)
                {
                    try
                    {
                        regPaths.AddRange(cmdlet.GetResolvedProviderPathFromPSPath(path, out provider));
                    }
                    catch (ItemNotFoundException e)
                    {
                        cmdlet.WriteError(new ErrorRecord(e, "RegKeyNotFound", ErrorCategory.ObjectNotFound, path));
                        continue;
                    }
                }
                else
                {
                    regPaths.Add(cmdlet.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                        path, out provider, out drive));
                }

                if (!RegistryProviderHelper.IsRegistryPath(cmdlet, provider, path))
                    continue;
            }

            return regPaths;
        }

        public static bool IsRegistryPath(PSCmdlet cmdlet, ProviderInfo provider, string path)
        {
            // Cannot check on provider.ImplementingType so just check the module and name as a string.
            // https://github.com/PowerShell/PowerShell/issues/16475
            if (provider.ModuleName != "Microsoft.PowerShell.Core" || provider.Name != "Registry")
            {
                ArgumentException ex = new ArgumentException(String.Format(
                    "{0} does not resolve to a path on the Registry provider.", path));
                ErrorRecord error = new ErrorRecord(ex, "InvalidProvider", ErrorCategory.InvalidArgument, path);
                cmdlet.WriteError(error);

                return false;
            }

            return true;
        }
    }
}
