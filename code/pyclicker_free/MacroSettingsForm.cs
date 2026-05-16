using System;
using System.Windows.Forms;

namespace PyClickerRecorder
{
    public partial class MacroSettingsForm : Form
    {
        private enum ScheduleMode { SpecificTime, TimeRange }
        
        private ScheduleMode _scheduleMode;
        private SavedMacro _macro;
        
        // Controls
        private GroupBox generalGroupBox;
        private Label loopCountLabel;
        private NumericUpDown loopCountNumeric;
        private CheckBox infiniteLoopCheckBox;
        private Label speedLabel;
        private TrackBar speedTrackBar;
        private Label speedValueLabel;
        
        private GroupBox scheduleGroupBox;
        private RadioButton noScheduleRadio;
        private RadioButton specificTimeRadio;
        private RadioButton timeRangeRadio;
        private RadioButton onStartupRadio;
        
        private DateTimePicker scheduleDatePicker;
        private DateTimePicker scheduleTimePicker;
        private DateTimePicker scheduleEndTimePicker;
        private CheckBox repeatDailyCheckBox;
        private Label scheduleDateLabel;
        private Label scheduleTimeLabel;
        private Label scheduleEndTimeLabel;
        
        private Button okButton;
        private Button cancelButton;

        public MacroSettingsForm(SavedMacro macro)
        {
            _macro = macro;
            _scheduleMode = ScheduleMode.SpecificTime;
            InitializeComponent();
            LoadMacroSettings();
        }

        private void InitializeComponent()
        {
            this.Text = $"Macro Settings: {_macro.Name}";
            this.Size = new Size(450, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // General Settings GroupBox
            generalGroupBox = new GroupBox
            {
                Text = "General Settings",
                Location = new Point(12, 12),
                Size = new Size(410, 120)
            };

            loopCountLabel = new Label
            {
                Text = "Loop Count:",
                Location = new Point(15, 25),
                Size = new Size(80, 20)
            };

            loopCountNumeric = new NumericUpDown
            {
                Location = new Point(100, 23),
                Size = new Size(80, 23),
                Minimum = 1,
                Maximum = 9999,
                Value = _macro.LoopCount
            };

            infiniteLoopCheckBox = new CheckBox
            {
                Text = "Infinite Loop",
                Location = new Point(200, 25),
                Size = new Size(100, 20),
                Checked = _macro.InfiniteLoop
            };

            speedLabel = new Label
            {
                Text = "Speed:",
                Location = new Point(15, 55),
                Size = new Size(50, 20)
            };

            speedTrackBar = new TrackBar
            {
                Location = new Point(70, 50),
                Size = new Size(200, 45),
                Minimum = 1,
                Maximum = 50,
                Value = (int)(_macro.SpeedFactor * 10),
                TickFrequency = 5
            };
            speedTrackBar.ValueChanged += SpeedTrackBar_ValueChanged;

            speedValueLabel = new Label
            {
                Text = $"{_macro.SpeedFactor:F1}x",
                Location = new Point(280, 55),
                Size = new Size(40, 20)
            };

            generalGroupBox.Controls.Add(loopCountLabel);
            generalGroupBox.Controls.Add(loopCountNumeric);
            generalGroupBox.Controls.Add(infiniteLoopCheckBox);
            generalGroupBox.Controls.Add(speedLabel);
            generalGroupBox.Controls.Add(speedTrackBar);
            generalGroupBox.Controls.Add(speedValueLabel);

            // Schedule GroupBox
            scheduleGroupBox = new GroupBox
            {
                Text = "Schedule Settings",
                Location = new Point(12, 140),
                Size = new Size(410, 280)
            };

            noScheduleRadio = new RadioButton
            {
                Text = "No Schedule (Manual execution only)",
                Location = new Point(15, 25),
                Size = new Size(250, 20),
                Checked = true
            };
            noScheduleRadio.CheckedChanged += ScheduleRadio_CheckedChanged;

            specificTimeRadio = new RadioButton
            {
                Text = "Run at specific time",
                Location = new Point(15, 50),
                Size = new Size(150, 20)
            };
            specificTimeRadio.CheckedChanged += ScheduleRadio_CheckedChanged;

            timeRangeRadio = new RadioButton
            {
                Text = "Run in time range",
                Location = new Point(15, 75),
                Size = new Size(150, 20)
            };
            timeRangeRadio.CheckedChanged += ScheduleRadio_CheckedChanged;

            onStartupRadio = new RadioButton
            {
                Text = "Run on Windows startup",
                Location = new Point(15, 100),
                Size = new Size(180, 20)
            };
            onStartupRadio.CheckedChanged += ScheduleRadio_CheckedChanged;

            scheduleDateLabel = new Label
            {
                Text = "Date:",
                Location = new Point(15, 135),
                Size = new Size(40, 20)
            };

            scheduleDatePicker = new DateTimePicker
            {
                Location = new Point(65, 133),
                Size = new Size(200, 23),
                Value = DateTime.Today.AddDays(1)
            };

            scheduleTimeLabel = new Label
            {
                Text = "Start Time:",
                Location = new Point(15, 165),
                Size = new Size(70, 20)
            };

            scheduleTimePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Location = new Point(90, 163),
                Size = new Size(100, 23),
                Value = DateTime.Today.Add(TimeSpan.FromHours(9))
            };

            scheduleEndTimeLabel = new Label
            {
                Text = "End Time:",
                Location = new Point(200, 165),
                Size = new Size(70, 20),
                Visible = false
            };

            scheduleEndTimePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Location = new Point(275, 163),
                Size = new Size(100, 23),
                Value = DateTime.Today.Add(TimeSpan.FromHours(17)),
                Visible = false
            };

            repeatDailyCheckBox = new CheckBox
            {
                Text = "Repeat Daily",
                Location = new Point(15, 195),
                Size = new Size(100, 20)
            };

            scheduleGroupBox.Controls.Add(noScheduleRadio);
            scheduleGroupBox.Controls.Add(specificTimeRadio);
            scheduleGroupBox.Controls.Add(timeRangeRadio);
            scheduleGroupBox.Controls.Add(onStartupRadio);
            scheduleGroupBox.Controls.Add(scheduleDateLabel);
            scheduleGroupBox.Controls.Add(scheduleDatePicker);
            scheduleGroupBox.Controls.Add(scheduleTimeLabel);
            scheduleGroupBox.Controls.Add(scheduleTimePicker);
            scheduleGroupBox.Controls.Add(scheduleEndTimeLabel);
            scheduleGroupBox.Controls.Add(scheduleEndTimePicker);
            scheduleGroupBox.Controls.Add(repeatDailyCheckBox);

            // Buttons
            okButton = new Button
            {
                Text = "OK",
                Location = new Point(265, 430),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(350, 430),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(generalGroupBox);
            this.Controls.Add(scheduleGroupBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            UpdateScheduleVisibility();
        }

        private void LoadMacroSettings()
        {
            // Load general settings
            loopCountNumeric.Value = _macro.LoopCount;
            infiniteLoopCheckBox.Checked = _macro.InfiniteLoop;
            speedTrackBar.Value = (int)(_macro.SpeedFactor * 10);
            speedValueLabel.Text = $"{_macro.SpeedFactor:F1}x";

            // Load schedule settings
            switch (_macro.ScheduleMode)
            {
                case MacroScheduleMode.SpecificTime:
                    specificTimeRadio.Checked = true;
                    _scheduleMode = ScheduleMode.SpecificTime;
                    break;
                case MacroScheduleMode.TimeRange:
                    timeRangeRadio.Checked = true;
                    _scheduleMode = ScheduleMode.TimeRange;
                    break;
                case MacroScheduleMode.OnWindowsStartup:
                    onStartupRadio.Checked = true;
                    break;
                default:
                    noScheduleRadio.Checked = true;
                    break;
            }

            if (_macro.ScheduledTime.Year > 1900)
            {
                scheduleDatePicker.Value = _macro.ScheduledTime.Date;
                scheduleTimePicker.Value = _macro.ScheduledTime;
            }

            if (_macro.ScheduledEndTime.Year > 1900)
            {
                scheduleEndTimePicker.Value = _macro.ScheduledEndTime;
            }

            repeatDailyCheckBox.Checked = _macro.RepeatDaily;

            UpdateScheduleVisibility();
        }

        private void SpeedTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            double speed = speedTrackBar.Value / 10.0;
            speedValueLabel.Text = $"{speed:F1}x";
        }

        private void ScheduleRadio_CheckedChanged(object? sender, EventArgs e)
        {
            if (specificTimeRadio.Checked)
                _scheduleMode = ScheduleMode.SpecificTime;
            else if (timeRangeRadio.Checked)
                _scheduleMode = ScheduleMode.TimeRange;

            UpdateScheduleVisibility();
        }

        private void UpdateScheduleVisibility()
        {
            bool showScheduleDetails = !noScheduleRadio.Checked && !onStartupRadio.Checked;
            bool showEndTime = timeRangeRadio.Checked && showScheduleDetails;

            scheduleDateLabel.Visible = showScheduleDetails;
            scheduleDatePicker.Visible = showScheduleDetails;
            scheduleTimeLabel.Visible = showScheduleDetails;
            scheduleTimePicker.Visible = showScheduleDetails;
            scheduleEndTimeLabel.Visible = showEndTime;
            scheduleEndTimePicker.Visible = showEndTime;
            repeatDailyCheckBox.Visible = showScheduleDetails;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            // Validate schedule settings
            if (!noScheduleRadio.Checked && !onStartupRadio.Checked)
            {
                DateTime date = scheduleDatePicker.Value.Date;
                DateTime startTime = scheduleTimePicker.Value;
                DateTime endTime = scheduleEndTimePicker.Value;

                DateTime scheduledStart = date.Add(startTime.TimeOfDay);
                DateTime scheduledEnd = date.Add(endTime.TimeOfDay);

                if (_scheduleMode == ScheduleMode.TimeRange && scheduledStart.TimeOfDay >= scheduledEnd.TimeOfDay)
                {
                    MessageBox.Show("End time must be after start time.", "Invalid Time Range", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!repeatDailyCheckBox.Checked)
                {
                    DateTime checkTime = (_scheduleMode == ScheduleMode.SpecificTime) ? scheduledStart : scheduledEnd;
                    if (checkTime < DateTime.Now)
                    {
                        MessageBox.Show("The scheduled time cannot be in the past.", "Invalid Time", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            // Save settings back to macro
            SaveSettingsToMacro();
        }

        private void SaveSettingsToMacro()
        {
            // Save general settings
            _macro.LoopCount = (int)loopCountNumeric.Value;
            _macro.InfiniteLoop = infiniteLoopCheckBox.Checked;
            _macro.SpeedFactor = speedTrackBar.Value / 10.0;

            // Save schedule settings
            if (noScheduleRadio.Checked)
            {
                _macro.ScheduleMode = (MacroScheduleMode)0; // No schedule
                _macro.ScheduledTime = new DateTime(2000, 1, 1);
                _macro.ScheduledEndTime = new DateTime(2000, 1, 1);
                _macro.RepeatDaily = false;
            }
            else if (onStartupRadio.Checked)
            {
                _macro.ScheduleMode = MacroScheduleMode.OnWindowsStartup;
                _macro.ScheduledTime = new DateTime(2000, 1, 1);
                _macro.ScheduledEndTime = new DateTime(2000, 1, 1);
                _macro.RepeatDaily = false;
            }
            else
            {
                DateTime date = scheduleDatePicker.Value.Date;
                DateTime startTime = scheduleTimePicker.Value;
                DateTime endTime = scheduleEndTimePicker.Value;

                DateTime scheduledStart = date.Add(startTime.TimeOfDay);
                DateTime scheduledEnd = date.Add(endTime.TimeOfDay);

                if (_scheduleMode == ScheduleMode.SpecificTime)
                {
                    _macro.ScheduleMode = MacroScheduleMode.SpecificTime;
                }
                else
                {
                    _macro.ScheduleMode = MacroScheduleMode.TimeRange;
                }

                _macro.ScheduledTime = scheduledStart;
                _macro.ScheduledEndTime = scheduledEnd;
                _macro.RepeatDaily = repeatDailyCheckBox.Checked;
            }
        }
    }
}