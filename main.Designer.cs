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
        private System.Windows.Forms.Timer updateTimer;
        // Add this inside the partial class Main
        private Label lblMemoryUsage;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ClientSize = new Size(500, 550);
            this.Text = "NCKeys";
            this.BackColor = Color.FromArgb(30, 30, 47);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = Properties.Resources.favicon;

            // Main layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.FromArgb(30, 30, 47)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 10f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 10f));

            // Output box
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

            // Buttons grid
            TableLayoutPanel buttonsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(30, 30, 47)
            };
            buttonsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            buttonsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            buttonsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            buttonsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            // Buttons
            btnScan = CreateModernIconButton("Scan", IconChar.Search);
            btnSettings = CreateModernIconButton("Settings", IconChar.Cogs);
            btnStartHook = CreateModernIconButton("Start Protection", IconChar.Lock);
            btnStopHook = CreateModernIconButton("Stop Protection", IconChar.Unlock);
            btnStopHook.Enabled = false;

            buttonsGrid.Controls.Add(btnScan, 0, 0);
            buttonsGrid.Controls.Add(btnSettings, 0, 1);
            buttonsGrid.Controls.Add(btnStartHook, 1, 0);
            buttonsGrid.Controls.Add(btnStopHook, 1, 1);

            mainLayout.Controls.Add(buttonsGrid, 0, 1);

            // Progress bar
            progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Blocks,
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.LimeGreen,
                Value = 0
            };
            mainLayout.Controls.Add(progressBar, 0, 2);

            // Labels row
            TableLayoutPanel labelsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(30, 30, 47)
            };
            labelsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            labelsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            lblStatus = new Label
            {
                ForeColor = Color.White,
                Text = "Status: Stopped",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            lblKeysCaptured = new Label
            {
                ForeColor = Color.White,
                Text = "Keys Captured: 0",
                AutoSize = true,
                Anchor = AnchorStyles.Right
            };

            lblMemoryUsage = new Label
            {
                ForeColor = Color.White,
                Text = "Memory Used: 0 MB",
                AutoSize = true,
                Anchor = AnchorStyles.Right
            };
            // Add lblMemoryUsage to labelsPanel
            labelsPanel.Controls.Add(lblMemoryUsage, 1, 1); // second row, right side
            labelsPanel.RowCount = 2;
            labelsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            labelsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            labelsPanel.Controls.Add(lblStatus, 0, 0);
            labelsPanel.Controls.Add(lblKeysCaptured, 1, 0);

            mainLayout.Controls.Add(labelsPanel, 0, 3);
            this.Controls.Add(mainLayout);

            // Timer: update every second
            updateTimer = new System.Windows.Forms.Timer(this.components)
            {
                Interval = 1000 // 1000 ms = 1 second
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private IconButton CreateModernIconButton(string text, IconChar icon)
        {
            var btn = new IconButton()
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                IconChar = icon,
                IconColor = Color.White,
                IconSize = 24,
                TextImageRelation = TextImageRelation.ImageBeforeText
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(70, 70, 100);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(40, 40, 60);
            return btn;
        }
    }
}
