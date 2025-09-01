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
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private double baselineMemoryMB = 0;

        public Main()
        {
            InitializeComponent();
            InitializeTray();

            btnScan.Click += BtnScan_Click;
            btnStartHook.Click += BtnStartHook_Click;
            btnStopHook.Click += BtnStopHook_Click;
            btnSettings.Click += BtnSettings_Click;
        }

        private void InitializeTray()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open", null, (s, e) => RestoreFromTray());

            var toggleProtection = new ToolStripMenuItem("Enable Protection")
            {
                Image = IconToBitmap(IconChar.Shield, 16, Color.DarkGreen),
                ForeColor = Color.DarkGreen
            };

            toggleProtection.Click += (s, e) =>
            {
                if (btnStartHook.Enabled)
                {
                    BtnStartHook_Click(null, EventArgs.Empty);
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
                trayIcon.Visible = false;
                trayIcon.Dispose();
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
            if (WindowState == FormWindowState.Minimized)
                HideToTray();
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
                trayIcon.Dispose();
                base.OnFormClosing(e);
            }
        }

        private void HideToTray()
        {
            this.Hide();
            this.ShowInTaskbar = false;
            trayIcon.Visible = true;
        }

        private void RestoreFromTray()
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateMemoryUsage();
        }

        private void UpdateMemoryUsage()
        {
            using var proc = Process.GetCurrentProcess();
            double currentMemoryMB = proc.PrivateMemorySize64 / (1024.0 * 1024.0);

            if (baselineMemoryMB > 0)
            {
                double delta = currentMemoryMB - baselineMemoryMB;
                if (delta < 0) delta = 0;
                lblMemoryUsage.Text = $"Memory Δ: {delta:F2} MB";
            }
            else
            {
                lblMemoryUsage.Text = $"Memory: {currentMemoryMB:F2} MB";
            }
        }

        private async void BtnScan_Click(object sender, EventArgs e)
        {
            btnScan.Enabled = false;
            txtOutput.Clear();
            lblStatus.Text = "Scanning...";
            lblStatus.ForeColor = Color.Orange;

            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            var progress = new Progress<int>(val =>
            {
                if (val <= progressBar.Maximum)
                    progressBar.Value = val;
            });

            try
            {
                string[] suspicious = await Task.Run(() =>
                {
                    var processes = ProcessScanner.GetSuspiciousProcessesSummary();
                    for (int i = 0; i <= 100; i += 5)
                    {
                        (progress as IProgress<int>)?.Report(i);
                        System.Threading.Thread.Sleep(30);
                    }
                    return processes;
                });

                if (suspicious.Length == 0)
                {
                    txtOutput.AppendText("✅ No suspicious processes detected.\r\n");
                    lblStatus.Text = "Scan OK";
                    lblStatus.ForeColor = Color.LimeGreen;
                }
                else
                {
                    txtOutput.AppendText("⚠️ Suspicious processes detected:\r\n");
                    foreach (var s in suspicious)
                        txtOutput.AppendText(" - " + s + "\r\n");
                    lblStatus.Text = "Scan Alert";
                    lblStatus.ForeColor = Color.Red;
                }

                progressBar.Value = progressBar.Maximum;
            }
            catch (Exception ex)
            {
                txtOutput.AppendText($"❌ Scan failed: {ex.Message}\r\n");
                lblStatus.Text = "Scan Failed";
                lblStatus.ForeColor = Color.Red;
            }
            finally
            {
                btnScan.Enabled = true;
            }
        }

        private async void BtnStartHook_Click(object sender, EventArgs e)
        {
            btnStartHook.Enabled = false;
            btnStopHook.Enabled = true;
            btnScan.Enabled = false;

            lblStatus.Text = "Starting...";
            lblStatus.ForeColor = Color.Orange;
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            txtOutput.AppendText("Initializing protection...\r\n");

            var progress = new Progress<int>(val => progressBar.Value = val);

            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i <= 100; i += 5)
                    {
                        (progress as IProgress<int>)?.Report(i);
                        System.Threading.Thread.Sleep(20);
                    }
                    KeyInterceptor.Start();
                });

                using var proc = Process.GetCurrentProcess();
                baselineMemoryMB = proc.PrivateMemorySize64 / (1024.0 * 1024.0);

                lblMemoryUsage.Text = $"Memory Δ : 0 MB";
                lblStatus.Text = "Protection On";
                lblStatus.ForeColor = Color.LimeGreen;
                progressBar.Value = 100;
                txtOutput.AppendText("✔ Protection active.\r\n");
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Protection Failed";
                lblStatus.ForeColor = Color.Red;
                txtOutput.AppendText($"✘ Failed to start protection: {ex.Message}\r\n");
                btnStartHook.Enabled = true;
                btnStopHook.Enabled = false;
                btnScan.Enabled = true;
            }
        }

        private void BtnStopHook_Click(object sender, EventArgs e)
        {
            btnStopHook.Enabled = false;
            btnStartHook.Enabled = true;
            btnScan.Enabled = true;

            try
            {
                KeyInterceptor.Stop();
                baselineMemoryMB = 0;

                lblStatus.Text = "Protection Off";
                lblStatus.ForeColor = Color.Red;
                progressBar.Value = 0;
                txtOutput.AppendText("✘ Protection stopped.\r\n");
            }
            catch (Exception ex)
            {
                txtOutput.AppendText($"✘ Failed to stop protection: {ex.Message}\r\n");
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm())
                settingsForm.ShowDialog(this);
        }
    }
}
