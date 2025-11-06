namespace PyClickerRecorder
{
    partial class ActivationForm
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
            this.infoLabel = new System.Windows.Forms.Label();
            this.machineIdLabel = new System.Windows.Forms.Label();
            this.machineIdTextBox = new System.Windows.Forms.TextBox();
            this.licenseKeyLabel = new System.Windows.Forms.Label();
            this.licenseKeyTextBox = new System.Windows.Forms.TextBox();
            this.activateButton = new System.Windows.Forms.Button();
            this.copyIdButton = new System.Windows.Forms.Button();
            this.CopyEmailButton = new System.Windows.Forms.Button();
            this.supportLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // infoLabel
            // 
            this.infoLabel.Location = new System.Drawing.Point(12, 9);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(426, 40);
            this.infoLabel.TabIndex = 0;
            this.infoLabel.Text = "Please copy the ID and send it to the support email address to get a license key.";
            // 
            // machineIdLabel
            // 
            this.machineIdLabel.AutoSize = true;
            this.machineIdLabel.Location = new System.Drawing.Point(12, 59);
            this.machineIdLabel.Name = "machineIdLabel";
            this.machineIdLabel.Size = new System.Drawing.Size(150, 20);
            this.machineIdLabel.TabIndex = 1;
            this.machineIdLabel.Text = "ID:";
            // 
            // machineIdTextBox
            // 
            this.machineIdTextBox.Location = new System.Drawing.Point(12, 82);
            this.machineIdTextBox.Multiline = true;
            this.machineIdTextBox.Name = "machineIdTextBox";
            this.machineIdTextBox.ReadOnly = true;
            this.machineIdTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.machineIdTextBox.Size = new System.Drawing.Size(426, 80);
            this.machineIdTextBox.TabIndex = 2;
            // 
            // copyIdButton
            // 
            this.copyIdButton.Location = new System.Drawing.Point(344, 168);
            this.copyIdButton.Name = "copyIdButton";
            this.copyIdButton.Size = new System.Drawing.Size(94, 29);
            this.copyIdButton.TabIndex = 6;
            this.copyIdButton.Text = "Copy ID";
            this.copyIdButton.UseVisualStyleBackColor = true;
            //
            // licenseKeyLabel
            // 
            this.licenseKeyLabel.AutoSize = true;
            this.licenseKeyLabel.Location = new System.Drawing.Point(12, 210);
            this.licenseKeyLabel.Name = "licenseKeyLabel";
            this.licenseKeyLabel.Size = new System.Drawing.Size(89, 20);
            this.licenseKeyLabel.TabIndex = 3;
            this.licenseKeyLabel.Text = "License Key:";
            // 
            // licenseKeyTextBox
            // 
            this.licenseKeyTextBox.Location = new System.Drawing.Point(107, 207);
            this.licenseKeyTextBox.Name = "licenseKeyTextBox";
            this.licenseKeyTextBox.Size = new System.Drawing.Size(331, 27);
            this.licenseKeyTextBox.TabIndex = 4;
            // 
            // activateButton
            // 
            this.activateButton.Location = new System.Drawing.Point(175, 240);
            this.activateButton.Name = "activateButton";
            this.activateButton.Size = new System.Drawing.Size(94, 29);
            this.activateButton.TabIndex = 5;
            this.activateButton.Text = "Activate";
            this.activateButton.UseVisualStyleBackColor = true;
            //
            // supportLabel
            //
            this.supportLabel.AutoSize = true;
            this.supportLabel.Location = new System.Drawing.Point(12, 280);
            this.supportLabel.Name = "supportLabel";
            this.supportLabel.Size = new System.Drawing.Size(200, 20);
            this.supportLabel.TabIndex = 7;
            this.supportLabel.Text = "pyclickermanager.material438@slmails.com";
            //
            //CopyEmailButton
            //
            this.CopyEmailButton.Location = new System.Drawing.Point(344, 280);
            this.CopyEmailButton.Name = "CopyEmailButton";
            this.CopyEmailButton.Size = new System.Drawing.Size(110, 29);
            this.CopyEmailButton.TabIndex = 8;
            this.CopyEmailButton.Text = "Copy Email";
            this.CopyEmailButton.UseVisualStyleBackColor = true;
            // 
            // ActivationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 315);
            this.Controls.Add(this.supportLabel);
            this.Controls.Add(this.copyIdButton);
            this.Controls.Add(this.CopyEmailButton);
            this.Controls.Add(this.activateButton);
            this.Controls.Add(this.licenseKeyTextBox);
            this.Controls.Add(this.licenseKeyLabel);
            this.Controls.Add(this.machineIdTextBox);
            this.Controls.Add(this.machineIdLabel);
            this.Controls.Add(this.infoLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ActivationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Activate PyClickerRecorder";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Label machineIdLabel;
        private System.Windows.Forms.TextBox machineIdTextBox;
        private System.Windows.Forms.Label licenseKeyLabel;
        private System.Windows.Forms.TextBox licenseKeyTextBox;
        private System.Windows.Forms.Button activateButton;
        private System.Windows.Forms.Button copyIdButton;
        private System.Windows.Forms.Button CopyEmailButton;
        private System.Windows.Forms.Label supportLabel;
    }
}
