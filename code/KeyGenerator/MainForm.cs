using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace KeyGenerator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            this.copyPublicKeyButton.Click += new System.EventHandler(this.copyPublicKeyButton_Click);

            try
            {
                // Display the public key for the admin to copy into the main application
                this.publicKeyTextBox.Text = LicenseGenerator.GetPublicKey();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A critical error occurred during key generation: {ex.Message}\n\nThe application will now close.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Close the form immediately if key generation fails.
                // Using Load event to close because constructor is too early.
                this.Load += (s, e) => this.Close();
            }
        }

        private void generateButton_Click(object? sender, EventArgs e)
        {
            string encryptedMachineId = this.machineIdTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(encryptedMachineId))
            {
                MessageBox.Show("Please enter the user's Encrypted Machine ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                this.licenseKeyTextBox.Text = LicenseGenerator.GenerateLicenseKey(encryptedMachineId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void copyPublicKeyButton_Click(object? sender, EventArgs e)
        {
            Clipboard.SetText(this.publicKeyTextBox.Text);
            MessageBox.Show("Public key copied to clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public static class LicenseGenerator
    {
        private const string PrivateKeyFileName = "private_key.b64";
        private static readonly RSA _privateKey;

        static LicenseGenerator()
        {
            // This static constructor is called once when the class is first accessed.
            if (File.Exists(PrivateKeyFileName))
            {
                // If a key file exists, load it.
                string privateKeyBase64 = File.ReadAllText(PrivateKeyFileName);
                _privateKey = RSA.Create();
                _privateKey.ImportRSAPrivateKey(Convert.FromBase64String(privateKeyBase64), out _);
            }
            else
            {
                // If no key file exists, generate a new one and save it.
                _privateKey = RSA.Create(2048); // Generate a new 2048-bit RSA key.
                byte[] privateKeyBytes = _privateKey.ExportRSAPrivateKey();
                string privateKeyBase64 = Convert.ToBase64String(privateKeyBytes);
                File.WriteAllText(PrivateKeyFileName, privateKeyBase64);
            }
        }

        public static string GetPublicKey()
        {
            // Export the public part of the key in Base64 format.
            byte[] publicKeyBytes = _privateKey.ExportRSAPublicKey();
            return Convert.ToBase64String(publicKeyBytes);
        }

        public static string GenerateLicenseKey(string encryptedMachineId)
        {
            // Decrypt the user's machine ID.
            byte[] encryptedBytes = Convert.FromBase64String(encryptedMachineId);
            byte[] decryptedBytes = _privateKey.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
            string machineId = Encoding.UTF8.GetString(decryptedBytes);

            // Sign the decrypted machine ID to create the license key.
            byte[] machineIdBytes = Encoding.UTF8.GetBytes(machineId);
            byte[] signature = _privateKey.SignData(machineIdBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }
    }
}
