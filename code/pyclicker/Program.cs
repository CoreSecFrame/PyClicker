namespace PyClickerRecorder
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            if (ActivationForm.CheckLicense())
            {
                Application.Run(new MainForm());
            }
            else
            {
                using (var activationForm = new ActivationForm())
                {
                    if (activationForm.ShowDialog() == DialogResult.OK && activationForm.IsActivated)
                    {
                        Application.Run(new MainForm());
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }
        }
    }
}
