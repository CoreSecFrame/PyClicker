using System;
using System.IO;
using System.Windows.Forms;

namespace PyClickerRecorder
{
    public partial class ActivationForm : Form
    {
        private static readonly string LicenseFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PyClickerRecorder", "license.key");

        public bool IsActivated { get; private set; }

        public ActivationForm()
        {
            InitializeComponent();
            LoadForm();
        }

        private void LoadForm()
        {
            machineIdTextBox.Text = LicenseManager.GetEncryptedMachineId();
            this.activateButton.Click += ActivateButton_Click;
            this.copyIdButton.Click += CopyIdButton_Click;
            this.CopyEmailButton.Click += CopyEmailButton_Click;
        }

        private void ActivateButton_Click(object? sender, EventArgs e)
        {
            string key = licenseKeyTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show("Please enter a license key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (LicenseManager.IsLicenseValid(key))
            {
                SaveLicenseKey(key);
                IsActivated = true;
                MessageBox.Show("Activation successful! Thank you.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("The license key is invalid. Please check the key and try again.", "Activation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void CopyEmailButton_Click(object? sender, EventArgs e)
        {
            Clipboard.SetText(supportLabel.Text);
            MessageBox.Show("Email copied to clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CopyIdButton_Click(object? sender, EventArgs e)
        {
            Clipboard.SetText(machineIdTextBox.Text);
            MessageBox.Show("ID copied to clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static bool CheckLicense()
        {
            if (!File.Exists(LicenseFile))
            {
                return false;
            }

            string storedKey = File.ReadAllText(LicenseFile);
            return LicenseManager.IsLicenseValid(storedKey);
        }
        
        private void SaveLicenseKey(string key)
        {
            try
            {
                string? directory = Path.GetDirectoryName(LicenseFile);
                if (directory != null)
                {
                    Directory.CreateDirectory(directory);
                    File.WriteAllText(LicenseFile, key);
                }
                else
                {
                    throw new InvalidOperationException("Could not determine the directory for the license file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save license key: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
