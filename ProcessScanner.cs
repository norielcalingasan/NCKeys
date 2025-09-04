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

        // -----------------------
        //  Whitelist System
        // -----------------------

        private static readonly string WhitelistFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "whitelist.txt");

        public static readonly HashSet<string> DefaultProcesses = new(StringComparer.OrdinalIgnoreCase)
        {
            // Core system
            "System", "Idle", "smss", "csrss", "wininit", "winlogon",
            "services", "lsass", "svchost", "fontdrvhost",

            // Session/host
            "sihost", "dwm", "ctfmon", "taskhostw", "explorer",

            // Console / dll host
            "conhost", "dllhost", "runtimebroker",

            // Shell & UI
            "ShellExperienceHost", "StartMenuExperienceHost",
            "SearchApp", "TextInputHost", "LockApp",

            // Audio & input
            "audiodg", "RtkAudUService64",

            // Graphics / GPU
            "nvcontainer", "NVIDIA Web Helper Service", "igfxEM",

            // Settings & app framework
            "SystemSettings", "ApplicationFrameHost", "CompPkgSrv",

            // Security & health
            "SecurityHealthSystray", "SecurityHealthService",

            // Self
            "NCKeys"
        };

        public static readonly HashSet<string> DefaultPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64"),
            @"C:\Program Files",
            @"C:\Program Files (x86)"
        };


        private static HashSet<string> SafeProcesses = new(StringComparer.OrdinalIgnoreCase);
        private static HashSet<string> SafePaths = new(StringComparer.OrdinalIgnoreCase);

        static ProcessScanner()
        {
            LoadOrCreateWhitelist();
        }

        private static void LoadOrCreateWhitelist()
        {
            if (!File.Exists(WhitelistFile))
            {
                SaveWhitelist(DefaultProcesses, DefaultPaths, new HashSet<string>(), new HashSet<string>());
            }

            var processes = new HashSet<string>(DefaultProcesses, StringComparer.OrdinalIgnoreCase);
            var paths = new HashSet<string>(DefaultPaths, StringComparer.OrdinalIgnoreCase);
            var userProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var userPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in File.ReadAllLines(WhitelistFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) continue;

                if (line.StartsWith("process:", StringComparison.OrdinalIgnoreCase))
                {
                    string proc = line.Substring(8).Trim();
                    if (!string.IsNullOrEmpty(proc))
                    {
                        if (!DefaultProcesses.Contains(proc)) userProcesses.Add(proc);
                        processes.Add(proc);
                    }
                }
                else if (line.StartsWith("path:", StringComparison.OrdinalIgnoreCase))
                {
                    string path = line.Substring(5).Trim();
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (!DefaultPaths.Contains(path)) userPaths.Add(path);
                        paths.Add(path);
                    }
                }
            }

            SafeProcesses = processes;
            SafePaths = paths;

            // rewrite to restore defaults if missing
            SaveWhitelist(DefaultProcesses, DefaultPaths, userProcesses, userPaths);
        }

        private static void SaveWhitelist(HashSet<string> defaultsProc, HashSet<string> defaultsPath,
                                          HashSet<string> userProc, HashSet<string> userPath)
        {
            using var writer = new StreamWriter(WhitelistFile);

            writer.WriteLine("# Default Safe Processes (auto-restored if deleted)");
            foreach (var p in defaultsProc.OrderBy(x => x)) writer.WriteLine("process: " + p);

            writer.WriteLine();
            writer.WriteLine("# Default Safe Paths (auto-restored if deleted)");
            foreach (var p in defaultsPath.OrderBy(x => x)) writer.WriteLine("path: " + p);

            writer.WriteLine();
            writer.WriteLine("# User Added Entries");
            foreach (var p in userProc.OrderBy(x => x)) writer.WriteLine("process: " + p);
            foreach (var p in userPath.OrderBy(x => x)) writer.WriteLine("path: " + p);
        }

        // -----------------------
        //  Main Logic
        // -----------------------

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

        public static string[] GetSuspiciousProcessesRaw()
        {
            var suspicious = new List<(string ProcessName, int Pid)>();

            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    string? name = p.ProcessName;
                    string? exePath = SafeGetPath(p);

                    if (string.IsNullOrEmpty(name))
                        continue;

                    if (IsSafeProcess(name, exePath ?? string.Empty))
                        continue;

                    if (IsSuspiciousProcess(p, exePath ?? string.Empty))
                        suspicious.Add((name, p.Id));
                }
                catch
                {
                    // ignore system/denied processes
                }
            }

            return suspicious.Select(s => $"{s.ProcessName} (PID {s.Pid})").ToArray();
        }


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

        private static string? SafeGetPath(Process p)
        {
            try
            {
                return p.MainModule?.FileName;
            }
            catch
            {
                return null;
            }
        }


        private static bool IsTrustedPublisher(string filePath)
        {
            try
            {
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
