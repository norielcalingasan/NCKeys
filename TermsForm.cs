using System;
using System.Drawing;
using System.Windows.Forms;

namespace NCKeys
{
    public class TermsForm : Form
    {
        private Panel headerPanel;
        private Label lblTitle;
        private Panel contentPanel;
        private RichTextBox rtbTerms;
        private Button btnAccept;
        private Button btnDecline;
        private Button btnClose;   // ✅ Declare Close button
        private FlowLayoutPanel buttonPanel;

        public TermsForm()
        {
            // Form properties
            this.Text = "";
            this.Size = new Size(650, 550);

            this.BackColor = Color.FromArgb(30, 30, 47);
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true; // Keeps it on top until decision
            this.StartPosition = FormStartPosition.CenterScreen;

            // --------------------
            // Header Panel
            // --------------------
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 70)
            };
            this.Controls.Add(headerPanel);

            lblTitle = new Label
            {
                Text = "⚖️ Terms of Use",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            headerPanel.Controls.Add(lblTitle);

            // ✅ Allow dragging from header panel
            headerPanel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(this.Handle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HTCAPTION, 0);
                }
            };

            // Optional: allow dragging anywhere on the form
            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(this.Handle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HTCAPTION, 0);
                }
            };

            // --------------------
            // Button Container Panel (bottom area)
            // --------------------
            Panel bottomContainer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 120, // matches button height comfortably
                Padding = new Padding(0, 15, 0, 20),
                BackColor = Color.FromArgb(30, 30, 47)
            };
            this.Controls.Add(bottomContainer);

            // Separator line
            Panel separator = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(70, 70, 90)
            };
            bottomContainer.Controls.Add(separator);

            // --------------------
            // Button Panel (centered)
            // --------------------
            buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            bottomContainer.Controls.Add(buttonPanel);

            btnDecline = new Button
            {
                Text = "Decline",
                Width = 150,
                Height = 50,
                BackColor = Color.FromArgb(130, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                Margin = new Padding(20, 15, 20, 10),
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = true
            };
            btnDecline.FlatAppearance.BorderSize = 0;
            btnDecline.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            btnAccept = new Button
            {
                Text = "Accept",
                Width = 150,
                Height = 50,
                BackColor = Color.FromArgb(70, 130, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                Margin = new Padding(20, 15, 20, 10),
                Enabled = false,
                Visible = false,
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = true
            };
            btnAccept.FlatAppearance.BorderSize = 0;
            btnAccept.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            // ✅ New Close button (for when opened inside app)
            btnClose = new Button
            {
                Text = "Close",
                Width = 150,
                Height = 50,
                BackColor = Color.FromArgb(70, 70, 130),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                Margin = new Padding(20, 15, 20, 10),
                Visible = false
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            // Default: add Accept + Decline (first launch)
            buttonPanel.Controls.Add(btnDecline);
            buttonPanel.Controls.Add(btnAccept);

            // Center buttons when layout changes
            buttonPanel.Layout += (s, e) =>
            {
                int totalWidth = 0;
                foreach (Control ctrl in buttonPanel.Controls)
                    if (ctrl.Visible) totalWidth += ctrl.Width + ctrl.Margin.Horizontal;

                buttonPanel.Padding = new Padding((buttonPanel.Width - totalWidth) / 2, 20, 0, 0);
            };

            // --------------------
            // Content Panel
            // --------------------
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 20, 20, 0), // bottom padding removed so text doesn’t clash with buttons
                AutoScroll = true
            };
            this.Controls.Add(contentPanel);
            this.Controls.SetChildIndex(contentPanel, 1); // keep it above bottomContainer

            rtbTerms = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 20, 35),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                Text = GetTermsText(),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            rtbTerms.VScroll += RtbTerms_VScroll;
            contentPanel.Controls.Add(rtbTerms);

            // Rounded corners
            this.Region = System.Drawing.Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, this.Width, this.Height, 20, 20));

            // ✅ Check on load if scrollbar exists
            this.Load += (s, e) => CheckIfScrollBarNeeded();
        }

        // ✅ Helper method to switch to Close-only mode
        public void ShowCloseOnly()
        {
            btnDecline.Visible = false;
            btnAccept.Visible = false;

            buttonPanel.Controls.Clear();
            btnClose.Visible = true;
            buttonPanel.Controls.Add(btnClose);
        }

        private void CheckIfScrollBarNeeded()
        {
            if (!rtbTerms.IsHandleCreated) return;

            int textHeight = TextRenderer.MeasureText(rtbTerms.Text, rtbTerms.Font,
                                new Size(rtbTerms.Width, int.MaxValue),
                                TextFormatFlags.WordBreak).Height;

            if (textHeight <= rtbTerms.ClientSize.Height)
            {
                btnAccept.Enabled = true;
                btnAccept.Visible = true;
                buttonPanel.PerformLayout();
            }
        }

        private void RtbTerms_VScroll(object? sender, EventArgs e)
        {
            int lastVisibleChar = rtbTerms.GetCharIndexFromPosition(new Point(1, rtbTerms.ClientRectangle.Bottom - 1));
            int lastVisibleLine = rtbTerms.GetLineFromCharIndex(lastVisibleChar);

            if (lastVisibleLine >= rtbTerms.Lines.Length - 1)
            {
                btnAccept.Enabled = true;
                btnAccept.Visible = true;
                buttonPanel.PerformLayout();
            }
        }

        private string GetTermsText()
        {
            return @"

Important: By using NCKeys, you acknowledge and agree to the following terms. If you do not agree, do not use this software.

1. Personal & Authorized Use Only
NCKeys is intended solely for personal security, privacy protection, and educational purposes. 
Do not install, run, or use this software on devices that you do not own or do not have explicit written permission to monitor.

2. Consent Requirement
Before using NCKeys on any shared, public, or third-party device, you must obtain the explicit consent of all users of that device. 
Unauthorized use may violate local, national, or international laws, including computer crime and privacy regulations.

3. Compliance with Laws
You are fully responsible for complying with all applicable laws regarding:
   - Computer monitoring
   - Keylogging
   - Data privacy
   - Cybersecurity  
This includes, but is not limited to:
   - Republic Act No. 10173 – Data Privacy Act of 2012 (Philippines)
   - Republic Act No. 8792 – E-Commerce Act
   - Cybercrime Prevention Act of 2012 (Republic Act No. 10175)
   - Local and international computer misuse, privacy, and cybersecurity laws

4. Prohibited Use
NCKeys must NOT be used to:
   - Steal passwords, credentials, or personally identifiable information
   - Intercept private communications without consent
   - Monitor, spy on, or access third-party devices without authorization  
Any violation may result in criminal or civil liability.

5. No Warranty / Disclaimer of Liability
NCKeys is provided 'as-is' without warranties of any kind, express or implied.  
The developer (Noriel Calingasan) and associated parties are explicitly **not responsible** for:
   - Misuse or unauthorized use of the software
   - Loss, theft, or corruption of data
   - Legal consequences or disputes arising from your actions
   - Any damage to hardware or software caused by use of this software

6. Security & Ethical Use
NCKeys is designed for:
   - Security enthusiasts
   - IT professionals
   - Developers learning about process monitoring and keylogging protection  
It is explicitly **not a hacking tool**. Users must always use NCKeys ethically and legally.

7. Acceptance & Responsibility
By installing, launching, or using NCKeys, you confirm that you:
   - Have read, understood, and agreed to these terms
   - Accept full responsibility for your actions and legal compliance
   - Will immediately uninstall and cease use if you do not agree with these terms

8. Governing Law
These terms are governed by Philippine law, and applicable local and international computer security regulations.  
All disputes or claims related to the use of NCKeys are subject to the exclusive jurisdiction of the relevant courts in the Philippines.
";
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern IntPtr CreateRoundRectRgn(
                int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
                int nWidthEllipse, int nHeightEllipse);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ReleaseCapture();

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

            public const int WM_NCLBUTTONDOWN = 0xA1;
            public const int HTCAPTION = 0x2;
        }
    }
}
