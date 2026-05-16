using System;
using System.Windows.Forms;
using PyClickerRecorder.Workflow;
using static PyClickerRecorder.SavedMacro;

namespace PyClickerRecorder
{
    public partial class WorkflowScheduleForm : Form
    {
        private enum ScheduleMode { SpecificTime, TimeRange }
        
        private ScheduleMode _scheduleMode;
        private PyClickerRecorder.Workflow.Workflow _workflow;
        
        // Controls
        private DateTimePicker scheduleDatePicker;
        private DateTimePicker scheduleTimePicker;
        private DateTimePicker scheduleEndTimePicker;
        private CheckBox repeatDailyCheckBox;
        private RadioButton specificTimeRadio;
        private RadioButton timeRangeRadio;
        private RadioButton onStartupRadio;
        private Button setScheduleButton;
        private Button cancelButton;
        private Label scheduleDateLabel;
        private Label scheduleTimeLabel;
        private Label scheduleEndTimeLabel;
        private GroupBox scheduleTypeGroupBox;
        private GroupBox scheduleDetailsGroupBox;

        public WorkflowScheduleForm(PyClickerRecorder.Workflow.Workflow workflow)
        {
            _workflow = workflow;
            _scheduleMode = ScheduleMode.SpecificTime;
            InitializeComponent();
            LoadWorkflowScheduleSettings();
        }

        private void InitializeComponent()
        {
            this.Text = $"Schedule Workflow: {_workflow.Name}";
            this.Size = new Size(420, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Schedule Type GroupBox
            scheduleTypeGroupBox = new GroupBox
            {
                Text = "Schedule Type",
                Location = new Point(12, 12),
                Size = new Size(380, 100)
            };

            specificTimeRadio = new RadioButton
            {
                Text = "Specific Time",
                Location = new Point(15, 25),
                Size = new Size(100, 20),
                Checked = true
            };
            specificTimeRadio.CheckedChanged += SpecificTimeRadio_CheckedChanged;

            timeRangeRadio = new RadioButton
            {
                Text = "Time Range",
                Location = new Point(15, 50),
                Size = new Size(100, 20)
            };
            timeRangeRadio.CheckedChanged += TimeRangeRadio_CheckedChanged;

            onStartupRadio = new RadioButton
            {
                Text = "On Windows Startup",
                Location = new Point(15, 75),
                Size = new Size(150, 20)
            };
            onStartupRadio.CheckedChanged += OnStartupRadio_CheckedChanged;

            scheduleTypeGroupBox.Controls.Add(specificTimeRadio);
            scheduleTypeGroupBox.Controls.Add(timeRangeRadio);
            scheduleTypeGroupBox.Controls.Add(onStartupRadio);

            // Schedule Details GroupBox
            scheduleDetailsGroupBox = new GroupBox
            {
                Text = "Schedule Details",
                Location = new Point(12, 120),
                Size = new Size(380, 160)
            };

            scheduleDateLabel = new Label
            {
                Text = "Date:",
                Location = new Point(15, 30),
                Size = new Size(40, 20)
            };

            scheduleDatePicker = new DateTimePicker
            {
                Location = new Point(65, 28),
                Size = new Size(200, 23),
                Value = DateTime.Today.AddDays(1)
            };

            scheduleTimeLabel = new Label
            {
                Text = "Time:",
                Location = new Point(15, 60),
                Size = new Size(40, 20)
            };

            scheduleTimePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Location = new Point(65, 58),
                Size = new Size(100, 23),
                Value = DateTime.Today.Add(TimeSpan.FromHours(9))
            };

            scheduleEndTimeLabel = new Label
            {
                Text = "End Time:",
                Location = new Point(180, 60),
                Size = new Size(60, 20),
                Visible = false
            };

            scheduleEndTimePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Location = new Point(245, 58),
                Size = new Size(100, 23),
                Value = DateTime.Today.Add(TimeSpan.FromHours(17)),
                Visible = false
            };

            repeatDailyCheckBox = new CheckBox
            {
                Text = "Repeat Daily",
                Location = new Point(15, 95),
                Size = new Size(100, 20)
            };

            scheduleDetailsGroupBox.Controls.Add(scheduleDateLabel);
            scheduleDetailsGroupBox.Controls.Add(scheduleDatePicker);
            scheduleDetailsGroupBox.Controls.Add(scheduleTimeLabel);
            scheduleDetailsGroupBox.Controls.Add(scheduleTimePicker);
            scheduleDetailsGroupBox.Controls.Add(scheduleEndTimeLabel);
            scheduleDetailsGroupBox.Controls.Add(scheduleEndTimePicker);
            scheduleDetailsGroupBox.Controls.Add(repeatDailyCheckBox);

            // Buttons
            setScheduleButton = new Button
            {
                Text = "Set Schedule",
                Location = new Point(225, 290),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            setScheduleButton.Click += SetScheduleButton_Click;

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(315, 290),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(scheduleTypeGroupBox);
            this.Controls.Add(scheduleDetailsGroupBox);
            this.Controls.Add(setScheduleButton);
            this.Controls.Add(cancelButton);

            this.AcceptButton = setScheduleButton;
            this.CancelButton = cancelButton;
        }

        private void LoadWorkflowScheduleSettings()
        {
            switch (_workflow.ScheduleMode)
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
            }

            if (_workflow.ScheduledTime != default(DateTime))
            {
                scheduleDatePicker.Value = _workflow.ScheduledTime.Date;
                scheduleTimePicker.Value = _workflow.ScheduledTime;
            }

            if (_workflow.ScheduledEndTime != default(DateTime))
            {
                scheduleEndTimePicker.Value = _workflow.ScheduledEndTime;
            }

            repeatDailyCheckBox.Checked = _workflow.RepeatDaily;

            UpdateScheduleDetailsVisibility();
        }

        private void SpecificTimeRadio_CheckedChanged(object? sender, EventArgs e)
        {
            if (specificTimeRadio.Checked)
            {
                _scheduleMode = ScheduleMode.SpecificTime;
                UpdateScheduleDetailsVisibility();
            }
        }

        private void TimeRangeRadio_CheckedChanged(object? sender, EventArgs e)
        {
            if (timeRangeRadio.Checked)
            {
                _scheduleMode = ScheduleMode.TimeRange;
                UpdateScheduleDetailsVisibility();
            }
        }

        private void OnStartupRadio_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateScheduleDetailsVisibility();
        }

        private void UpdateScheduleDetailsVisibility()
        {
            bool showDetails = !onStartupRadio.Checked;
            bool showEndTime = timeRangeRadio.Checked && showDetails;

            scheduleDateLabel.Visible = showDetails;
            scheduleDatePicker.Visible = showDetails;
            scheduleTimeLabel.Visible = showDetails;
            scheduleTimePicker.Visible = showDetails;
            scheduleEndTimeLabel.Visible = showEndTime;
            scheduleEndTimePicker.Visible = showEndTime;
            repeatDailyCheckBox.Visible = showDetails;
        }

        private void SetScheduleButton_Click(object? sender, EventArgs e)
        {
            if (onStartupRadio.Checked)
            {
                _workflow.ScheduleMode = MacroScheduleMode.OnWindowsStartup;
                _workflow.RepeatDaily = false;
                MessageBox.Show($"Workflow '{_workflow.Name}' scheduled to run on Windows startup.", 
                    "Schedule Set", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime date = scheduleDatePicker.Value.Date;
            DateTime startTime = scheduleTimePicker.Value;
            DateTime endTime = scheduleEndTimePicker.Value;

            DateTime scheduledStart = date.Add(startTime.TimeOfDay);
            DateTime scheduledEnd = date.Add(endTime.TimeOfDay);

            // Validation
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

            // Save schedule settings to workflow
            if (_scheduleMode == ScheduleMode.SpecificTime)
            {
                _workflow.ScheduleMode = MacroScheduleMode.SpecificTime;
            }
            else
            {
                _workflow.ScheduleMode = MacroScheduleMode.TimeRange;
            }

            _workflow.ScheduledTime = scheduledStart;
            _workflow.ScheduledEndTime = scheduledEnd;
            _workflow.RepeatDaily = repeatDailyCheckBox.Checked;

            // Message formatting
            string message;
            if (_scheduleMode == ScheduleMode.SpecificTime)
            {
                if (repeatDailyCheckBox.Checked)
                {
                    message = $"Workflow '{_workflow.Name}' scheduled to run daily at: {scheduledStart:T}";
                }
                else
                {
                    message = $"Workflow '{_workflow.Name}' scheduled to run at: {scheduledStart:g}";
                }
            }
            else // TimeRange mode
            {
                if (repeatDailyCheckBox.Checked)
                {
                    message = $"Workflow '{_workflow.Name}' scheduled to run daily between {scheduledStart:T} and {scheduledEnd:T}.";
                }
                else
                {
                    message = $"Workflow '{_workflow.Name}' scheduled to run on {scheduledStart:d} between {scheduledStart:T} and {scheduledEnd:T}.";
                }
            }

            MessageBox.Show(message, "Schedule Set", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}