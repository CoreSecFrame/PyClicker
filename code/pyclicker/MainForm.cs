using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using PyClickerRecorder.Workflow;

namespace PyClickerRecorder
{
    public partial class MainForm : Form
    {
        private readonly Player _player;
        private readonly Recorder _recorder;
        private List<RecordedAction> _recordedActions;
        private bool _isRecording;
        private readonly IKeyboardMouseEvents? _globalHook;
        private List<SavedMacro> _savedMacros;
        private static readonly string MacrosFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PyClickerRecorder", "macros.json");
        private const string AppName = "PyClicker - Pro";

        // Scheduler components
        private System.Windows.Forms.Timer _macroCheckTimer;
        private readonly Dictionary<string, CancellationTokenSource> _runningMacros = new Dictionary<string, CancellationTokenSource>();
        private bool _startupMacrosExecuted = false;

        // Workflow components
        private List<PyClickerRecorder.Workflow.Workflow> _savedWorkflows = new List<PyClickerRecorder.Workflow.Workflow>();
        private readonly WorkflowEngine _workflowEngine;
        private readonly WorkflowStorage _workflowStorage;

        // Public property to access workflow engine
        public WorkflowEngine WorkflowEngine => _workflowEngine;

        public MainForm()
        {
            InitializeComponent();

            // Set window and tray icon from embedded resource
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("PyClickerRecorder.icon.ico"))
                {
                    if (stream != null)
                    {
                        var icon = new Icon(stream);
                        this.Icon = icon;
                        trayIcon.Icon = icon;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if icon fails to load, but don't crash
                Console.WriteLine($"Failed to load icon from embedded resource: {ex.Message}");
            }

            _player = new Player();
            _recorder = new Recorder();
            _recordedActions = new List<RecordedAction>();
            _savedMacros = new List<SavedMacro>();
            LoadMacros();
            UpdateSavedMacrosListView();
            _isRecording = false;

            // Initialize workflow components
            _workflowStorage = new WorkflowStorage();
            _workflowEngine = new WorkflowEngine(_player);
            Task.Run(async () =>
            {
                await LoadWorkflowsAsync();
                this.Invoke(() => UpdateWorkflowsListView());
            });
            _workflowEngine.WorkflowStatusChanged += WorkflowEngine_WorkflowStatusChanged;

            // Wire up event handlers
            this.playButton.Click += PlayButton_Click;
            this.recordButton.Click += RecordButton_Click;
            this.saveMenuItem.Click += SaveAsMenuItem_Click;
            this.loadMenuItem.Click += LoadMenuItem_Click;
            // Settings controls removed - now handled by MacroSettingsForm

            // Tray Icon Handlers
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.trayIcon.DoubleClick += new System.EventHandler(this.trayIcon_DoubleClick);
            this.showMenuItem.Click += new System.EventHandler(this.showMenuItem_Click);
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);


            // Global Hotkeys
            _globalHook = Gma.System.MouseKeyHook.Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHook_KeyDown;

            // Scheduler Setup
            _macroCheckTimer = new System.Windows.Forms.Timer();
            _macroCheckTimer.Interval = 1000; // Check every second
            _macroCheckTimer.Tick += new System.EventHandler(this._macroCheckTimer_Tick);
            _macroCheckTimer.Start();

            // Saved Macros Tab Handlers
            this.saveMacroButton.Click += new System.EventHandler(this.SaveMacroButton_Click);
            this.newMacroButton.Click += new System.EventHandler(this.NewMacroButton_Click);
            this.editMacroButton.Click += new System.EventHandler(this.EditMacroButton_Click);
            this.deleteMacroButton.Click += new System.EventHandler(this.DeleteMacroButton_Click);
            this.renameMacroButton.Click += new System.EventHandler(this.RenameMacroButton_Click);
            this.runMacroButton.Click += new System.EventHandler(this.RunMacroButton_Click);
            this.settingsMacroButton.Click += new System.EventHandler(this.SettingsMacroButton_Click);
            this.savedMacrosListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.SavedMacrosListView_ItemCheck);

            // Workflow Tab Handlers
            this.newWorkflowButton.Click += new System.EventHandler(this.NewWorkflowButton_Click);
            this.editWorkflowButton.Click += new System.EventHandler(this.EditWorkflowButton_Click);
            this.deleteWorkflowButton.Click += new System.EventHandler(this.DeleteWorkflowButton_Click);
            this.renameWorkflowButton.Click += new System.EventHandler(this.RenameWorkflowButton_Click);
            this.runWorkflowButton.Click += new System.EventHandler(this.RunWorkflowButton_Click);
            this.scheduleWorkflowButton.Click += new System.EventHandler(this.ScheduleWorkflowButton_Click);
            this.workflowsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.WorkflowsListView_ItemCheck);

            // User Settings Tab Handlers
            this.darkModeCheckBox.CheckedChanged += new System.EventHandler(this.DarkModeCheckBox_CheckedChanged);
            this.startWithWindowsCheckBox.CheckedChanged += new System.EventHandler(this.StartWithWindowsCheckBox_CheckedChanged);


            // Load initial state for Start with Windows checkbox
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
                startWithWindowsCheckBox.Checked = (rk.GetValue(AppName) != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read registry for startup setting: {ex.Message}");
            }
        }

        private void GlobalHook_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F6)
            {
                RecordButton_Click(this, EventArgs.Empty);
                e.Handled = true; // Prevents the key press from being passed to other applications
            }
            else if (e.KeyCode == Keys.F7)
            {
                PlayButton_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void RecordButton_Click(object? sender, EventArgs e)
        {
            if (_isRecording)
            {
                // Stop recording
                _recorder.Stop();
                _isRecording = false;
                recordButton.Text = "Start Recording (F6)";
                statusLabel.Text = "Recording Stopped.";
                trayIcon.ShowBalloonTip(1000, "PyClicker - Pro", "Recording stopped.", ToolTipIcon.Info);

                // Get actions and update listbox
                _recordedActions = _recorder.Actions;
                UpdateActionsListBox();
            }
            else
            {
                // Start recording
                if (_runningMacros.Any()) return; // Don't record while playing

                _isRecording = true;
                recordButton.Text = "Stop Recording (F6)";
                statusLabel.Text = "Recording...";
                trayIcon.ShowBalloonTip(1000, "PyClicker - Pro", "Recording started.", ToolTipIcon.Info);
                _recorder.Start();
            }
        }

        private void UpdateActionsListBox()
        {
            actionsListBox.Items.Clear();
            if (_recordedActions == null) return;

            foreach (var action in _recordedActions)
            {
                string description = action switch
                {
                    MouseMoveAction move => $"Mouse Move to {move.Position}",
                    MouseClickAction click => $"Mouse {click.Button} {click.State} at {click.Position}",
                    MouseScrollAction scroll => $"Mouse Scroll {scroll.Amount} at {scroll.Position}",
                    KeyAction key => FormatKeyAction(key),
                    DelayAction delay => $"Delay for {delay.Duration:F2}s",
                    _ => action.GetType().Name
                };
                actionsListBox.Items.Add(description);
            }
        }

        private string FormatKeyAction(KeyAction keyAction)
        {
            var keyName = ((Keys)keyAction.Key).ToString();
            var modifiers = string.Join(" + ", keyAction.Modifiers);
            var fullKey = string.IsNullOrEmpty(modifiers) ? keyName : $"{modifiers} + {keyName}";
            return $"Key {keyAction.State}: {fullKey}";
        }

        // Settings controls removed - now handled by MacroSettingsForm

        private async void PlayButton_Click(object? sender, EventArgs e)
        {
            const string currentMacroName = "__CURRENT__";
            var tempMacro = new SavedMacro { Name = currentMacroName };

            if (_runningMacros.ContainsKey(currentMacroName))
            {
                StopMacro(tempMacro);
                playButton.Text = "Start Playback (F7)";
                statusLabel.Text = "Ready";
                return;
            }

            if (_recordedActions == null || _recordedActions.Count == 0)
            {
                MessageBox.Show("No actions to play.", "Playback", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Create a temporary macro with default settings for playback
            var currentMacro = new SavedMacro
            {
                Name = currentMacroName,
                Actions = new List<RecordedAction>(_recordedActions),
                LoopCount = 1,
                InfiniteLoop = false,
                SpeedFactor = 1.0
            };

            playButton.Text = "Stop Playback (F7)";
            statusLabel.Text = "Playing...";

            await PlayMacro(currentMacro, currentMacro.InfiniteLoop);

            playButton.Text = "Start Playback (F7)";
            statusLabel.Text = "Ready";
        }

        private void UpdateMacroPropertiesFromForm(SavedMacro macro)
        {
            macro.Actions = new List<RecordedAction>(_recordedActions);

            // Set defaults for new macros - settings configured via MacroSettingsForm
            if (macro.LoopCount == 0) // Only set defaults for truly new macros
            {
                macro.LoopCount = 1;
                macro.InfiniteLoop = false;
                macro.SpeedFactor = 1.0;
                macro.ScheduleMode = (MacroScheduleMode)0; // Default/None
                macro.ScheduledTime = new DateTime(2000, 1, 1);
                macro.ScheduledEndTime = new DateTime(2000, 1, 1);
                macro.RepeatDaily = false;
            }
        }

        private void SaveAsMenuItem_Click(object? sender, EventArgs e)
        {
            string macroName = "";
            if (InputBox.Show("Save Macro As", "Enter a name for the macro:", ref macroName) == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(macroName))
                {
                    MessageBox.Show("Macro name cannot be empty.", "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var existingMacro = _savedMacros.FirstOrDefault(m => m.Name == macroName);
                if (existingMacro != null)
                {
                    if (MessageBox.Show($"A macro named '{macroName}' already exists. Do you want to overwrite it?", "Confirm Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        return;
                    }
                    UpdateMacroPropertiesFromForm(existingMacro); // Use helper function
                }
                else
                {
                    var newMacro = new SavedMacro
                    {
                        Name = macroName,
                        IsEnabled = true
                    };
                    UpdateMacroPropertiesFromForm(newMacro); // Use helper function
                    _savedMacros.Add(newMacro);
                }

                SaveMacros();
                UpdateSavedMacrosListView();
                statusLabel.Text = $"Macro '{macroName}' saved.";
            }
        }

        private void LoadMenuItem_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                openFileDialog.Title = "Load Macro";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string jsonString = File.ReadAllText(openFileDialog.FileName);
                        var options = new JsonSerializerOptions();
                        _recordedActions = JsonSerializer.Deserialize<List<RecordedAction>>(jsonString, options);
                        UpdateActionsListBox();
                        statusLabel.Text = $"Loaded: {Path.GetFileName(openFileDialog.FileName)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load file: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
                trayIcon.Visible = true;
                trayIcon.ShowBalloonTip(1000, "PyClicker - Pro", "Application minimized to tray.", ToolTipIcon.Info);
            }
        }

        private void trayIcon_DoubleClick(object? sender, EventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            trayIcon.Visible = false;
        }

        private void showMenuItem_Click(object? sender, EventArgs e)
        {
            trayIcon_DoubleClick(sender, e);
        }

        private void exitMenuItem_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }


        private async Task PlayMacro(SavedMacro macro, bool isLooping = false)
        {
            if (_runningMacros.ContainsKey(macro.Name)) return;

            var cts = new CancellationTokenSource();
            _runningMacros[macro.Name] = cts;

            try
            {
                int loopCount = isLooping ? 0 : macro.LoopCount;
                await _player.Play(macro.Actions, loopCount, macro.SpeedFactor, cts.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during playback of macro '{macro.Name}': {ex.Message}", "Playback Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _runningMacros.Remove(macro.Name);
            }
        }

        private void StopMacro(SavedMacro macro)
        {
            if (_runningMacros.TryGetValue(macro.Name, out var cts))
            {
                cts.Cancel();
                _runningMacros.Remove(macro.Name);
            }
        }

        private async void _macroCheckTimer_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            // Execute startup macros only once when the application starts
            if (!_startupMacrosExecuted)
            {
                var startupMacros = _savedMacros.Where(m => m.IsEnabled && m.ScheduleMode == MacroScheduleMode.OnWindowsStartup).ToList();
                foreach (var macro in startupMacros)
                {
                    if (!_runningMacros.ContainsKey(macro.Name))
                    {
                        await PlayMacro(macro);
                    }
                }
                _startupMacrosExecuted = true;
            }

            foreach (var macro in _savedMacros)
            {
                if (!macro.IsEnabled)
                {
                    continue;
                }

                // Skip macros without schedule (default value 0) or with invalid dates
                if (macro.ScheduleMode == (MacroScheduleMode)0 || macro.ScheduledTime.Year < 1900)
                {
                    continue;
                }

                bool isRunning = _runningMacros.ContainsKey(macro.Name);

                if (macro.ScheduleMode == MacroScheduleMode.SpecificTime)
                {
                    bool shouldRun = false;
                    if (macro.RepeatDaily)
                    {
                        var scheduledTimeOfDay = macro.ScheduledTime.TimeOfDay;
                        if (now.TimeOfDay >= scheduledTimeOfDay && now.TimeOfDay < scheduledTimeOfDay.Add(TimeSpan.FromSeconds(1)) && !isRunning)
                        {
                            shouldRun = true;
                        }
                    }
                    else
                    {
                        if (now >= macro.ScheduledTime && !isRunning)
                        {
                            shouldRun = true;
                        }
                    }

                    if (shouldRun)
                    {
                        if (!macro.RepeatDaily)
                        {
                            macro.IsEnabled = false;
                            UpdateSavedMacrosListView();
                            SaveMacros();
                        }

                        await PlayMacro(macro);
                    }
                }
                else if (macro.ScheduleMode == MacroScheduleMode.TimeRange)
                {
                    bool inRange;
                    if (macro.RepeatDaily)
                    {
                        inRange = (now.TimeOfDay >= macro.ScheduledTime.TimeOfDay && now.TimeOfDay < macro.ScheduledEndTime.TimeOfDay);
                    }
                    else
                    {
                        inRange = (now >= macro.ScheduledTime && now < macro.ScheduledEndTime);
                    }

                    if (inRange && !isRunning)
                    {
                        await PlayMacro(macro, isLooping: true);
                    }
                    else if (!inRange && isRunning)
                    {
                        StopMacro(macro);
                    }
                }
                // OnWindowsStartup macros are handled separately above
            }

            // Check workflows for scheduled execution
            foreach (var workflow in _savedWorkflows)
            {
                if (!workflow.IsEnabled || _workflowEngine.IsWorkflowRunning(workflow.Id))
                {
                    continue;
                }

                bool shouldRun = false;

                if (workflow.ScheduleMode == MacroScheduleMode.SpecificTime)
                {
                    if (workflow.RepeatDaily)
                    {
                        var scheduledTimeOfDay = workflow.ScheduledTime.TimeOfDay;
                        if (now.TimeOfDay >= scheduledTimeOfDay && now.TimeOfDay < scheduledTimeOfDay.Add(TimeSpan.FromSeconds(1)))
                        {
                            shouldRun = true;
                        }
                    }
                    else
                    {
                        if (now >= workflow.ScheduledTime && now < workflow.ScheduledTime.AddSeconds(1))
                        {
                            shouldRun = true;
                        }
                    }
                }
                else if (workflow.ScheduleMode == MacroScheduleMode.TimeRange)
                {
                    bool inRange;
                    if (workflow.RepeatDaily)
                    {
                        inRange = (now.TimeOfDay >= workflow.ScheduledTime.TimeOfDay && now.TimeOfDay < workflow.ScheduledEndTime.TimeOfDay);
                    }
                    else
                    {
                        inRange = (now >= workflow.ScheduledTime && now < workflow.ScheduledEndTime);
                    }

                    if (inRange)
                    {
                        shouldRun = true;
                    }
                }
                else if (workflow.ScheduleMode == MacroScheduleMode.OnWindowsStartup && !_startupMacrosExecuted)
                {
                    shouldRun = true;
                }

                if (shouldRun)
                {
                    try
                    {
                        await _workflowEngine.ExecuteWorkflowAsync(workflow);

                        // Disable one-time scheduled workflows
                        if (!workflow.RepeatDaily && workflow.ScheduleMode != MacroScheduleMode.OnWindowsStartup)
                        {
                            workflow.IsEnabled = false;
                            UpdateWorkflowsListView();
                            _ = SaveWorkflowsAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to execute scheduled workflow '{workflow.Name}': {ex.Message}");
                    }
                }
            }
        }

        private void SaveMacros()
        {
            try
            {
                string? directory = Path.GetDirectoryName(MacrosFilePath);
                if (directory != null)
                {
                    Directory.CreateDirectory(directory);
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(_savedMacros, options);
                    File.WriteAllText(MacrosFilePath, jsonString);
                }
                else
                {
                    throw new InvalidOperationException("Could not determine the directory for the macros file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save macros: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMacros()
        {
            try
            {
                if (File.Exists(MacrosFilePath))
                {
                    string jsonString = File.ReadAllText(MacrosFilePath);
                    var options = new JsonSerializerOptions();
                    var loadedMacros = JsonSerializer.Deserialize<List<SavedMacro>>(jsonString, options);
                    _savedMacros = loadedMacros ?? new List<SavedMacro>();

                }
                else
                {
                    _savedMacros = new List<SavedMacro>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load macros: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _savedMacros = new List<SavedMacro>();
            }
        }

        private void UpdateSavedMacrosListView()
        {
            savedMacrosListView.Items.Clear();

            if (savedMacrosListView.Columns.Count == 0)
            {
                savedMacrosListView.Columns.Add("Name", 200);
                savedMacrosListView.Columns.Add("Status", 100);
                savedMacrosListView.Columns.Add("Schedule", 250);
            }

            if (_savedMacros == null) return;

            if (_savedMacros.Count == 0)
            {
                // Show helpful message when no macros exist
                var emptyItem = new ListViewItem("No saved macros");
                emptyItem.SubItems.Add("Record a macro to get started");
                emptyItem.SubItems.Add("");
                emptyItem.ForeColor = System.Drawing.Color.Gray;
                emptyItem.Tag = null; // No macro object associated
                // Disable checkbox for empty message
                emptyItem.UseItemStyleForSubItems = true;
                savedMacrosListView.Items.Add(emptyItem);
                return;
            }

            foreach (var macro in _savedMacros)
            {
                var item = new ListViewItem(macro.Name);
                item.SubItems.Add(macro.IsEnabled ? "Enabled" : "Disabled");

                string scheduleSummary = "Not scheduled";
                if (macro.ScheduleMode == MacroScheduleMode.OnWindowsStartup)
                {
                    scheduleSummary = "Run on Windows startup";
                }
                else if (macro.RepeatDaily)
                {
                    if (macro.ScheduleMode == MacroScheduleMode.SpecificTime)
                    {
                        scheduleSummary = $"Daily at {macro.ScheduledTime:T}";
                    }
                    else if (macro.ScheduleMode == MacroScheduleMode.TimeRange)
                    {
                        scheduleSummary = $"Daily from {macro.ScheduledTime:T} to {macro.ScheduledEndTime:T}";
                    }
                }
                else
                {
                    if (macro.ScheduleMode == MacroScheduleMode.SpecificTime)
                    {
                        scheduleSummary = $"At {macro.ScheduledTime:g}";
                    }
                    else if (macro.ScheduleMode == MacroScheduleMode.TimeRange)
                    {
                        scheduleSummary = $"From {macro.ScheduledTime:g} to {macro.ScheduledEndTime:g}";
                    }
                }
                item.SubItems.Add(scheduleSummary);
                item.Tag = macro;
                item.Checked = macro.IsEnabled;
                savedMacrosListView.Items.Add(item);
            }
        }

        private void SaveMacroButton_Click(object? sender, EventArgs e)
        {
            if (savedMacrosListView.SelectedItems.Count > 0)
            {
                var selectedItem = savedMacrosListView.SelectedItems[0];
                var macroToSave = (SavedMacro)selectedItem.Tag;

                if (MessageBox.Show($"Are you sure you want to overwrite the macro '{macroToSave.Name}'?", "Confirm Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    UpdateMacroPropertiesFromForm(macroToSave); // Use helper function
                    SaveMacros();
                    UpdateSavedMacrosListView();
                    statusLabel.Text = $"Macro '{macroToSave.Name}' updated.";
                }
            }
            else
            {
                // If no macro is selected, treat it as a "Save As..." action
                SaveAsMenuItem_Click(sender, e);
            }
        }

        private void NewMacroButton_Click(object? sender, EventArgs e)
        {
            var newMacro = new SavedMacro();
            int counter = 1;
            string baseName = "New Macro";
            newMacro.Name = $"{baseName} {counter}";
            while (_savedMacros.Any(m => m.Name == newMacro.Name))
            {
                counter++;
                newMacro.Name = $"{baseName} {counter}";
            }

            newMacro.Actions = new List<RecordedAction>();

            _savedMacros.Add(newMacro);
            SaveMacros();
            UpdateSavedMacrosListView();
        }

        private void EditMacroButton_Click(object? sender, EventArgs e)
        {
            if (savedMacrosListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a macro to edit.", "No Macro Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = savedMacrosListView.SelectedItems[0];
            var macroToEdit = (SavedMacro)selectedItem.Tag;

            _recordedActions = new List<RecordedAction>(macroToEdit.Actions);
            UpdateActionsListBox();

            // All settings are now handled by MacroSettingsForm - no UI updates needed here

            mainTabControl.SelectedTab = recordPlayTab;

            MessageBox.Show($"The macro '{macroToEdit.Name}' is now loaded for editing. Use the 'Save Macro' button to save your changes to this macro.", "Macro Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteMacroButton_Click(object? sender, EventArgs e)
        {
            if (savedMacrosListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a macro to delete.", "No Macro Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = savedMacrosListView.SelectedItems[0];
            var macroToDelete = (SavedMacro)selectedItem.Tag;

            if (MessageBox.Show($"Are you sure you want to delete the macro '{macroToDelete.Name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _savedMacros.Remove(macroToDelete);
                SaveMacros();
                UpdateSavedMacrosListView();
            }
        }

        private async void RunMacroButton_Click(object? sender, EventArgs e)
        {
            if (savedMacrosListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a macro to run.", "No Macro Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = savedMacrosListView.SelectedItems[0];
            var macroToRun = (SavedMacro)selectedItem.Tag;

            if (_runningMacros.ContainsKey(macroToRun.Name))
            {
                StopMacro(macroToRun);
            }
            else
            {
                await PlayMacro(macroToRun);
            }
        }

        private void SettingsMacroButton_Click(object? sender, EventArgs e)
        {
            if (savedMacrosListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a macro to configure.", "No Macro Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = savedMacrosListView.SelectedItems[0];
            var macroToEdit = (SavedMacro)selectedItem.Tag;

            using (var settingsForm = new MacroSettingsForm(macroToEdit))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Save the updated macro settings
                    SaveMacros();
                    UpdateSavedMacrosListView();
                    statusLabel.Text = $"Settings updated for macro '{macroToEdit.Name}'";
                }
            }
        }

        private void SavedMacrosListView_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue == e.NewValue) return;

            var item = savedMacrosListView.Items[e.Index];

            // Prevent checking/unchecking of empty message item
            if (item.Tag == null)
            {
                e.NewValue = e.CurrentValue; // Prevent the change
                return;
            }

            var macro = (SavedMacro)item.Tag;
            macro.IsEnabled = e.NewValue == CheckState.Checked;
            item.SubItems[1].Text = macro.IsEnabled ? "Enabled" : "Disabled";
            SaveMacros();
        }

        private void RenameMacroButton_Click(object? sender, EventArgs e)
        {
            if (savedMacrosListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a macro to rename.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = savedMacrosListView.SelectedItems[0];

            // Prevent renaming the empty message item
            if (selectedItem.Tag == null)
            {
                return;
            }

            var macro = (SavedMacro)selectedItem.Tag;

            // Simple input dialog for new name
            string currentName = macro.Name;
            string newName = ShowInputDialog("Rename Macro", "Enter new name:", currentName);

            if (!string.IsNullOrWhiteSpace(newName) && newName != currentName)
            {
                // Check if name already exists
                if (_savedMacros.Any(m => m.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && m != macro))
                {
                    MessageBox.Show("A macro with this name already exists.", "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                macro.Name = newName;
                SaveMacros();
                UpdateSavedMacrosListView();
            }
        }

        private void RenameWorkflowButton_Click(object? sender, EventArgs e)
        {
            if (workflowsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a workflow to rename.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = workflowsListView.SelectedItems[0];
            var workflow = (PyClickerRecorder.Workflow.Workflow)selectedItem.Tag;

            // Simple input dialog for new name
            string currentName = workflow.Name;
            string newName = ShowInputDialog("Rename Workflow", "Enter new name:", currentName);

            if (!string.IsNullOrWhiteSpace(newName) && newName != currentName)
            {
                // Check if name already exists
                if (_savedWorkflows.Any(w => w.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && w != workflow))
                {
                    MessageBox.Show("A workflow with this name already exists.", "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                workflow.Name = newName;
                _ = SaveWorkflowsAsync();
                UpdateWorkflowsListView();
            }
        }

        private string ShowInputDialog(string title, string prompt, string defaultValue = "")
        {
            using (var inputForm = new Form())
            {
                inputForm.Text = title;
                inputForm.Size = new Size(400, 150);
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                var label = new Label
                {
                    Text = prompt,
                    Location = new Point(12, 15),
                    Size = new Size(360, 20)
                };

                var textBox = new TextBox
                {
                    Text = defaultValue,
                    Location = new Point(12, 40),
                    Size = new Size(360, 23)
                };
                textBox.SelectAll();

                var okButton = new Button
                {
                    Text = "OK",
                    Location = new Point(217, 75),
                    Size = new Size(75, 23),
                    DialogResult = DialogResult.OK
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    Location = new Point(297, 75),
                    Size = new Size(75, 23),
                    DialogResult = DialogResult.Cancel
                };

                inputForm.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;

                return inputForm.ShowDialog(this) == DialogResult.OK ? textBox.Text : defaultValue;
            }
        }

        private void DarkModeCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            SetDarkMode(darkModeCheckBox.Checked);
        }

        private void SetDarkMode(bool isDark)
        {
            var backColor = isDark ? Color.FromArgb(28, 28, 28) : SystemColors.Control;
            var foreColor = isDark ? Color.White : SystemColors.ControlText;
            var darkerBackColor = isDark ? Color.FromArgb(45, 45, 48) : SystemColors.ControlLight;
            var borderColor = isDark ? Color.FromArgb(60, 60, 60) : SystemColors.ControlDark;

            this.BackColor = backColor;
            this.ForeColor = foreColor;

            // Special handling for MenuStrip and StatusStrip
            var toolStripRenderer = isDark ? new DarkModeToolStripRenderer() : null;
            menuStrip.Renderer = toolStripRenderer;
            statusStrip.Renderer = toolStripRenderer;
            statusStrip.BackColor = backColor;
            statusStrip.ForeColor = foreColor;


            foreach (Control c in this.Controls)
            {
                UpdateColorControls(c, backColor, foreColor, darkerBackColor, borderColor);
            }
        }

        private void UpdateColorControls(Control myControl, Color backColor, Color foreColor, Color darkerBackColor, Color borderColor)
        {
            myControl.BackColor = backColor;
            myControl.ForeColor = foreColor;

            if (myControl is Button button)
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = borderColor;
            }
            else if (myControl is TextBox || myControl is ListBox || myControl is NumericUpDown)
            {
                myControl.BackColor = darkerBackColor;
            }
            else if (myControl is ListView listView)
            {
                listView.BackColor = darkerBackColor;
                listView.ForeColor = foreColor;
            }
            else if (myControl is GroupBox groupBox)
            {
                groupBox.ForeColor = foreColor;
            }
            else if (myControl is TabControl tabControl)
            {
                foreach (TabPage tabPage in tabControl.TabPages)
                {
                    tabPage.BackColor = backColor;
                    tabPage.ForeColor = foreColor;
                }
            }


            foreach (Control subC in myControl.Controls)
            {
                UpdateColorControls(subC, backColor, foreColor, darkerBackColor, borderColor);
            }
        }

        public class DarkModeToolStripRenderer : ToolStripProfessionalRenderer
        {
            public DarkModeToolStripRenderer() : base(new DarkModeColorTable()) { }
        }

        public class DarkModeColorTable : ProfessionalColorTable
        {
            private readonly Color _backColor = Color.FromArgb(45, 45, 48);
            private readonly Color _foreColor = Color.White;
            private readonly Color _borderColor = Color.FromArgb(60, 60, 60);

            public override Color MenuStripGradientBegin => _backColor;
            public override Color MenuStripGradientEnd => _backColor;
            public override Color MenuItemSelected => _borderColor;
            public override Color MenuItemBorder => _borderColor;
            public override Color MenuItemPressedGradientBegin => _backColor;
            public override Color MenuItemPressedGradientEnd => _backColor;
            public override Color ToolStripDropDownBackground => _backColor;
            public override Color ImageMarginGradientBegin => _backColor;
            public override Color ImageMarginGradientMiddle => _backColor;
            public override Color ImageMarginGradientEnd => _backColor;
            public override Color SeparatorDark => _borderColor;
            public override Color SeparatorLight => _borderColor;
            public override Color StatusStripGradientBegin => _backColor;
            public override Color StatusStripGradientEnd => _backColor;
        }

        private void StartWithWindowsCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (startWithWindowsCheckBox.Checked)
                {
                    rk.SetValue(AppName, Application.ExecutablePath);
                }
                else
                {
                    rk.DeleteValue(AppName, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update registry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Workflow Management Methods
        private async Task LoadWorkflowsAsync()
        {
            try
            {
                _savedWorkflows = await _workflowStorage.LoadWorkflowsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load workflows: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _savedWorkflows = new List<PyClickerRecorder.Workflow.Workflow>();
            }
        }

        private async Task SaveWorkflowsAsync()
        {
            try
            {
                await _workflowStorage.SaveWorkflowsAsync(_savedWorkflows);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save workflows: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateWorkflowsListView()
        {
            workflowsListView.Items.Clear();

            if (workflowsListView.Columns.Count == 0)
            {
                workflowsListView.Columns.Add("Name", 200);
                workflowsListView.Columns.Add("Status", 100);
                workflowsListView.Columns.Add("Schedule", 150);
                workflowsListView.Columns.Add("Description", 200);
            }

            if (_savedWorkflows == null) return;

            foreach (var workflow in _savedWorkflows)
            {
                var item = new ListViewItem(workflow.Name ?? "");
                item.SubItems.Add(workflow.IsEnabled ? "Enabled" : "Disabled");

                string scheduleSummary = "Not scheduled";
                if (workflow.ScheduleMode == MacroScheduleMode.OnWindowsStartup)
                {
                    scheduleSummary = "Run on Windows startup";
                }
                else if (workflow.RepeatDaily)
                {
                    if (workflow.ScheduleMode == MacroScheduleMode.SpecificTime)
                    {
                        scheduleSummary = $"Daily at {workflow.ScheduledTime:T}";
                    }
                    else if (workflow.ScheduleMode == MacroScheduleMode.TimeRange)
                    {
                        scheduleSummary = $"Daily from {workflow.ScheduledTime:T} to {workflow.ScheduledEndTime:T}";
                    }
                }
                else
                {
                    if (workflow.ScheduleMode == MacroScheduleMode.SpecificTime)
                    {
                        scheduleSummary = $"At {workflow.ScheduledTime:g}";
                    }
                    else if (workflow.ScheduleMode == MacroScheduleMode.TimeRange)
                    {
                        scheduleSummary = $"From {workflow.ScheduledTime:g} to {workflow.ScheduledEndTime:g}";
                    }
                }

                item.SubItems.Add(scheduleSummary);
                item.SubItems.Add(workflow.Description ?? "");
                item.Tag = workflow;
                item.Checked = workflow.IsEnabled;
                workflowsListView.Items.Add(item);
            }
        }

        private void NewWorkflowButton_Click(object? sender, EventArgs e)
        {
            var newWorkflow = new PyClickerRecorder.Workflow.Workflow
            {
                Name = GetUniqueWorkflowName("New Workflow")
            };

            using (var designer = new PowerAutomateWorkflowDesigner(newWorkflow, _savedMacros))
            {
                if (designer.ShowDialog() == DialogResult.OK)
                {
                    _savedWorkflows.Add(designer.CurrentWorkflow);
                    _ = SaveWorkflowsAsync();
                    UpdateWorkflowsListView();
                }
            }
        }

        private void EditWorkflowButton_Click(object? sender, EventArgs e)
        {
            if (workflowsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a workflow to edit.", "No Workflow Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = workflowsListView.SelectedItems[0];
            var workflowToEdit = (PyClickerRecorder.Workflow.Workflow)selectedItem.Tag!;

            using (var designer = new PowerAutomateWorkflowDesigner(workflowToEdit, _savedMacros))
            {
                if (designer.ShowDialog() == DialogResult.OK)
                {
                    _ = SaveWorkflowsAsync();
                    UpdateWorkflowsListView();
                }
            }
        }

        private async void DeleteWorkflowButton_Click(object? sender, EventArgs e)
        {
            if (workflowsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a workflow to delete.", "No Workflow Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = workflowsListView.SelectedItems[0];
            var workflowToDelete = (PyClickerRecorder.Workflow.Workflow)selectedItem.Tag!;

            if (MessageBox.Show($"Are you sure you want to delete the workflow '{workflowToDelete.Name ?? "Unknown"}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _savedWorkflows.Remove(workflowToDelete);
                await _workflowStorage.DeleteWorkflowAsync(workflowToDelete.Id);
                UpdateWorkflowsListView();
            }
        }

        private async void RunWorkflowButton_Click(object? sender, EventArgs e)
        {
            if (workflowsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a workflow to run.", "No Workflow Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = workflowsListView.SelectedItems[0];
            var workflowToRun = (PyClickerRecorder.Workflow.Workflow)selectedItem.Tag!;

            if (_workflowEngine.IsWorkflowRunning(workflowToRun.Id))
            {
                _workflowEngine.StopWorkflow(workflowToRun.Id);
                runWorkflowButton.Text = "Run";
                statusLabel.Text = "Workflow stopped.";
            }
            else
            {
                if (workflowToRun.Blocks.Count == 0)
                {
                    MessageBox.Show("This workflow is empty. Please edit it and add some blocks first.", "Empty Workflow", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                runWorkflowButton.Text = "Stop";
                statusLabel.Text = $"Running workflow: {workflowToRun.Name ?? "Unknown"}";

                try
                {
                    await _workflowEngine.ExecuteWorkflowAsync(workflowToRun);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to run workflow: {ex.Message}", "Workflow Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void ScheduleWorkflowButton_Click(object? sender, EventArgs e)
        {
            if (workflowsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a workflow to schedule.", "No Workflow Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = workflowsListView.SelectedItems[0];
            var workflowToSchedule = (PyClickerRecorder.Workflow.Workflow)selectedItem.Tag!;

            using (var scheduleForm = new WorkflowScheduleForm(workflowToSchedule))
            {
                if (scheduleForm.ShowDialog() == DialogResult.OK)
                {
                    // Save the updated workflow with new schedule settings
                    await _workflowStorage.SaveWorkflowAsync(workflowToSchedule);
                    UpdateWorkflowsListView();

                    // Update status to show schedule info
                    string statusText = GetWorkflowScheduleStatus(workflowToSchedule);
                    if (!string.IsNullOrEmpty(statusText))
                    {
                        statusLabel.Text = statusText;
                    }
                }
            }
        }

        private string GetWorkflowScheduleStatus(PyClickerRecorder.Workflow.Workflow workflow)
        {
            if (workflow.ScheduleMode == MacroScheduleMode.OnWindowsStartup)
            {
                return $"Workflow '{workflow.Name}' scheduled on startup";
            }
            else if (workflow.ScheduleMode == MacroScheduleMode.SpecificTime)
            {
                if (workflow.RepeatDaily)
                {
                    return $"Workflow '{workflow.Name}' scheduled daily at {workflow.ScheduledTime:T}";
                }
                else
                {
                    return $"Workflow '{workflow.Name}' scheduled for {workflow.ScheduledTime:g}";
                }
            }
            else if (workflow.ScheduleMode == MacroScheduleMode.TimeRange)
            {
                if (workflow.RepeatDaily)
                {
                    return $"Workflow '{workflow.Name}' scheduled daily from {workflow.ScheduledTime:T} to {workflow.ScheduledEndTime:T}";
                }
                else
                {
                    return $"Workflow '{workflow.Name}' scheduled for {workflow.ScheduledTime:d} from {workflow.ScheduledTime:T} to {workflow.ScheduledEndTime:T}";
                }
            }
            return "";
        }

        private void WorkflowsListView_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue == e.NewValue) return;

            var item = workflowsListView.Items[e.Index];
            var workflow = (PyClickerRecorder.Workflow.Workflow)item.Tag!;
            workflow.IsEnabled = e.NewValue == CheckState.Checked;
            item.SubItems[1].Text = workflow.IsEnabled ? "Enabled" : "Disabled";
            _ = SaveWorkflowsAsync();
        }

        private void WorkflowEngine_WorkflowStatusChanged(object? sender, WorkflowStatusEventArgs e)
        {
            this.Invoke(() =>
            {
                var workflow = _savedWorkflows.FirstOrDefault(w => w.Id == e.WorkflowId);
                if (workflow != null)
                {
                    switch (e.Status)
                    {
                        case WorkflowStatus.Started:
                            statusLabel.Text = $"Workflow '{workflow.Name ?? "Unknown"}' started.";
                            break;
                        case WorkflowStatus.Completed:
                            statusLabel.Text = $"Workflow '{workflow.Name ?? "Unknown"}' completed.";
                            runWorkflowButton.Text = "Run";
                            break;
                        case WorkflowStatus.Failed:
                            statusLabel.Text = $"Workflow '{workflow.Name ?? "Unknown"}' failed: {e.Exception?.Message ?? "Unknown error"}";
                            runWorkflowButton.Text = "Run";
                            break;
                        case WorkflowStatus.Cancelled:
                            statusLabel.Text = $"Workflow '{workflow.Name ?? "Unknown"}' cancelled.";
                            runWorkflowButton.Text = "Run";
                            break;
                    }
                }
            });
        }

        private string GetUniqueWorkflowName(string baseName)
        {
            int counter = 1;
            string name = baseName;
            while (_savedWorkflows.Any(w => w.Name == name))
            {
                counter++;
                name = $"{baseName} {counter}";
            }
            return name;
        }

        private static class InputBox
        {
            public static DialogResult Show(string title, string promptText, ref string value)
            {
                Form form = new Form();
                Label label = new Label();
                TextBox textBox = new TextBox();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = promptText;
                textBox.Text = value;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                textBox.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                form.ClientSize = new Size(System.Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                DialogResult dialogResult = form.ShowDialog();
                value = textBox.Text;
                return dialogResult;
            }
        }

        private void deleteWorkflowButton_Click_1(object sender, EventArgs e)
        {

        }

        private void runWorkflowButton_Click_1(object sender, EventArgs e)
        {

        }
    }
}
