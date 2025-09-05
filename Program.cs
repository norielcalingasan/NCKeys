using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace NCKeys
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Ensure only one instance is running
            using (Mutex mutex = new Mutex(true, "NCKeysMutex", out bool isNewInstance))
            {
                if (!isNewInstance)
                {
                    MessageBox.Show("NCKeys is already running.", "NCKeys",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ✅ Hide from taskbar by running inside ApplicationContext
                Application.Run(new TermsAppContext());
            }
        }
    }

    // Custom ApplicationContext to control flow
    internal class TermsAppContext : ApplicationContext
    {
        public TermsAppContext()
        {
            TermsForm termsForm = new TermsForm();
            termsForm.FormClosed += (s, e) =>
            {
                if (termsForm.DialogResult == DialogResult.OK)
                {
                    // Log acceptance
                    LogAcceptance();

                    // Run the main app if accepted
                    Main mainForm = new Main();
                    mainForm.FormClosed += (s2, e2) => ExitThread();
                    mainForm.Show();
                }
                else
                {
                    // Exit if declined
                    ExitThread();
                }
            };
            termsForm.Show();
        }

        private void LogAcceptance()
        {
            try
            {
                string logDir = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Logs",
                    "Terms"
                );

                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                // 📌 Group logs by day (e.g., Accepted_20250905.log)
                string logFile = Path.Combine(logDir, $"Accepted_{DateTime.Now:yyyyMMdd}.log");

                string logContent = $@"
===============================
Timestamp : {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Machine   : {Environment.MachineName}
User      : {Environment.UserName}
Domain    : {Environment.UserDomainName}
OS        : {Environment.OSVersion}
64-bit OS : {Environment.Is64BitOperatingSystem}
Processor : {Environment.ProcessorCount} logical cores
CLR       : {Environment.Version}
";

                // 📌 Append instead of overwrite
                File.AppendAllText(logFile, logContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠ Failed to write acceptance log: {ex.Message}",
                    "NCKeys Logging Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

    }
}
