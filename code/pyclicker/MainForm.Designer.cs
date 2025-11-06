namespace PyClickerRecorder
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
            components = new System.ComponentModel.Container();
            trayContextMenu = new ContextMenuStrip(components);
            showMenuItem = new ToolStripMenuItem();
            exitMenuItem = new ToolStripMenuItem();
            trayIcon = new NotifyIcon(components);
            menuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            loadMenuItem = new ToolStripMenuItem();
            saveMenuItem = new ToolStripMenuItem();
            mainTabControl = new TabControl();
            recordPlayTab = new TabPage();
            actionsListBox = new ListBox();
            playButton = new Button();
            recordButton = new Button();
            saveMacroButton = new Button();
            savedMacrosTab = new TabPage();
            savedMacrosListView = new ListView();
            newMacroButton = new Button();
            editMacroButton = new Button();
            deleteMacroButton = new Button();
            renameMacroButton = new Button();
            runMacroButton = new Button();
            settingsMacroButton = new Button();
            workflowsTab = new TabPage();
            workflowsListView = new ListView();
            newWorkflowButton = new Button();
            editWorkflowButton = new Button();
            deleteWorkflowButton = new Button();
            renameWorkflowButton = new Button();
            runWorkflowButton = new Button();
            scheduleWorkflowButton = new Button();
            userSettingsTab = new TabPage();
            darkModeCheckBox = new CheckBox();
            startWithWindowsCheckBox = new CheckBox();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            trayContextMenu.SuspendLayout();
            menuStrip.SuspendLayout();
            mainTabControl.SuspendLayout();
            recordPlayTab.SuspendLayout();
            savedMacrosTab.SuspendLayout();
            workflowsTab.SuspendLayout();
            userSettingsTab.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // trayContextMenu
            // 
            trayContextMenu.ImageScalingSize = new Size(20, 20);
            trayContextMenu.Items.AddRange(new ToolStripItem[] { showMenuItem, exitMenuItem });
            trayContextMenu.Name = "trayContextMenu";
            trayContextMenu.Size = new Size(104, 48);
            // 
            // showMenuItem
            // 
            showMenuItem.Name = "showMenuItem";
            showMenuItem.Size = new Size(103, 22);
            showMenuItem.Text = "Show";
            // 
            // exitMenuItem
            // 
            exitMenuItem.Name = "exitMenuItem";
            exitMenuItem.Size = new Size(103, 22);
            exitMenuItem.Text = "Exit";
            // 
            // trayIcon
            // 
            trayIcon.ContextMenuStrip = trayContextMenu;
            trayIcon.Text = "PyClicker - Pro";
            trayIcon.Visible = true;
            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new Size(20, 20);
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Padding = new Padding(5, 2, 0, 2);
            menuStrip.Size = new Size(555, 24);
            menuStrip.TabIndex = 2;
            menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadMenuItem, saveMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // loadMenuItem
            // 
            loadMenuItem.Name = "loadMenuItem";
            loadMenuItem.Size = new Size(123, 22);
            loadMenuItem.Text = "&Load...";
            // 
            // saveMenuItem
            // 
            saveMenuItem.Name = "saveMenuItem";
            saveMenuItem.Size = new Size(123, 22);
            saveMenuItem.Text = "Save &As...";
            // 
            // mainTabControl
            // 
            mainTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainTabControl.Controls.Add(recordPlayTab);
            mainTabControl.Controls.Add(savedMacrosTab);
            mainTabControl.Controls.Add(workflowsTab);
            mainTabControl.Controls.Add(userSettingsTab);
            mainTabControl.Location = new Point(10, 23);
            mainTabControl.Margin = new Padding(3, 2, 3, 2);
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new Size(534, 309);
            mainTabControl.TabIndex = 0;
            // 
            // recordPlayTab
            // 
            recordPlayTab.Controls.Add(actionsListBox);
            recordPlayTab.Controls.Add(playButton);
            recordPlayTab.Controls.Add(recordButton);
            recordPlayTab.Controls.Add(saveMacroButton);
            recordPlayTab.Location = new Point(4, 24);
            recordPlayTab.Margin = new Padding(3, 2, 3, 2);
            recordPlayTab.Name = "recordPlayTab";
            recordPlayTab.Padding = new Padding(3, 2, 3, 2);
            recordPlayTab.Size = new Size(526, 281);
            recordPlayTab.TabIndex = 0;
            recordPlayTab.Text = "Record & Play";
            recordPlayTab.UseVisualStyleBackColor = true;
            // 
            // actionsListBox
            // 
            actionsListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            actionsListBox.FormattingEnabled = true;
            actionsListBox.ItemHeight = 15;
            actionsListBox.Location = new Point(5, 44);
            actionsListBox.Margin = new Padding(3, 2, 3, 2);
            actionsListBox.Name = "actionsListBox";
            actionsListBox.Size = new Size(517, 229);
            actionsListBox.TabIndex = 2;
            // 
            // playButton
            // 
            playButton.Location = new Point(152, 11);
            playButton.Margin = new Padding(3, 2, 3, 2);
            playButton.Name = "playButton";
            playButton.Size = new Size(142, 28);
            playButton.TabIndex = 1;
            playButton.Text = "Start Playback (F7)";
            playButton.UseVisualStyleBackColor = true;
            // 
            // recordButton
            // 
            recordButton.Location = new Point(5, 11);
            recordButton.Margin = new Padding(3, 2, 3, 2);
            recordButton.Name = "recordButton";
            recordButton.Size = new Size(142, 28);
            recordButton.TabIndex = 0;
            recordButton.Text = "Start Recording (F6)";
            recordButton.UseVisualStyleBackColor = true;
            // 
            // saveMacroButton
            // 
            saveMacroButton.Location = new Point(299, 11);
            saveMacroButton.Margin = new Padding(3, 2, 3, 2);
            saveMacroButton.Name = "saveMacroButton";
            saveMacroButton.Size = new Size(142, 28);
            saveMacroButton.TabIndex = 3;
            saveMacroButton.Text = "Save Macro";
            saveMacroButton.UseVisualStyleBackColor = true;
            // 
            // savedMacrosTab
            // 
            savedMacrosTab.Controls.Add(savedMacrosListView);
            savedMacrosTab.Controls.Add(newMacroButton);
            savedMacrosTab.Controls.Add(editMacroButton);
            savedMacrosTab.Controls.Add(deleteMacroButton);
            savedMacrosTab.Controls.Add(renameMacroButton);
            savedMacrosTab.Controls.Add(runMacroButton);
            savedMacrosTab.Controls.Add(settingsMacroButton);
            savedMacrosTab.Location = new Point(4, 24);
            savedMacrosTab.Margin = new Padding(3, 2, 3, 2);
            savedMacrosTab.Name = "savedMacrosTab";
            savedMacrosTab.Padding = new Padding(3, 2, 3, 2);
            savedMacrosTab.Size = new Size(526, 281);
            savedMacrosTab.TabIndex = 2;
            savedMacrosTab.Text = "Saved Macros";
            savedMacrosTab.UseVisualStyleBackColor = true;
            // 
            // savedMacrosListView
            // 
            savedMacrosListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            savedMacrosListView.CheckBoxes = true;
            savedMacrosListView.Location = new Point(5, 4);
            savedMacrosListView.Margin = new Padding(3, 2, 3, 2);
            savedMacrosListView.Name = "savedMacrosListView";
            savedMacrosListView.Size = new Size(517, 241);
            savedMacrosListView.TabIndex = 0;
            savedMacrosListView.UseCompatibleStateImageBehavior = false;
            savedMacrosListView.View = View.Details;
            // 
            // newMacroButton
            // 
            newMacroButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            newMacroButton.Location = new Point(5, 249);
            newMacroButton.Margin = new Padding(3, 2, 3, 2);
            newMacroButton.Name = "newMacroButton";
            newMacroButton.Size = new Size(82, 22);
            newMacroButton.TabIndex = 1;
            newMacroButton.Text = "New";
            newMacroButton.UseVisualStyleBackColor = true;
            // 
            // editMacroButton
            // 
            editMacroButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            editMacroButton.Location = new Point(91, 249);
            editMacroButton.Margin = new Padding(3, 2, 3, 2);
            editMacroButton.Name = "editMacroButton";
            editMacroButton.Size = new Size(82, 22);
            editMacroButton.TabIndex = 2;
            editMacroButton.Text = "Edit";
            editMacroButton.UseVisualStyleBackColor = true;
            // 
            // deleteMacroButton
            // 
            deleteMacroButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            deleteMacroButton.Location = new Point(179, 249);
            deleteMacroButton.Margin = new Padding(3, 2, 3, 2);
            deleteMacroButton.Name = "deleteMacroButton";
            deleteMacroButton.Size = new Size(82, 22);
            deleteMacroButton.TabIndex = 3;
            deleteMacroButton.Text = "Delete";
            deleteMacroButton.UseVisualStyleBackColor = true;
            // 
            // renameMacroButton
            // 
            renameMacroButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            renameMacroButton.Location = new Point(267, 249);
            renameMacroButton.Margin = new Padding(3, 2, 3, 2);
            renameMacroButton.Name = "renameMacroButton";
            renameMacroButton.Size = new Size(82, 22);
            renameMacroButton.TabIndex = 4;
            renameMacroButton.Text = "Rename";
            renameMacroButton.UseVisualStyleBackColor = true;
            // 
            // runMacroButton
            // 
            runMacroButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            runMacroButton.Location = new Point(355, 249);
            runMacroButton.Margin = new Padding(3, 2, 3, 2);
            runMacroButton.Name = "runMacroButton";
            runMacroButton.Size = new Size(82, 22);
            runMacroButton.TabIndex = 5;
            runMacroButton.Text = "Run";
            runMacroButton.UseVisualStyleBackColor = true;
            // 
            // settingsMacroButton
            // 
            settingsMacroButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            settingsMacroButton.Location = new Point(440, 249);
            settingsMacroButton.Margin = new Padding(3, 2, 3, 2);
            settingsMacroButton.Name = "settingsMacroButton";
            settingsMacroButton.Size = new Size(82, 22);
            settingsMacroButton.TabIndex = 6;
            settingsMacroButton.Text = "Settings";
            settingsMacroButton.UseVisualStyleBackColor = true;
            // 
            // workflowsTab
            // 
            workflowsTab.Controls.Add(workflowsListView);
            workflowsTab.Controls.Add(newWorkflowButton);
            workflowsTab.Controls.Add(editWorkflowButton);
            workflowsTab.Controls.Add(deleteWorkflowButton);
            workflowsTab.Controls.Add(renameWorkflowButton);
            workflowsTab.Controls.Add(runWorkflowButton);
            workflowsTab.Controls.Add(scheduleWorkflowButton);
            workflowsTab.Location = new Point(4, 24);
            workflowsTab.Margin = new Padding(3, 2, 3, 2);
            workflowsTab.Name = "workflowsTab";
            workflowsTab.Padding = new Padding(3, 2, 3, 2);
            workflowsTab.Size = new Size(526, 281);
            workflowsTab.TabIndex = 3;
            workflowsTab.Text = "Workflows";
            workflowsTab.UseVisualStyleBackColor = true;
            // 
            // workflowsListView
            // 
            workflowsListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            workflowsListView.CheckBoxes = true;
            workflowsListView.Location = new Point(5, 4);
            workflowsListView.Margin = new Padding(3, 2, 3, 2);
            workflowsListView.Name = "workflowsListView";
            workflowsListView.Size = new Size(517, 241);
            workflowsListView.TabIndex = 0;
            workflowsListView.UseCompatibleStateImageBehavior = false;
            workflowsListView.View = View.Details;
            // 
            // newWorkflowButton
            // 
            newWorkflowButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            newWorkflowButton.Location = new Point(5, 249);
            newWorkflowButton.Margin = new Padding(3, 2, 3, 2);
            newWorkflowButton.Name = "newWorkflowButton";
            newWorkflowButton.Size = new Size(82, 22);
            newWorkflowButton.TabIndex = 1;
            newWorkflowButton.Text = "New";
            newWorkflowButton.UseVisualStyleBackColor = true;
            // 
            // editWorkflowButton
            // 
            editWorkflowButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            editWorkflowButton.Location = new Point(91, 249);
            editWorkflowButton.Margin = new Padding(3, 2, 3, 2);
            editWorkflowButton.Name = "editWorkflowButton";
            editWorkflowButton.Size = new Size(82, 22);
            editWorkflowButton.TabIndex = 2;
            editWorkflowButton.Text = "Edit";
            editWorkflowButton.UseVisualStyleBackColor = true;
            // 
            // deleteWorkflowButton
            // 
            deleteWorkflowButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            deleteWorkflowButton.Location = new Point(179, 249);
            deleteWorkflowButton.Margin = new Padding(3, 2, 3, 2);
            deleteWorkflowButton.Name = "deleteWorkflowButton";
            deleteWorkflowButton.Size = new Size(82, 22);
            deleteWorkflowButton.TabIndex = 3;
            deleteWorkflowButton.Text = "Delete";
            deleteWorkflowButton.UseVisualStyleBackColor = true;
            deleteWorkflowButton.Click += deleteWorkflowButton_Click_1;
            // 
            // renameWorkflowButton
            // 
            renameWorkflowButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            renameWorkflowButton.Location = new Point(268, 249);
            renameWorkflowButton.Margin = new Padding(3, 2, 3, 2);
            renameWorkflowButton.Name = "renameWorkflowButton";
            renameWorkflowButton.Size = new Size(82, 22);
            renameWorkflowButton.TabIndex = 4;
            renameWorkflowButton.Text = "Rename";
            renameWorkflowButton.UseVisualStyleBackColor = true;
            // 
            // runWorkflowButton
            // 
            runWorkflowButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            runWorkflowButton.Location = new Point(356, 249);
            runWorkflowButton.Margin = new Padding(3, 2, 3, 2);
            runWorkflowButton.Name = "runWorkflowButton";
            runWorkflowButton.Size = new Size(82, 22);
            runWorkflowButton.TabIndex = 5;
            runWorkflowButton.Text = "Run";
            runWorkflowButton.UseVisualStyleBackColor = true;
            runWorkflowButton.Click += runWorkflowButton_Click_1;
            // 
            // scheduleWorkflowButton
            // 
            scheduleWorkflowButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            scheduleWorkflowButton.Location = new Point(440, 249);
            scheduleWorkflowButton.Margin = new Padding(3, 2, 3, 2);
            scheduleWorkflowButton.Name = "scheduleWorkflowButton";
            scheduleWorkflowButton.Size = new Size(82, 22);
            scheduleWorkflowButton.TabIndex = 6;
            scheduleWorkflowButton.Text = "Schedule";
            scheduleWorkflowButton.UseVisualStyleBackColor = true;
            // 
            // userSettingsTab
            // 
            userSettingsTab.Controls.Add(darkModeCheckBox);
            userSettingsTab.Controls.Add(startWithWindowsCheckBox);
            userSettingsTab.Location = new Point(4, 24);
            userSettingsTab.Margin = new Padding(3, 2, 3, 2);
            userSettingsTab.Name = "userSettingsTab";
            userSettingsTab.Padding = new Padding(3, 2, 3, 2);
            userSettingsTab.Size = new Size(526, 281);
            userSettingsTab.TabIndex = 3;
            userSettingsTab.Text = "User Settings";
            userSettingsTab.UseVisualStyleBackColor = true;
            // 
            // darkModeCheckBox
            // 
            darkModeCheckBox.AutoSize = true;
            darkModeCheckBox.Location = new Point(18, 15);
            darkModeCheckBox.Margin = new Padding(3, 2, 3, 2);
            darkModeCheckBox.Name = "darkModeCheckBox";
            darkModeCheckBox.Size = new Size(84, 19);
            darkModeCheckBox.TabIndex = 0;
            darkModeCheckBox.Text = "Dark Mode";
            darkModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // startWithWindowsCheckBox
            // 
            startWithWindowsCheckBox.AutoSize = true;
            startWithWindowsCheckBox.Location = new Point(18, 38);
            startWithWindowsCheckBox.Margin = new Padding(3, 2, 3, 2);
            startWithWindowsCheckBox.Name = "startWithWindowsCheckBox";
            startWithWindowsCheckBox.Size = new Size(128, 19);
            startWithWindowsCheckBox.TabIndex = 1;
            startWithWindowsCheckBox.Text = "Start with Windows";
            startWithWindowsCheckBox.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 336);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new Padding(1, 0, 12, 0);
            statusStrip.Size = new Size(555, 22);
            statusStrip.TabIndex = 1;
            statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(39, 17);
            statusLabel.Text = "Ready";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(555, 358);
            Controls.Add(statusStrip);
            Controls.Add(mainTabControl);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Margin = new Padding(3, 2, 3, 2);
            Name = "MainForm";
            Text = "PyClicker - Pro";
            trayContextMenu.ResumeLayout(false);
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            mainTabControl.ResumeLayout(false);
            recordPlayTab.ResumeLayout(false);
            savedMacrosTab.ResumeLayout(false);
            workflowsTab.ResumeLayout(false);
            userSettingsTab.ResumeLayout(false);
            userSettingsTab.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.ContextMenuStrip trayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem showMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage recordPlayTab;
        private System.Windows.Forms.Button recordButton;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.ListBox actionsListBox;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadMenuItem;
        private System.Windows.Forms.TabPage savedMacrosTab;
        private System.Windows.Forms.ListView savedMacrosListView;
        private System.Windows.Forms.Button newMacroButton;
        private System.Windows.Forms.Button editMacroButton;
        private System.Windows.Forms.Button deleteMacroButton;
        private System.Windows.Forms.Button runMacroButton;
        private System.Windows.Forms.Button renameMacroButton;
        private System.Windows.Forms.Button saveMacroButton;
        private System.Windows.Forms.Button settingsMacroButton;
        private System.Windows.Forms.TabPage workflowsTab;
        private System.Windows.Forms.ListView workflowsListView;
        private System.Windows.Forms.Button newWorkflowButton;
        private System.Windows.Forms.Button editWorkflowButton;
        private System.Windows.Forms.Button deleteWorkflowButton;
        private System.Windows.Forms.Button renameWorkflowButton;
        private System.Windows.Forms.Button runWorkflowButton;
        private System.Windows.Forms.Button scheduleWorkflowButton;
        private System.Windows.Forms.TabPage userSettingsTab;
        private System.Windows.Forms.CheckBox darkModeCheckBox;
        private System.Windows.Forms.CheckBox startWithWindowsCheckBox;
    }
}
