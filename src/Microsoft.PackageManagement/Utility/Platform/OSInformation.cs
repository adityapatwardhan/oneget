﻿
namespace Microsoft.PackageManagement.Internal.Utility.Platform
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;

    /// <summary>
    /// These are platform abstractions and platform specific implementations
    /// </summary>
    public static class OSInformation
    {
        private static readonly string _dollarPSHome = "$PSHome";
        private static bool? _isWindows = null;
        private static bool? _isWindowsPowerShell = null;


        /// <summary>
        /// True if the current platform is Windows.
        /// </summary>
        public static bool IsWindows
        {
            get
            {
                if (_isWindows.HasValue) { return _isWindows.Value; }

#if CORECLR
                try
                {
                    _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                }
                catch
                {
                    _isWindows = false;
                }
#else
                _isWindows = true;
#endif
                return _isWindows.Value;
            }
        }

        /// <summary>
        /// true, on Windows FullCLR or Nano Server
        /// false, on PowerShellCore which can be Linux, Windows PowerShellCore.
        /// 
        /// Is CORECLR will be 'true' for both PowerShellCore and Nano 
        /// 
        /// </summary>
        public static bool IsWindowsPowerShell
        {
            get
            {
                if (_isWindowsPowerShell.HasValue) { return _isWindowsPowerShell.Value; }

                var psHomePath = RunPowerShellCommand(_dollarPSHome);

                if (!string.IsNullOrWhiteSpace(psHomePath) &&
                    psHomePath.TrimEnd(new char[] { '\\' })
                        .EndsWith(@"\WindowsPowerShell\v1.0", StringComparison.OrdinalIgnoreCase))
                {
                    _isWindowsPowerShell = true;
                }
                else
                {
                    _isWindowsPowerShell = false;
                }
                return _isWindowsPowerShell.Value;
            }
        }


        private static string RunPowerShellCommand(string commandName)
        {
            var iis = InitialSessionState.CreateDefault2();
            using (Runspace rs = RunspaceFactory.CreateRunspace(iis))
            {
                using (PowerShell powershell = PowerShell.Create())
                {
                    rs.Open();
                    powershell.Runspace = rs;
                    powershell.AddScript(commandName);
                    var retval = powershell.Invoke().FirstOrDefault();
                    if (retval != null)
                    {
                        return retval.ToString();
                    }
                }
            }
            return string.Empty;
        }
    }
}
