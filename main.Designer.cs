using System;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;

namespace NCKeys
{
    partial class Main
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox txtOutput;
        private Button btnStartHook;
        private Button btnStopHook;
        private Button btnSettings;
        private Button btnScan;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Label lblKeysCaptured;
        private Label lblMemoryUsage;
        private System.Windows.Forms.Timer updateTimer;
        private Button btnWhitelist;
        private Button btnTerms;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ClientSize = new Size(500, 600);
            this.Text = "NCKeys – Anti-Keylogger Tool";
            this.BackColor = Color.FromArgb(30, 30, 47);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = Properties.Resources.favicon;

            // --------------------
            // Main layout
            // --------------------
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.FromArgb(30, 30, 47)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 65f)); // Output box
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25f)); // Buttons grid
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 10f)); // Labels row

            // --------------------
            // Output TextBox
            // --------------------
            txtOutput = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 20, 35),
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 10, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainLayout.Controls.Add(txtOutput, 0, 0);

            // --------------------
            // Buttons grid
            // --------------------
            TableLayoutPanel buttonsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(30, 30, 47)
            };
            buttonsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            buttonsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            buttonsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
            buttonsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
            buttonsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 34f));

            btnScan = CreateModernIconButton("Run Security Scan", IconChar.Search);
            btnSettings = CreateModernIconButton("Settings", IconChar.Cogs);
            btnStartHook = CreateModernIconButton("Start Protection", IconChar.Lock);
            btnStopHook = CreateModernIconButton("Stop Protection", IconChar.Unlock);
            btnWhitelist = CreateModernIconButton("Whitelist", IconChar.ListCheck);
            btnTerms = CreateModernIconButton("Terms of Use", IconChar.FileContract);
            btnStopHook.Enabled = false;

            buttonsGrid.Controls.Add(btnScan, 0, 0);
            buttonsGrid.Controls.Add(btnSettings, 0, 1);
            buttonsGrid.Controls.Add(btnStartHook, 1, 0);
            buttonsGrid.Controls.Add(btnStopHook, 1, 1);
            buttonsGrid.Controls.Add(btnWhitelist, 0, 2);
            buttonsGrid.Controls.Add(btnTerms, 1, 2);

            mainLayout.Controls.Add(buttonsGrid, 0, 1);

            // --------------------
            // Progress bar
            // --------------------
            progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Blocks,
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.LimeGreen,
                Value = 0
            };

            // --------------------
            // Labels row
            // --------------------
            TableLayoutPanel labelsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.FromArgb(30, 30, 47)
            };
            labelsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            labelsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            labelsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));

            lblStatus = new Label
            {
                ForeColor = Color.White,
                Text = "Status: Idle",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            };

            lblKeysCaptured = new Label
            {
                ForeColor = Color.White,
                Text = "Keys Captured: 0",
                Anchor = AnchorStyles.None,
                AutoSize = true
            };

            lblMemoryUsage = new Label
            {
                ForeColor = Color.White,
                Text = "Memory: 0 MB",
                Anchor = AnchorStyles.Right,
                AutoSize = true
            };

            labelsPanel.Controls.Add(lblStatus, 0, 0);
            labelsPanel.Controls.Add(lblKeysCaptured, 1, 0);
            labelsPanel.Controls.Add(lblMemoryUsage, 2, 0);

            mainLayout.Controls.Add(progressBar, 0, 2);
            mainLayout.Controls.Add(labelsPanel, 0, 2);

            // --------------------
            // Footer
            // --------------------
            Panel footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                BackColor = Color.FromArgb(30, 30, 47)
            };
            Label lblFooter = new Label
            {
                Text = "© 2025 NCDevs. All rights reserved. Developed by Noriel Calingasan.",
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8, FontStyle.Italic)
            };
            footerPanel.Controls.Add(lblFooter);
            this.Controls.Add(footerPanel);

            this.Controls.Add(mainLayout);
        }

        private IconButton CreateModernIconButton(string text, IconChar icon)
        {
            var btn = new IconButton
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                IconChar = icon,
                IconColor = Color.White,
                IconSize = 20,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Height = 26
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(70, 70, 100);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(40, 40, 60);
            return btn;
        }
    }
}
