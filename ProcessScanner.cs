using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Linq;

namespace NCKeys
{
    public static class ProcessScanner
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(
            IntPtr pTcpTable,
            ref int dwOutBufLen,
            bool sort,
            int ipVersion,
            int tblClass,
            int reserved);

        private enum TcpTableClass
        {
            TCP_TABLE_OWNER_PID_ALL = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_TCPROW_OWNER_PID
        {
            public uint state;
            public uint localAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] localPort;
            public uint remoteAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] remotePort;
            public int owningPid;
        }

        // Known safe processes (by name)
        private static readonly HashSet<string> SafeProcesses = new(StringComparer.OrdinalIgnoreCase)
        {
            // Browsers / Common apps
            "chrome", "msedge", "explorer", "steam", "discord",
            "nvcontainer", "SearchApp", "LockApp", "RuntimeBroker",
            "ShellExperienceHost", "ctfmon", "audiodg", "services",
            "ms-teams",

            // Windows core processes
            "svchost", "wininit", "csrss", "winlogon", "lsass",
            "taskhostw", "sihost", "conhost", "dllhost",

            // Intel / NVIDIA / Drivers
            "igfxEM", "RtkAudUService64", "NVIDIA Overlay",

            // Misc system
            "SecurityHealthSystray", "StartMenuExperienceHost",
            "TextInputHost", "CompPkgSrv", "smss",
            "ApplicationFrameHost", "SystemSettings",

            // Development (Visual Studio related)
            "devenv", "PerfWatson2", "MSBuild", "VBCSCompiler",
            "ServiceHub.IdentityHost", "ServiceHub.RoslynCodeAnalysisService",
            "ServiceHub.Host.dotnet.x64", "ServiceHub.DataWarehouseHost",
            "ServiceHub.ThreadedWaitDialog", "ServiceHub.IntellicodeModelService",
            "ServiceHub.TestWindowStoreHost", "ServiceHub.IndexingService",
            "DesignToolsServer",

            // Self
            "NCKeys"
        };

        // Known safe directories
        private static readonly string[] SafePaths =
        {
            @"C:\Windows\System32",
            @"C:\Program Files",
            @"C:\Program Files (x86)",
            Environment.GetFolderPath(Environment.SpecialFolder.Windows)
        };

        /// <summary>
        /// Returns suspicious processes as a grouped summary:
        /// e.g. "Discord (3 instance(s))"
        /// </summary>
        public static string[] GetSuspiciousProcessesSummary()
        {
            var results = new List<string>();

            var suspicious = GetSuspiciousProcessesRaw()
                .GroupBy(p => p.Split(" (PID")[0]) // group by process name
                .OrderBy(g => g.Key);

            foreach (var group in suspicious)
            {
                results.Add($"{group.Key} ({group.Count()} instance(s))");
            }

            return results.ToArray();
        }

        /// <summary>
        /// Returns raw suspicious processes with PID (un-grouped).
        /// </summary>
        public static string[] GetSuspiciousProcessesRaw()
        {
            var suspicious = new List<(string ProcessName, int Pid)>();

            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    string name = p.ProcessName;
                    string exePath = SafeGetPath(p);

                    if (IsSafeProcess(name, exePath))
                        continue;

                    if (IsSuspiciousProcess(p, exePath))
                        suspicious.Add((name, p.Id));
                }
                catch
                {
                    // ignore system/denied processes
                }
            }

            return suspicious.Select(s => $"{s.ProcessName} (PID {s.Pid})").ToArray();
        }

        // -----------------------
        //  Helpers
        // -----------------------

        private static bool IsSafeProcess(string name, string exePath)
        {
            if (SafeProcesses.Contains(name))
            {
                if (!string.IsNullOrEmpty(exePath) &&
                    SafePaths.Any(path => exePath.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsSuspiciousProcess(Process p, string exePath)
        {
            string name = p.ProcessName.ToLower();

            // 1. Suspicious name
            if (name.Contains("keylogger") || name.Contains("logger") || name.Contains("hook"))
                return true;

            // 2. Unsigned or untrusted publisher
            if (!string.IsNullOrEmpty(exePath) && !IsTrustedPublisher(exePath))
                return true;

            // 3. Hidden window + high CPU/memory
            if (string.IsNullOrEmpty(p.MainWindowTitle) &&
                (p.TotalProcessorTime.TotalSeconds > 20 ||
                 p.PrivateMemorySize64 > 100 * 1024 * 1024))
                return true;

            // 4. Has TCP but unsigned/untrusted
            if (ProcessHasTcpConnection(p.Id) && !IsTrustedPublisher(exePath))
                return true;

            return false;
        }

        private static string SafeGetPath(Process p)
        {
            try { return p.MainModule.FileName; }
            catch { return string.Empty; }
        }

        private static bool IsTrustedPublisher(string filePath)
        {
            try
            {
                // Use the new loader instead of the obsolete constructor
                var cert = X509CertificateLoader.LoadCertificateFromFile(filePath);
                string subject = cert.Subject?.ToLowerInvariant() ?? "";

                return subject.Contains("microsoft corporation") ||
                       subject.Contains("nvidia corporation") ||
                       subject.Contains("google llc") ||
                       subject.Contains("discord inc") ||
                       subject.Contains("valve") ||
                       subject.Contains("intel corporation") ||
                       subject.Contains("realtek semiconductor");
            }
            catch
            {
                return false;
            }
        }

        private static bool ProcessHasTcpConnection(int pid)
        {
            int buffSize = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, 2, (int)TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);
            IntPtr tcpTablePtr = Marshal.AllocHGlobal(buffSize);

            try
            {
                if (GetExtendedTcpTable(tcpTablePtr, ref buffSize, true, 2, (int)TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0) == 0)
                {
                    int rowSize = Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();
                    int numEntries = Marshal.ReadInt32(tcpTablePtr);
                    IntPtr rowPtr = (IntPtr)((long)tcpTablePtr + 4);

                    for (int i = 0; i < numEntries; i++)
                    {
                        var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                        if (row.owningPid == pid)
                            return true;

                        rowPtr = (IntPtr)((long)rowPtr + rowSize);
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTablePtr);
            }

            return false;
        }
    }
}
