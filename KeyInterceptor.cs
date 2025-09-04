using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCKeys
{
    enum MibTcpState
    {
        Closed = 1, Listen, SynSent, SynReceived, Established, FinWait1, FinWait2,
        CloseWait, Closing, LastAck, TimeWait, DeleteTcb
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MIB_TCPROW_OWNER_PID
    {
        public MibTcpState state;
        public uint localAddr;
        public uint localPort;
        public uint remoteAddr;
        public uint remotePort;
        public uint owningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        public MIB_TCPROW_OWNER_PID table;
    }

    class NativeMethods
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern uint GetExtendedTcpTable(
            IntPtr pTcpTable,
            ref int dwOutBufLen,
            bool sort,
            int ipVersion,
            int tblClass,
            uint reserved);

        public const int AF_INET = 2;
        public const int TCP_TABLE_OWNER_PID_ALL = 5;
    }

    public class KeyInterceptor
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static byte[]? aesKey;
        private static byte[]? aesIV;

        private static readonly string[] TrustedProcesses = new[]
        {
            "explorer", "chrome", "msedge", "discord", "steam", "NCKeys"
        };
        public static bool PrivacyModeEnabled { get; set; } = false;
        public static bool ClipboardProtectionEnabled { get; set; } = false;

        private static System.Windows.Forms.Timer? _realtimeTimer;
        private static readonly HashSet<int> ScannedPIDs = new();
        private static readonly Dictionary<string, bool> TrustedPublisherCache = new();
        private static readonly Dictionary<string, string> KnownHashes = new();

        private static System.Timers.Timer? _hookRecoveryTimer;

        public static event Action<string>? OnEncryptedKey;
        public static event Action<string>? OnSuspiciousProcessDetected;

        static KeyInterceptor()
        {
            using var aes = Aes.Create();
            aesKey = aes?.Key;
            aesIV = aes?.IV;

            _hookRecoveryTimer = new System.Timers.Timer(5000);
            _hookRecoveryTimer.Elapsed += (s, e) =>
            {
                if (_hookID == IntPtr.Zero)
                    _hookID = SetHook(_proc);
            };
            _hookRecoveryTimer.Start();
        }

        public static void Start()
        {
            if (_hookID == IntPtr.Zero)
                _hookID = SetHook(_proc);

            StartRealtimeScan();
        }

        public static void Stop()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }

            StopRealtimeScan();
        }

        private static void StartRealtimeScan()
        {
            _realtimeTimer ??= new System.Windows.Forms.Timer();
            _realtimeTimer.Interval = 2000;
            _realtimeTimer.Tick += (s, e) =>
            {
                Task.Run(() =>
                {
                    var processes = Process.GetProcesses();
                    foreach (var p in processes)
                    {
                        try
                        {
                            if (ScannedPIDs.Contains(p.Id)) continue;
                            ScannedPIDs.Add(p.Id);

                            if (!TrustedProcesses.Contains(p.ProcessName) && IsSuspiciousProcess(p))
                                OnSuspiciousProcessDetected?.Invoke($"{p.ProcessName} (PID {p.Id})");
                        }
                        catch { }
                    }
                });

                MonitorClipboard();
            };
            _realtimeTimer.Start();
        }

        private static void StopRealtimeScan()
        {
            _realtimeTimer?.Stop();
            _realtimeTimer?.Dispose();
            _realtimeTimer = null;
            ScannedPIDs.Clear();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule? curModule = curProcess.MainModule;
            return curModule != null
                ? SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0)
                : IntPtr.Zero;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                if (PrivacyModeEnabled)
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);

                int vkCode = Marshal.ReadInt32(lParam);
                string keyStr = MapKey(vkCode);
                if (!string.IsNullOrEmpty(keyStr))
                {
                    string encrypted = EncryptKey(keyStr);
                    if (!string.IsNullOrEmpty(encrypted))
                        OnEncryptedKey?.Invoke(encrypted);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void MonitorClipboard()
        {
            if (!ClipboardProtectionEnabled) return;
            if (Clipboard.ContainsText())
                Clipboard.Clear();
        }

        private static string MapKey(int vkCode)
        {
            Keys key = (Keys)vkCode;
            bool shiftPressed = (GetKeyState((int)Keys.ShiftKey) & 0x8000) != 0;
            bool capsLock = (GetKeyState((int)Keys.CapsLock) & 1) != 0;

            string result = key switch
            {
                Keys.Back => "[BACK]",
                Keys.Enter => "[ENTER]",
                Keys.Space => " ",
                Keys.Tab => "[TAB]",
                Keys.ShiftKey or Keys.ControlKey or Keys.Menu => "",
                Keys.CapsLock => "[CAPSLOCK]",
                Keys.NumLock => "[NUMLOCK]",
                _ => key.ToString()
            };

            if (result.Length == 1 && char.IsLetter(result[0]))
                result = (capsLock ^ shiftPressed) ? result.ToUpper() : result.ToLower();

            return result;
        }

        private static string EncryptKey(string key)
        {
            if (aesKey == null || aesIV == null) return string.Empty;

            try
            {
                using Aes aes = Aes.Create()!;
                aes.Key = aesKey;
                aes.IV = aesIV;
                ICryptoTransform encryptor = aes.CreateEncryptor();
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] encrypted = encryptor.TransformFinalBlock(keyBytes, 0, keyBytes.Length);
                return Convert.ToBase64String(encrypted);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool IsSuspiciousProcess(Process p)
        {
            try
            {
                string path = p.MainModule?.FileName ?? "";
                if (string.IsNullOrEmpty(path)) return false;

                if (!IsTrustedPublisher(path)) return true;

                if (ProcessHasTcpConnection(p.Id)) return true;

                string hash = ComputeFileHash(path);
                if (KnownHashes.TryGetValue(p.ProcessName.ToLowerInvariant(), out string? knownHash))
                {
                    if (hash is not null && hash != knownHash)
                        return true;
                }


                return false;
            }
            catch { return false; }
        }

        private static bool IsTrustedPublisher(string filePath)
        {
            if (TrustedPublisherCache.TryGetValue(filePath, out bool cached)) return cached;

            bool result = false;
            try
            {
                var cert = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadCertificateFromFile(filePath);
                string subject = cert?.Subject?.ToLowerInvariant() ?? "";
                result = subject.Contains("microsoft corporation") ||
                         subject.Contains("nvidia corporation") ||
                         subject.Contains("google llc") ||
                         subject.Contains("discord inc") ||
                         subject.Contains("valve") ||
                         subject.Contains("intel corporation") ||
                         subject.Contains("realtek semiconductor");
            }
            catch { result = false; }

            TrustedPublisherCache[filePath] = result;
            return result;
        }

        private static bool ProcessHasTcpConnection(int pid)
        {
            int buffSize = 0;
            NativeMethods.GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, 2, NativeMethods.TCP_TABLE_OWNER_PID_ALL, 0);
            IntPtr tcpTablePtr = Marshal.AllocHGlobal(buffSize);

            try
            {
                if (NativeMethods.GetExtendedTcpTable(tcpTablePtr, ref buffSize, true, 2, NativeMethods.TCP_TABLE_OWNER_PID_ALL, 0) == 0)
                {
                    int rowSize = Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();
                    int numEntries = Marshal.ReadInt32(tcpTablePtr);
                    IntPtr rowPtr = (IntPtr)((long)tcpTablePtr + 4);

                    for (int i = 0; i < numEntries; i++)
                    {
                        var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                        if (row.owningPid == pid && row.state == MibTcpState.Established)
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

        private static string ComputeFileHash(string path)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = System.IO.File.OpenRead(path);
                byte[] hash = sha256.ComputeHash(stream);
                return Convert.ToBase64String(hash);
            }
            catch { return string.Empty; }
        }

        #region DLLImports
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int keyCode);
        #endregion
    }
}
