namespace KeyGenerator
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.machineIdLabel = new System.Windows.Forms.Label();
            this.machineIdTextBox = new System.Windows.Forms.TextBox();
            this.licenseKeyLabel = new System.Windows.Forms.Label();
            this.licenseKeyTextBox = new System.Windows.Forms.TextBox();
            this.generateButton = new System.Windows.Forms.Button();
            this.publicKeyLabel = new System.Windows.Forms.Label();
            this.publicKeyTextBox = new System.Windows.Forms.TextBox();
            this.copyPublicKeyButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // machineIdLabel
            // 
            this.machineIdLabel.AutoSize = true;
            this.machineIdLabel.Location = new System.Drawing.Point(12, 15);
            this.machineIdLabel.Name = "machineIdLabel";
            this.machineIdLabel.Size = new System.Drawing.Size(121, 20);
            this.machineIdLabel.TabIndex = 0;
            this.machineIdLabel.Text = "Encrypted Machine ID:";
            // 
            // machineIdTextBox
            // 
            this.machineIdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.machineIdTextBox.Location = new System.Drawing.Point(139, 12);
            this.machineIdTextBox.Name = "machineIdTextBox";
            this.machineIdTextBox.Size = new System.Drawing.Size(433, 27);
            this.machineIdTextBox.TabIndex = 1;
            // 
            // generateButton
            // 
            this.generateButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.generateButton.Location = new System.Drawing.Point(12, 45);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(560, 29);
            this.generateButton.TabIndex = 4;
            this.generateButton.Text = "Generate License Key";
            this.generateButton.UseVisualStyleBackColor = true;
            //
            // licenseKeyLabel
            // 
            this.licenseKeyLabel.AutoSize = true;
            this.licenseKeyLabel.Location = new System.Drawing.Point(12, 84);
            this.licenseKeyLabel.Name = "licenseKeyLabel";
            this.licenseKeyLabel.Size = new System.Drawing.Size(121, 20);
            this.licenseKeyLabel.TabIndex = 2;
            this.licenseKeyLabel.Text = "Generated License Key:";
            // 
            // licenseKeyTextBox
            // 
            this.licenseKeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.licenseKeyTextBox.Location = new System.Drawing.Point(12, 107);
            this.licenseKeyTextBox.Name = "licenseKeyTextBox";
            this.licenseKeyTextBox.ReadOnly = true;
            this.licenseKeyTextBox.Size = new System.Drawing.Size(560, 27);
            this.licenseKeyTextBox.TabIndex = 3;
            //
            // publicKeyLabel
            //
            this.publicKeyLabel.AutoSize = true;
            this.publicKeyLabel.Location = new System.Drawing.Point(12, 150);
            this.publicKeyLabel.Name = "publicKeyLabel";
            this.publicKeyLabel.Size = new System.Drawing.Size(270, 20);
            this.publicKeyLabel.TabIndex = 5;
            this.publicKeyLabel.Text = "Public Key (copy this into the main app):";
            // 
            // publicKeyTextBox
            // 
            this.publicKeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.publicKeyTextBox.Location = new System.Drawing.Point(12, 173);
            this.publicKeyTextBox.Multiline = true;
            this.publicKeyTextBox.Name = "publicKeyTextBox";
            this.publicKeyTextBox.ReadOnly = true;
            this.publicKeyTextBox.Size = new System.Drawing.Size(560, 65);
            this.publicKeyTextBox.TabIndex = 6;
            //
            // copyPublicKeyButton
            //
            this.copyPublicKeyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.copyPublicKeyButton.Location = new System.Drawing.Point(422, 244);
            this.copyPublicKeyButton.Name = "copyPublicKeyButton";
            this.copyPublicKeyButton.Size = new System.Drawing.Size(150, 29);
            this.copyPublicKeyButton.TabIndex = 7;
            this.copyPublicKeyButton.Text = "Copy Public Key";
            this.copyPublicKeyButton.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 285);
            this.Controls.Add(this.copyPublicKeyButton);
            this.Controls.Add(this.publicKeyTextBox);
            this.Controls.Add(this.publicKeyLabel);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.licenseKeyTextBox);
            this.Controls.Add(this.licenseKeyLabel);
            this.Controls.Add(this.machineIdTextBox);
            this.Controls.Add(this.machineIdLabel);
            this.MinimumSize = new System.Drawing.Size(500, 330);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PyClickerRecorder Key Generator";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label machineIdLabel;
        private System.Windows.Forms.TextBox machineIdTextBox;
        private System.Windows.Forms.Label licenseKeyLabel;
        private System.Windows.Forms.TextBox licenseKeyTextBox;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.Label publicKeyLabel;
        private System.Windows.Forms.TextBox publicKeyTextBox;
        private System.Windows.Forms.Button copyPublicKeyButton;
    }
}
