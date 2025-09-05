using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace NCKeys
{
    public partial class SettingsForm : Form
    {
        private GroupBox? grpGeneral, grpPrivacy, grpNotifications;
        private CheckBox? chkRunOnStartup, chkPrivacyMode, chkClipboardProtection, chkAlerts;
        private Button? btnSave, btnDefault;

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "Settings";
            this.Size = new Size(450, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 47);
            this.MaximizeBox = false;
            this.MinimizeBox = false; // remove minimize button
            this.ShowIcon = true;

            int groupSpacing = 20;

            // ----- General Group -----
            grpGeneral = new GroupBox
            {
                Text = "General",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 30, 47),
                Size = new Size(400, 60),
                Location = new Point(20, 20)
            };
            chkRunOnStartup = new CheckBox
            {
                Text = "Run on Windows Startup",
                ForeColor = Color.White,
                Location = new Point(20, 30),
                AutoSize = true
            };
            grpGeneral.Controls.Add(chkRunOnStartup);

            // ----- Privacy Group -----
            grpPrivacy = new GroupBox
            {
                Text = "Privacy Protection",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 30, 47),
                Size = new Size(400, 80),
                Location = new Point(20, grpGeneral.Bottom + groupSpacing)
            };
            chkPrivacyMode = new CheckBox
            {
                Text = "Enable Privacy Mode",
                ForeColor = Color.White,
                Location = new Point(20, 30),
                AutoSize = true
            };
            chkClipboardProtection = new CheckBox
            {
                Text = "Block Clipboard Uploads",
                ForeColor = Color.White,
                Location = new Point(20, 50),
                AutoSize = true
            };
            grpPrivacy.Controls.Add(chkPrivacyMode);
            grpPrivacy.Controls.Add(chkClipboardProtection);

            // ----- Notifications Group -----
            grpNotifications = new GroupBox
            {
                Text = "Notifications",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 30, 47),
                Size = new Size(400, 60),
                Location = new Point(20, grpPrivacy.Bottom + groupSpacing)
            };
            chkAlerts = new CheckBox
            {
                Text = "Show Alerts",
                ForeColor = Color.White,
                Location = new Point(20, 30),
                AutoSize = true
            };
            grpNotifications.Controls.Add(chkAlerts);

            // ----- Buttons -----
            btnSave = CreateButton("Save", new Point(50, grpNotifications.Bottom + 20), Color.FromArgb(45, 45, 70), BtnSave_Click);
            btnDefault = CreateButton("Default", new Point(250, grpNotifications.Bottom + 20), Color.FromArgb(70, 70, 100), BtnDefault_Click);

            this.Controls.AddRange(new Control[] { grpGeneral, grpPrivacy, grpNotifications, btnSave, btnDefault });

            // Load saved settings
            LoadSettings();
        }

        private Button CreateButton(string text, Point location, Color backColor, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(140, 40),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                Location = location
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            btn.Click += onClick;
            return btn;
        }

        private void LoadSettings()
        {
            chkRunOnStartup!.Checked = Properties.Settings.Default.RunOnStartup;
            chkPrivacyMode!.Checked = Properties.Settings.Default.PrivacyMode;
            chkClipboardProtection!.Checked = Properties.Settings.Default.ClipboardProtection;
            chkAlerts!.Checked = Properties.Settings.Default.ShowAlerts;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                SaveSettings();
                MessageBox.Show(
                    "Settings have been successfully saved.",
                    "NCKeys Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                this.DialogResult = DialogResult.OK; // Signals success to caller
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while saving settings:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        private void BtnDefault_Click(object? sender, EventArgs e)
        {
            chkRunOnStartup!.Checked = false;
            chkPrivacyMode!.Checked = true;
            chkClipboardProtection!.Checked = true;
            chkAlerts!.Checked = true;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.RunOnStartup = chkRunOnStartup!.Checked;
            Properties.Settings.Default.PrivacyMode = chkPrivacyMode!.Checked;
            Properties.Settings.Default.ClipboardProtection = chkClipboardProtection!.Checked;
            Properties.Settings.Default.ShowAlerts = chkAlerts!.Checked;
            Properties.Settings.Default.Save();

            ApplyStartupSetting(chkRunOnStartup!.Checked);
        }

        private void ApplyStartupSetting(bool enable)
        {
            try
            {
                string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(runKey, writable: true))
                {
                    if (key is null)
                    {
                        MessageBox.Show(
                            "Failed to open registry key for startup settings.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }

                    if (enable)
                    {
                        key.SetValue("NCKeys", Application.ExecutablePath);
                    }
                    else
                    {
                        key.DeleteValue("NCKeys", throwOnMissingValue: false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update startup setting: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
