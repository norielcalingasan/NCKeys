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
                    MessageBox.Show("NCKeys is already running.", "NCKeys", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Run the app
                Application.Run(new Main());
            }
        }
    }
}
