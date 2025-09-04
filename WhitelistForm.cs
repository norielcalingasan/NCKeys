using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NCKeys
{
    public partial class WhitelistForm : Form
    {
        private readonly string whitelistPath;

        public WhitelistForm()
        {
            InitializeComponent();
            whitelistPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "whitelist.txt");

            if (!File.Exists(whitelistPath))
            {
                WriteDefaultsToFile();
            }

            LoadWhitelist();
        }

        private void LoadWhitelist()
        {
            listBoxWhitelist.Items.Clear();

            var lines = File.ReadAllLines(whitelistPath)
                            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"));

            foreach (var line in lines)
                listBoxWhitelist.Items.Add(line);
        }

        private void SaveWhitelist()
        {
            var lines = listBoxWhitelist.Items.Cast<string>()
                             .Where(l => !string.IsNullOrWhiteSpace(l))
                             .ToList();

            File.WriteAllLines(whitelistPath, lines);
        }


        private void WriteDefaultsToFile()
        {
            var lines = new System.Collections.Generic.List<string>();

            lines.Add("# Default Safe Processes");
            lines.AddRange(ProcessScanner.DefaultProcesses.OrderBy(x => x));
            lines.Add("");
            lines.Add("# Default Safe Paths");
            lines.AddRange(ProcessScanner.DefaultPaths.OrderBy(x => x));
            lines.Add("");
            lines.Add("# Add custom entries below");

            File.WriteAllLines(whitelistPath, lines);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtEntry.Text))
            {
                listBoxWhitelist.Items.Add(txtEntry.Text.Trim());
                txtEntry.Clear();
                SaveWhitelist();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listBoxWhitelist.SelectedItem != null)
            {
                listBoxWhitelist.Items.Remove(listBoxWhitelist.SelectedItem);
                SaveWhitelist();
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to restore defaults?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                WriteDefaultsToFile();
                LoadWhitelist();
            }
        }
    }
}
