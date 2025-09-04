using System;
using System.Drawing;
using System.Windows.Forms;

namespace NCKeys
{
    partial class WhitelistForm
    {
        private System.ComponentModel.IContainer components = null;
        private ListBox listBoxWhitelist;
        private TextBox txtEntry;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnRestore;
        private Button btnBrowse;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Whitelist Manager";
            this.ClientSize = new Size(400, 370);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 47);
            this.Padding = new Padding(0, 10, 0, 0); // 10px top padding for all children
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ControlBox = true;       // only [X]
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;

            // --------------------
            // ListBox (whitelist entries)
            // --------------------
            listBoxWhitelist = new ListBox
            {
                Dock = DockStyle.Top,
                Height = 280,
                BackColor = Color.FromArgb(20, 20, 35),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10, FontStyle.Regular)
            };

            // --------------------
            // TextBox + Browse button in a panel
            // --------------------
            TableLayoutPanel entryPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 35,
                ColumnCount = 2,
                Margin = new Padding(0, 10, 0, 0),
                BackColor = Color.FromArgb(30, 30, 47)
            };
            entryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75f));
            entryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            txtEntry = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                PlaceholderText = "Enter process name or path..."
            };

            btnBrowse = CreateModernButton("Browse");
            btnBrowse.Dock = DockStyle.Fill;
            btnBrowse.Click += BtnBrowse_Click;

            entryPanel.Controls.Add(txtEntry, 0, 0);
            entryPanel.Controls.Add(btnBrowse, 1, 0);

            // --------------------
            // Buttons layout
            // --------------------
            TableLayoutPanel buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                RowCount = 1,
                Height = 45,
                BackColor = Color.FromArgb(30, 30, 47)
            };
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));

            btnAdd = CreateModernButton("Add");
            btnDelete = CreateModernButton("Delete Selected");
            btnRestore = CreateModernButton("Restore Defaults");

            btnAdd.Click += btnAdd_Click;
            btnDelete.Click += btnDelete_Click;
            btnRestore.Click += btnRestore_Click;

            buttonPanel.Controls.Add(btnAdd, 0, 0);
            buttonPanel.Controls.Add(btnDelete, 1, 0);
            buttonPanel.Controls.Add(btnRestore, 2, 0);

            // --------------------
            // Assemble controls
            // --------------------
            this.Controls.Add(buttonPanel);
            this.Controls.Add(entryPanel);
            this.Controls.Add(listBoxWhitelist);
        }

        private Button CreateModernButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold),
                Margin = new Padding(5),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;

            // Hover effect
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(70, 70, 100);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(40, 40, 60);

            return btn;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Executable or File";
                ofd.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                ofd.CheckFileExists = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtEntry.Text = ofd.FileName;
                }
            }
        }
    }
}
