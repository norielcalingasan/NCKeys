using FontAwesome.Sharp;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCKeys
{
    public partial class Main : Form
    {
        private NotifyIcon? trayIcon;
        private ContextMenuStrip? trayMenu;
        private double baselineMemoryMB = 0;

        public Main()
        {
            InitializeComponent();

            // Tray + wiring + timer
            InitializeTray();

            btnScan.Click += async (s, e) => await RunScanAsync();
            btnStartHook.Click += async (s, e) => await StartProtectionAsync();
            btnStopHook.Click += BtnStopHook_Click;
            btnSettings.Click += BtnSettings_Click;
            btnWhitelist.Click += BtnWhitelist_Click;
            btnTerms.Click += BtnTerms_Click;

            updateTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }



        // --------------------
        // Tray & Helpers
        // --------------------
        private void InitializeTray()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open", null, (s, e) => RestoreFromTray());

            var toggleProtection = new ToolStripMenuItem("Enable Protection")
            {
                Image = IconToBitmap(IconChar.Shield, 16, Color.DarkGreen),
                ForeColor = Color.DarkGreen
            };

            toggleProtection.Click += async (s, e) =>
            {
                if (btnStartHook.Enabled)
                {
                    await StartProtectionAsync();
                    toggleProtection.Text = "Disable Protection";
                    toggleProtection.Image = IconToBitmap(IconChar.Shield, 16, Color.Red);
                    toggleProtection.ForeColor = Color.Red;
                }
                else
                {
                    BtnStopHook_Click(null, EventArgs.Empty);
                    toggleProtection.Text = "Enable Protection";
                    toggleProtection.Image = IconToBitmap(IconChar.Shield, 16, Color.DarkGreen);
                    toggleProtection.ForeColor = Color.DarkGreen;
                }
            };

            trayMenu.Items.Add(toggleProtection);
            trayMenu.Items.Add("Exit", null, (s, e) =>
            {
                trayIcon?.Dispose();
                Application.Exit();
            });

            trayIcon = new NotifyIcon
            {
                Icon = Properties.Resources.favicon,
                Text = "NCKeys",
                Visible = true,
                ContextMenuStrip = trayMenu
            };
            trayIcon.DoubleClick += (s, e) => RestoreFromTray();
        }

        public static Bitmap IconToBitmap(IconChar icon, int size, Color color)
        {
            using var iconPic = new IconPictureBox
            {
                IconChar = icon,
                IconColor = color,
                IconSize = size,
                BackColor = Color.Transparent
            };
            var bmp = new Bitmap(size, size);
            iconPic.DrawToBitmap(bmp, new Rectangle(0, 0, size, size));
            return bmp;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized) HideToTray();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                HideToTray();
            }
            else
            {
                trayIcon?.Dispose();
                base.OnFormClosing(e);
            }
        }

        private void HideToTray()
        {
            this.Hide();
            this.ShowInTaskbar = false;
            if (trayIcon != null) trayIcon.Visible = true;
        }

        private void RestoreFromTray()
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        // --------------------
        // Timer
        // --------------------
        private void UpdateTimer_Tick(object? sender, EventArgs e) => UpdateMemoryUsage();

        private void UpdateMemoryUsage()
        {
            try
            {
                using var proc = Process.GetCurrentProcess();
                double currentMemoryMB = proc.PrivateMemorySize64 / (1024.0 * 1024.0);

                if (baselineMemoryMB > 0)
                {
                    double delta = currentMemoryMB - baselineMemoryMB;
                    lblMemoryUsage.Text = $"Memory Δ: {Math.Max(0, delta):F2} MB";
                }
                else lblMemoryUsage.Text = $"Memory: {currentMemoryMB:F2} MB";
            }
            catch { }
        }

        // --------------------
        // Buttons Async Helpers
        // --------------------
        private void SetButtonsEnabled(bool scan, bool start, bool stop, bool settings = true, bool whitelist = true, bool terms = true)
        {
            btnScan.Enabled = scan;
            btnStartHook.Enabled = start;
            btnStopHook.Enabled = stop;
            btnSettings.Enabled = settings;
            btnWhitelist.Enabled = whitelist;
            btnTerms.Enabled = terms;
        }

        private async Task RunScanAsync()
        {
            // Disable buttons during scan
            SetButtonsEnabled(false, false, false);
            txtOutput.Clear();
            lblStatus.Text = "Scanning...";
            lblStatus.ForeColor = Color.Orange;

            IProgress<string> outputReporter = new Progress<string>(line =>
            {
                txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] {line}\r\n");
                txtOutput.ScrollToCaret();
            });

            IProgress<int> progressReporter = new Progress<int>(val => progressBar.Value = val);

            try
            {
                await Task.Run(async () =>
                {
                    outputReporter.Report("🔍 Starting scan...");
                    var processes = ProcessScanner.GetSuspiciousProcessesSummary();

                    if (processes.Length == 0)
                    {
                        outputReporter.Report("✅ No suspicious processes detected.");
                        progressReporter.Report(100);
                    }
                    else
                    {
                        for (int i = 0; i < processes.Length; i++)
                        {
                            outputReporter.Report($"⚠️ {processes[i]}");
                            progressReporter.Report((int)((i + 1) / (double)processes.Length * 100));
                            await Task.Delay(20); // non-blocking async delay
                        }
                    }

                    outputReporter.Report("🔍 Scan complete.");
                    progressReporter.Report(100);
                });


                // Update status label after scan
                bool hasAlert = txtOutput.Text.Contains("⚠️");
                lblStatus.Text = hasAlert ? "Scan Alert" : "Scan OK";
                lblStatus.ForeColor = hasAlert ? Color.Red : Color.LimeGreen;
            }
            catch (Exception ex)
            {
                outputReporter.Report($"❌ Scan failed: {ex.Message}");
                lblStatus.Text = "Scan Failed";
                lblStatus.ForeColor = Color.Red;
            }
            finally
            {
                // Enable Scan button and Start Protection button
                SetButtonsEnabled(true, true, btnStopHook.Enabled);
            }
        }

        private async Task StartProtectionAsync()
        {
            SetButtonsEnabled(false, false, false);
            lblStatus.Text = "Starting protection...";
            lblStatus.ForeColor = Color.Orange;
            progressBar.Value = 0;

            IProgress<string> outputReporter = new Progress<string>(line =>
            {
                txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] {line}\r\n");
                txtOutput.ScrollToCaret();
            });

            IProgress<int> progressReporter = new Progress<int>(val => progressBar.Value = val);

            try
            {
                await Task.Run(() =>
                {
                    outputReporter.Report("🛡 Initializing protection...");
                    for (int i = 0; i <= 100; i += 5)
                    {
                        progressReporter.Report(i);
                        if (i % 25 == 0) outputReporter.Report($"🔹 Progress: {i}%");
                        System.Threading.Thread.Sleep(20);
                    }

                    KeyInterceptor.Start();
                    outputReporter.Report("✔ Protection active.");
                });

                using var proc = Process.GetCurrentProcess();
                baselineMemoryMB = proc.PrivateMemorySize64 / (1024.0 * 1024.0);

                lblMemoryUsage.Text = "Memory Δ: 0 MB";
                lblStatus.Text = "Protection On";
                lblStatus.ForeColor = Color.LimeGreen;
                progressBar.Value = 100;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Protection Failed";
                lblStatus.ForeColor = Color.Red;
                txtOutput.AppendText($"✘ Failed to start protection: {ex.Message}\r\n");
            }
            finally
            {
                SetButtonsEnabled(true, false, true);
            }
        }

        private void BtnStopHook_Click(object? sender, EventArgs e)
        {
            try
            {
                KeyInterceptor.Stop();
                baselineMemoryMB = 0;

                lblStatus.Text = "Protection Off";
                lblStatus.ForeColor = Color.Red;
                lblMemoryUsage.Text = "Memory: 0 MB";
                progressBar.Value = 0;

                txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] ✘ Protection stopped.\r\n");

                // Enable Scan and Start Protection buttons, disable Stop Protection
                SetButtonsEnabled(true, true, false);
            }
            catch (Exception ex)
            {
                txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] ✘ Failed to stop protection: {ex.Message}\r\n");
            }
        }


        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            using var settingsForm = new SettingsForm();
            settingsForm.ShowDialog(this);
        }

        private void BtnWhitelist_Click(object? sender, EventArgs e)
        {
            using var form = new WhitelistForm();
            form.ShowDialog(this);
        }

        private void BtnTerms_Click(object? sender, EventArgs e)
        {
            var termsForm = new TermsForm();
            termsForm.ShowCloseOnly();
            termsForm.ShowDialog(this);
        }
    }
}
