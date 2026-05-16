using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PyClickerRecorder.Workflow
{
    public partial class BlockPropertiesForm : Form
    {
        private readonly WorkflowBlock _block;
        private readonly List<SavedMacro> _availableMacros;
        private readonly PyClickerRecorder.Workflow.Workflow _workflow;
        
        // Constants for placeholder texts
        private const string PLACEHOLDER_STATIC_VALUE = "Enter static value";
        private const string PLACEHOLDER_PROMPT_TEXT = "Enter prompt text";
        private const string PLACEHOLDER_CLIPBOARD = "(Current clipboard content)";
        private const string PLACEHOLDER_WINDOW_TITLE = "(Current window title)";
        private const string PLACEHOLDER_LAST_ACTIVE_WINDOW = "(Last active window title)";
        private const string PLACEHOLDER_NOT_SUPPORTED = "(Not supported in conditions)";
        private const string PLACEHOLDER_TYPE_VARIABLE = "(Type variable name)";
        
        // Common controls
        private TextBox _nameTextBox;
        private CheckBox _enabledCheckBox;
        
        // Macro block controls
        private ComboBox _macroComboBox;
        private NumericUpDown _loopCountNumeric;
        private CheckBox _infiniteLoopCheckBox;
        private TrackBar _speedTrackBar;
        private Label _speedLabel;
        
        // Conditional block controls
        private ComboBox _leftSourceComboBox;
        private TextBox _leftValueTextBox;
        private ComboBox _leftVariableComboBox; // For Variable source dropdown
        private ComboBox _conditionTypeComboBox;
        private ComboBox _rightSourceComboBox;
        private TextBox _rightValueTextBox;
        private ComboBox _rightVariableComboBox; // For Variable source dropdown
        private CheckBox _caseSensitiveCheckBox;
        
        // Loop block controls
        private ComboBox _loopTypeComboBox;
        private NumericUpDown _loopCountNumeric2;
        private NumericUpDown _durationHoursNumeric;
        private NumericUpDown _durationMinutesNumeric;
        private NumericUpDown _durationSecondsNumeric;
        private ComboBox _exitConditionComboBox;
        private TextBox _exitConditionValueTextBox;
        
        // Delay block controls
        private NumericUpDown _delaySecondsNumeric;
        private CheckBox _randomDelayCheckBox;
        private NumericUpDown _minDelayNumeric;
        private NumericUpDown _maxDelayNumeric;
        
        // Variable block controls
        private TextBox _variableNameTextBox;
        private ComboBox _variableSourceComboBox;
        private TextBox _variableSourceValueTextBox;
        private ComboBox _variableSourceComboBox2; // For Variable source dropdown
        private TextBox _directValueTextBox; // For direct value input
        private Button _setValueButton; // Set Value button

        // Switch Case block controls
        private ComboBox? _switchSourceComboBox;
        private TextBox? _switchValueTextBox;
        private ComboBox? _switchVariableComboBox; // For Variable source dropdown
        private Button? _switchSelectWindowButton;
        private ComboBox? _initialMacroComboBox; // For selecting initial macro
        private CheckBox? _switchCaseSensitiveCheckBox;
        private ListBox? _casesListBox;
        private Button? _addCaseButton;
        private Button? _removeCaseButton;
        private Button? _addDefaultCaseButton;
        private Panel? _caseEditPanel;
        private ComboBox? _caseSourceComboBox;
        private TextBox? _caseValueTextBox;
        private ComboBox? _caseVariableComboBox;
        private Button? _caseSelectWindowButton;
        private Label? _caseBlocksInfoLabel; // For displaying connected blocks information
        private CheckBox? _isDefaultCaseCheckBox;

        // Window selection buttons for other blocks
        private Button? _leftSelectWindowButton;
        private Button? _rightSelectWindowButton;
        private Button? _variableSelectWindowButton;

        public BlockPropertiesForm(WorkflowBlock block, List<SavedMacro> availableMacros, PyClickerRecorder.Workflow.Workflow workflow = null)
        {
            _block = block;
            _availableMacros = availableMacros ?? [];
            _workflow = workflow;
            
            InitializeComponent();
            SetupForm();
            LoadBlockProperties();
        }
        
        // Helper method to check if text is a UI placeholder that shouldn't be saved
        private static bool IsUIPlaceholderText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
                
            // Only these exact UI placeholders should be filtered out when saving
            return text == PLACEHOLDER_STATIC_VALUE ||
                   text == PLACEHOLDER_PROMPT_TEXT ||
                   text == PLACEHOLDER_TYPE_VARIABLE;
        }
        
        // Helper method to check if text is any kind of placeholder (for UI display logic)
        private static bool IsAnyPlaceholderText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
                
            return text == PLACEHOLDER_STATIC_VALUE ||
                   text == PLACEHOLDER_PROMPT_TEXT ||
                   text == PLACEHOLDER_CLIPBOARD ||
                   text == PLACEHOLDER_WINDOW_TITLE ||
                   text == PLACEHOLDER_LAST_ACTIVE_WINDOW ||
                   text == PLACEHOLDER_NOT_SUPPORTED ||
                   text == PLACEHOLDER_TYPE_VARIABLE;
        }

        private void SetupForm()
        {
            this.Text = $"Properties - {_block.Name}";
            this.Size = new Size(400, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            
            CreateCommonControls();
            CreateTypeSpecificControls();
            CreateButtons();
        }

        private void CreateCommonControls()
        {
            // Name
            var nameLabel = new Label
            {
                Text = "Name:",
                Location = new Point(12, 15),
                Size = new Size(60, 23)
            };
            this.Controls.Add(nameLabel);
            
            _nameTextBox = new TextBox
            {
                Location = new Point(80, 12),
                Size = new Size(280, 23)
            };
            this.Controls.Add(_nameTextBox);
            
            // Enabled
            _enabledCheckBox = new CheckBox
            {
                Text = "Enabled",
                Location = new Point(12, 45),
                Size = new Size(100, 23),
                Checked = true
            };
            this.Controls.Add(_enabledCheckBox);
        }

        private void CreateTypeSpecificControls()
        {
            int yOffset = 80;
            
            switch (_block.Type)
            {
                case WorkflowBlockType.MacroBlock:
                    CreateMacroBlockControls(ref yOffset);
                    break;
                case WorkflowBlockType.ConditionalBlock:
                    CreateConditionalBlockControls(ref yOffset);
                    break;
                case WorkflowBlockType.LoopBlock:
                    CreateLoopBlockControls(ref yOffset);
                    break;
                case WorkflowBlockType.DelayBlock:
                    CreateDelayBlockControls(ref yOffset);
                    break;
                case WorkflowBlockType.VariableBlock:
                    CreateVariableBlockControls(ref yOffset);
                    break;
                case WorkflowBlockType.SwitchCaseBlock:
                    CreateSwitchCaseBlockControls(ref yOffset);
                    break;
            }
            
            // Adjust form height based on controls
            this.Height = Math.Max(500, yOffset + 80);
        }

        private void CreateMacroBlockControls(ref int yOffset)
        {
            // Macro selection
            var macroLabel = new Label
            {
                Text = "Macro:",
                Location = new Point(12, yOffset),
                Size = new Size(60, 23)
            };
            this.Controls.Add(macroLabel);
            
            _macroComboBox = new ComboBox
            {
                Location = new Point(80, yOffset),
                Size = new Size(280, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _macroComboBox.Items.Clear();
            _macroComboBox.Items.Add("(Record New Macro)");
            
            if (_availableMacros != null && _availableMacros.Count > 0)
            {
                foreach (var macro in _availableMacros)
                {
                    if (!string.IsNullOrEmpty(macro.Name))
                    {
                        _macroComboBox.Items.Add(macro.Name);
                    }
                }
            }
            else
            {
                _macroComboBox.Items.Add("(No saved macros found)");
            }
            
            // Set default selection
            if (_macroComboBox.Items.Count > 1)
            {
                _macroComboBox.SelectedIndex = 0;
            }
            this.Controls.Add(_macroComboBox);
            yOffset += 35;
            
            // Loop count
            var loopLabel = new Label
            {
                Text = "Loop Count:",
                Location = new Point(12, yOffset),
                Size = new Size(80, 23)
            };
            this.Controls.Add(loopLabel);
            
            _loopCountNumeric = new NumericUpDown
            {
                Location = new Point(100, yOffset),
                Size = new Size(80, 23),
                Minimum = 1,
                Maximum = 10000,
                Value = 1
            };
            this.Controls.Add(_loopCountNumeric);
            
            _infiniteLoopCheckBox = new CheckBox
            {
                Text = "Infinite Loop",
                Location = new Point(200, yOffset),
                Size = new Size(100, 23)
            };
            _infiniteLoopCheckBox.CheckedChanged += (s, e) =>
            {
                _loopCountNumeric.Enabled = !_infiniteLoopCheckBox.Checked;
            };
            this.Controls.Add(_infiniteLoopCheckBox);
            yOffset += 35;
            
            // Speed
            var speedLabel = new Label
            {
                Text = "Speed:",
                Location = new Point(12, yOffset),
                Size = new Size(60, 23)
            };
            this.Controls.Add(speedLabel);
            
            _speedTrackBar = new TrackBar
            {
                Location = new Point(80, yOffset),
                Size = new Size(200, 45),
                Minimum = 1,
                Maximum = 20,
                Value = 10,
                TickFrequency = 5
            };
            _speedTrackBar.ValueChanged += SpeedTrackBar_ValueChanged;
            this.Controls.Add(_speedTrackBar);
            
            _speedLabel = new Label
            {
                Text = "1.0x",
                Location = new Point(290, yOffset + 10),
                Size = new Size(60, 23)
            };
            this.Controls.Add(_speedLabel);
            yOffset += 50;
        }

        private void CreateConditionalBlockControls(ref int yOffset)
        {
            // Left source
            var leftSourceLabel = new Label
            {
                Text = "Left Source:",
                Location = new Point(12, yOffset),
                Size = new Size(80, 23)
            };
            this.Controls.Add(leftSourceLabel);
            
            _leftSourceComboBox = new ComboBox
            {
                Location = new Point(100, yOffset),
                Size = new Size(120, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (VariableSource source in Enum.GetValues(typeof(VariableSource)))
            {
                _leftSourceComboBox.Items.Add(source.ToString());
            }
            _leftSourceComboBox.SelectedIndexChanged += LeftSourceComboBox_SelectedIndexChanged;
            this.Controls.Add(_leftSourceComboBox);
            
            _leftValueTextBox = new TextBox
            {
                Location = new Point(230, yOffset),
                Size = new Size(130, 23)
            };
            _leftValueTextBox.Enter += TextBox_Enter;
            _leftValueTextBox.Leave += TextBox_Leave;
            this.Controls.Add(_leftValueTextBox);

            // Left window selection button (for WindowTitle source only) - positioned below text field
            _leftSelectWindowButton = CreateWindowSelectButton(new Point(230, yOffset + 30), "left");
            this.Controls.Add(_leftSelectWindowButton);
            
            // Left variable dropdown (for Variable source only)
            _leftVariableComboBox = new ComboBox
            {
                Location = new Point(230, yOffset),
                Size = new Size(130, 23),
                DropDownStyle = ComboBoxStyle.DropDown,
                Visible = false // Initially hidden
            };
            PopulateVariableComboBox(_leftVariableComboBox);
            this.Controls.Add(_leftVariableComboBox);
            
            yOffset += 60; // Extra space for window selection button below
            
            // Condition type
            var conditionTypeLabel = new Label
            {
                Text = "Condition:",
                Location = new Point(12, yOffset),
                Size = new Size(80, 23)
            };
            this.Controls.Add(conditionTypeLabel);
            
            _conditionTypeComboBox = new ComboBox
            {
                Location = new Point(100, yOffset),
                Size = new Size(260, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (ConditionType conditionType in Enum.GetValues(typeof(ConditionType)))
            {
                _conditionTypeComboBox.Items.Add(conditionType.ToString());
            }
            this.Controls.Add(_conditionTypeComboBox);
            yOffset += 35;
            
            // Right source
            var rightSourceLabel = new Label
            {
                Text = "Right Source:",
                Location = new Point(12, yOffset),
                Size = new Size(80, 23)
            };
            this.Controls.Add(rightSourceLabel);
            
            _rightSourceComboBox = new ComboBox
            {
                Location = new Point(100, yOffset),
                Size = new Size(120, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (VariableSource source in Enum.GetValues(typeof(VariableSource)))
            {
                _rightSourceComboBox.Items.Add(source.ToString());
            }
            _rightSourceComboBox.SelectedIndexChanged += RightSourceComboBox_SelectedIndexChanged;
            this.Controls.Add(_rightSourceComboBox);
            
            _rightValueTextBox = new TextBox
            {
                Location = new Point(230, yOffset),
                Size = new Size(130, 23)
            };
            _rightValueTextBox.Enter += TextBox_Enter;
            _rightValueTextBox.Leave += TextBox_Leave;
            this.Controls.Add(_rightValueTextBox);

            // Right window selection button (for WindowTitle source only) - positioned below text field
            _rightSelectWindowButton = CreateWindowSelectButton(new Point(230, yOffset + 30), "right");
            this.Controls.Add(_rightSelectWindowButton);
            
            // Right variable dropdown (for Variable source only)
            _rightVariableComboBox = new ComboBox
            {
                Location = new Point(230, yOffset),
                Size = new Size(130, 23),
                DropDownStyle = ComboBoxStyle.DropDown,
                Visible = false // Initially hidden
            };
            PopulateVariableComboBox(_rightVariableComboBox);
            this.Controls.Add(_rightVariableComboBox);
            
            yOffset += 60; // Extra space for window selection button below
            
            // Case sensitive
            _caseSensitiveCheckBox = new CheckBox
            {
                Text = "Case Sensitive",
                Location = new Point(12, yOffset),
                Size = new Size(120, 23)
            };
            this.Controls.Add(_caseSensitiveCheckBox);
            yOffset += 35;
        }

        private void CreateLoopBlockControls(ref int yOffset)
        {
            // Loop type
            var loopTypeLabel = new Label
            {
                Text = "Loop Type:",
                Location = new Point(12, yOffset),
                Size = new Size(80, 23)
            };
            this.Controls.Add(loopTypeLabel);
            
            _loopTypeComboBox = new ComboBox
            {
                Location = new Point(100, yOffset),
                Size = new Size(260, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (LoopType loopType in Enum.GetValues(typeof(LoopType)))
            {
                _loopTypeComboBox.Items.Add(loopType.ToString());
            }
            _loopTypeComboBox.SelectedIndexChanged += LoopTypeComboBox_SelectedIndexChanged;
            this.Controls.Add(_loopTypeComboBox);
            yOffset += 35;
            
            // Count-based loop
            var countLabel = new Label
            {
                Text = "Count:",
                Location = new Point(12, yOffset),
                Size = new Size(60, 23)
            };
            this.Controls.Add(countLabel);
            
            _loopCountNumeric2 = new NumericUpDown
            {
                Location = new Point(80, yOffset),
                Size = new Size(80, 23),
                Minimum = 1,
                Maximum = 10000,
                Value = 1
            };
            this.Controls.Add(_loopCountNumeric2);
            yOffset += 35;
            
            // Duration-based loop
            var durationLabel = new Label
            {
                Text = "Duration:",
                Location = new Point(12, yOffset),
                Size = new Size(60, 23)
            };
            this.Controls.Add(durationLabel);
            
            _durationHoursNumeric = new NumericUpDown
            {
                Location = new Point(80, yOffset),
                Size = new Size(50, 23),
                Maximum = 23
            };
            this.Controls.Add(_durationHoursNumeric);
            
            var hoursLabel = new Label
            {
                Text = "h",
                Location = new Point(135, yOffset),
                Size = new Size(15, 23)
            };
            this.Controls.Add(hoursLabel);
            
            _durationMinutesNumeric = new NumericUpDown
            {
                Location = new Point(155, yOffset),
                Size = new Size(50, 23),
                Maximum = 59
            };
            this.Controls.Add(_durationMinutesNumeric);
            
            var minutesLabel = new Label
            {
                Text = "m",
                Location = new Point(210, yOffset),
                Size = new Size(15, 23)
            };
            this.Controls.Add(minutesLabel);
            
            _durationSecondsNumeric = new NumericUpDown
            {
                Location = new Point(230, yOffset),
                Size = new Size(50, 23),
                Maximum = 59
            };
            this.Controls.Add(_durationSecondsNumeric);
            
            var secondsLabel = new Label
            {
                Text = "s",
                Location = new Point(285, yOffset),
                Size = new Size(15, 23)
            };
            this.Controls.Add(secondsLabel);
            yOffset += 35;
            
            // Exit condition
            var exitConditionLabel = new Label
            {
                Text = "Exit Condition:",
                Location = new Point(12, yOffset),
                Size = new Size(100, 23)
            };
            this.Controls.Add(exitConditionLabel);
            
            _exitConditionComboBox = new ComboBox
            {
                Location = new Point(120, yOffset),
                Size = new Size(240, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (ConditionType conditionType in Enum.GetValues(typeof(ConditionType)))
            {
                _exitConditionComboBox.Items.Add(conditionType.ToString());
            }
            this.Controls.Add(_exitConditionComboBox);
            yOffset += 35;
            
            _exitConditionValueTextBox = new TextBox
            {
                Location = new Point(120, yOffset),
                Size = new Size(240, 23)
            };
            this.Controls.Add(_exitConditionValueTextBox);
            yOffset += 35;
        }

        private void CreateDelayBlockControls(ref int yOffset)
        {
            // Delay seconds
            var delayLabel = new Label
            {
                Text = "Delay (seconds):",
                Location = new Point(12, yOffset),
                Size = new Size(100, 23)
            };
            this.Controls.Add(delayLabel);
            
            _delaySecondsNumeric = new NumericUpDown
            {
                Location = new Point(120, yOffset),
                Size = new Size(80, 23),
                DecimalPlaces = 1,
                Minimum = 0.1m,
                Maximum = 3600m,
                Value = 1.0m,
                Increment = 0.1m
            };
            this.Controls.Add(_delaySecondsNumeric);
            yOffset += 35;
            
            // Random delay
            _randomDelayCheckBox = new CheckBox
            {
                Text = "Random Delay",
                Location = new Point(12, yOffset),
                Size = new Size(120, 23)
            };
            _randomDelayCheckBox.CheckedChanged += RandomDelayCheckBox_CheckedChanged;
            this.Controls.Add(_randomDelayCheckBox);
            yOffset += 35;
            
            // Min delay
            var minDelayLabel = new Label
            {
                Text = "Min (seconds):",
                Location = new Point(12, yOffset),
                Size = new Size(100, 23)
            };
            this.Controls.Add(minDelayLabel);
            
            _minDelayNumeric = new NumericUpDown
            {
                Location = new Point(120, yOffset),
                Size = new Size(80, 23),
                DecimalPlaces = 1,
                Minimum = 0.1m,
                Maximum = 3600m,
                Value = 0.5m,
                Increment = 0.1m,
                Enabled = false
            };
            this.Controls.Add(_minDelayNumeric);
            yOffset += 35;
            
            // Max delay
            var maxDelayLabel = new Label
            {
                Text = "Max (seconds):",
                Location = new Point(12, yOffset),
                Size = new Size(100, 23)
            };
            this.Controls.Add(maxDelayLabel);
            
            _maxDelayNumeric = new NumericUpDown
            {
                Location = new Point(120, yOffset),
                Size = new Size(80, 23),
                DecimalPlaces = 1,
                Minimum = 0.1m,
                Maximum = 3600m,
                Value = 2.0m,
                Increment = 0.1m,
                Enabled = false
            };
            this.Controls.Add(_maxDelayNumeric);
            yOffset += 35;
        }

        private void CreateVariableBlockControls(ref int yOffset)
        {
            _nameTextBox.ReadOnly = true;

            // Variable name (will sync with block name)
            var nameLabel = new Label
            {
                Text = "Variable Name:",
                Location = new Point(12, yOffset),
                Size = new Size(100, 23)
            };
            this.Controls.Add(nameLabel);
            
            _variableNameTextBox = new TextBox
            {
                Location = new Point(120, yOffset),
                Size = new Size(240, 23)
            };
            // Sync variable name with block name
            _variableNameTextBox.TextChanged += (s, e) => _nameTextBox.Text = _variableNameTextBox.Text;
            this.Controls.Add(_variableNameTextBox);
            yOffset += 35;
            
            // Variable source
            var sourceLabel = new Label
            {
                Text = "Source:",
                Location = new Point(12, yOffset),
                Size = new Size(100, 23)
            };
            this.Controls.Add(sourceLabel);
            
            _variableSourceComboBox = new ComboBox
            {
                Location = new Point(120, yOffset),
                Size = new Size(240, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (VariableSource source in Enum.GetValues(typeof(VariableSource)))
            {
                _variableSourceComboBox.Items.Add(source.ToString());
            }
            _variableSourceComboBox.SelectedIndexChanged += VariableSourceComboBox_SelectedIndexChanged;
            this.Controls.Add(_variableSourceComboBox);
            yOffset += 35;
            
            // Source value (for Clipboard, WindowTitle - read-only info)
            var sourceValueLabel = new Label
            {
                Text = "Source Info:",
                Location = new Point(12, yOffset),
                Size = new Size(100, 23)
            };
            this.Controls.Add(sourceValueLabel);
            
            _variableSourceValueTextBox = new TextBox
            {
                Location = new Point(120, yOffset),
                Size = new Size(240, 23),
                Enabled = false
            };
            this.Controls.Add(_variableSourceValueTextBox);

            // Variable window selection button (for WindowTitle source only) - positioned below text field
            _variableSelectWindowButton = CreateWindowSelectButton(new Point(120, yOffset + 30), "variable");
            this.Controls.Add(_variableSelectWindowButton);
            
            // Variable dropdown (for Variable source only)
            _variableSourceComboBox2 = new ComboBox
            {
                Location = new Point(120, yOffset),
                Size = new Size(240, 23),
                DropDownStyle = ComboBoxStyle.DropDown,
                Visible = false // Initially hidden
            };
            PopulateVariableComboBox(_variableSourceComboBox2);
            this.Controls.Add(_variableSourceComboBox2);
            yOffset += 60; // Extra space for window selection button below
            
            // Direct value input (for Value source)
            var valueLabel = new Label
            {
                Text = "Value:",
                Location = new Point(12, yOffset),
                Size = new Size(100, 23)
            };
            this.Controls.Add(valueLabel);
            
            _directValueTextBox = new TextBox
            {
                Location = new Point(120, yOffset),
                Size = new Size(160, 23),
                Visible = false // Initially hidden
            };
            this.Controls.Add(_directValueTextBox);
            
            _setValueButton = new Button
            {
                Text = "Set Value",
                Location = new Point(290, yOffset),
                Size = new Size(70, 23),
                Visible = false // Initially hidden
            };
            _setValueButton.Click += SetValueButton_Click;
            this.Controls.Add(_setValueButton);
            
            yOffset += 35;
        }

        private void CreateSwitchCaseBlockControls(ref int yOffset)
        {
            // Switch source
            var switchSourceLabel = new Label
            {
                Text = "Switch Source:",
                Location = new Point(12, yOffset),
                Size = new Size(100, 23)
            };
            this.Controls.Add(switchSourceLabel);

            _switchSourceComboBox = new ComboBox
            {
                Location = new Point(120, yOffset),
                Size = new Size(120, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (SwitchSourceType source in Enum.GetValues(typeof(SwitchSourceType)))
            {
                _switchSourceComboBox.Items.Add(source.ToString());
            }
            _switchSourceComboBox.SelectedIndexChanged += SwitchSourceComboBox_SelectedIndexChanged;
            this.Controls.Add(_switchSourceComboBox);
            yOffset += 35;

            // Initial macro selection
            var initialMacroLabel = new Label
            {
                Text = "Initial Macro:",
                Location = new Point(12, yOffset),
                Size = new Size(100, 23)
            };
            this.Controls.Add(initialMacroLabel);

            _initialMacroComboBox = new ComboBox
            {
                Location = new Point(120, yOffset),
                Size = new Size(240, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            PopulateMacroComboBox(_initialMacroComboBox);
            this.Controls.Add(_initialMacroComboBox);
            yOffset += 35;

            // Case sensitive
            _switchCaseSensitiveCheckBox = new CheckBox
            {
                Text = "Case Sensitive",
                Location = new Point(12, yOffset),
                Size = new Size(120, 23)
            };
            this.Controls.Add(_switchCaseSensitiveCheckBox);
            yOffset += 35;

            // Cases list
            var casesLabel = new Label
            {
                Text = "Cases:",
                Location = new Point(12, yOffset),
                Size = new Size(60, 23)
            };
            this.Controls.Add(casesLabel);
            yOffset += 25;

            _casesListBox = new ListBox
            {
                Location = new Point(12, yOffset),
                Size = new Size(250, 120)
            };
            _casesListBox.SelectedIndexChanged += CasesListBox_SelectedIndexChanged;
            this.Controls.Add(_casesListBox);

            // Case management buttons
            _addCaseButton = new Button
            {
                Text = "Add Case",
                Location = new Point(270, yOffset),
                Size = new Size(90, 25)
            };
            _addCaseButton.Click += AddCaseButton_Click;
            this.Controls.Add(_addCaseButton);

            _addDefaultCaseButton = new Button
            {
                Text = "Add Default",
                Location = new Point(270, yOffset + 30),
                Size = new Size(90, 25)
            };
            _addDefaultCaseButton.Click += AddDefaultCaseButton_Click;
            this.Controls.Add(_addDefaultCaseButton);

            _removeCaseButton = new Button
            {
                Text = "Remove Case",
                Location = new Point(270, yOffset + 60),
                Size = new Size(90, 25),
                Enabled = false
            };
            _removeCaseButton.Click += RemoveCaseButton_Click;
            this.Controls.Add(_removeCaseButton);

            yOffset += 130;

            // Case edit panel
            var caseEditLabel = new Label
            {
                Text = "Case Properties:",
                Location = new Point(12, yOffset),
                Size = new Size(100, 23)
            };
            this.Controls.Add(caseEditLabel);
            yOffset += 25;

            _caseEditPanel = new Panel
            {
                Location = new Point(12, yOffset),
                Size = new Size(348, 120), // Increased height for additional info
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false
            };

            // Case value (simplified - just a static value to match against)
            var caseValueLabel = new Label
            {
                Text = "Match Value:",
                Location = new Point(8, 8),
                Size = new Size(100, 23)
            };
            _caseEditPanel.Controls.Add(caseValueLabel);

            _caseValueTextBox = new TextBox
            {
                Location = new Point(115, 8),
                Size = new Size(220, 23)
            };
            _caseValueTextBox.Enter += TextBox_Enter;
            _caseValueTextBox.Leave += TextBox_Leave;
            _caseValueTextBox.TextChanged += CaseValueTextBox_TextChanged;
            _caseEditPanel.Controls.Add(_caseValueTextBox);

            // Case window selection button (for ActiveWindow switch only)
            _caseSelectWindowButton = CreateWindowSelectButton(new Point(115, 38), "case");
            _caseEditPanel.Controls.Add(_caseSelectWindowButton);

            // Is default case
            _isDefaultCaseCheckBox = new CheckBox
            {
                Text = "Is Default Case",
                Location = new Point(8, 40),
                Size = new Size(120, 23)
            };
            _isDefaultCaseCheckBox.CheckedChanged += IsDefaultCaseCheckBox_CheckedChanged;
            _caseEditPanel.Controls.Add(_isDefaultCaseCheckBox);

            // Case blocks information (read-only display)
            var caseBlocksLabel = new Label
            {
                Text = "Connected Blocks:",
                Location = new Point(8, 70),
                Size = new Size(100, 23)
            };
            _caseEditPanel.Controls.Add(caseBlocksLabel);

            _caseBlocksInfoLabel = new Label
            {
                Text = "(No blocks connected - use visual designer)",
                Location = new Point(115, 70),
                Size = new Size(220, 23),
                ForeColor = System.Drawing.Color.Gray,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic)
            };
            _caseEditPanel.Controls.Add(_caseBlocksInfoLabel);

            // Instructions for using the visual designer
            var instructionsLabel = new Label
            {
                Text = "💡 To add blocks to cases: Use the visual workflow designer",
                Location = new Point(8, 95),
                Size = new Size(330, 20),
                ForeColor = System.Drawing.Color.DarkGreen,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold)
            };
            _caseEditPanel.Controls.Add(instructionsLabel);

            this.Controls.Add(_caseEditPanel);
            yOffset += 150;
        }

        private void CreateButtons()
        {
            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(this.Width - 180, this.Height - 70),
                Size = new Size(75, 23)
            };
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);
            
            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(this.Width - 95, this.Height - 70),
                Size = new Size(75, 23)
            };
            this.Controls.Add(cancelButton);
            
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        private void LoadBlockProperties()
        {
            _nameTextBox.Text = _block.Name;
            _enabledCheckBox.Checked = _block.IsEnabled;
            
            switch (_block)
            {
                case MacroBlock macroBlock:
                    LoadMacroBlockProperties(macroBlock);
                    break;
                case ConditionalBlock conditionalBlock:
                    LoadConditionalBlockProperties(conditionalBlock);
                    break;
                case LoopBlock loopBlock:
                    LoadLoopBlockProperties(loopBlock);
                    break;
                case DelayBlock delayBlock:
                    LoadDelayBlockProperties(delayBlock);
                    break;
                case VariableBlock variableBlock:
                    LoadVariableBlockProperties(variableBlock);
                    break;
                case SwitchCaseBlock switchCaseBlock:
                    LoadSwitchCaseBlockProperties(switchCaseBlock);
                    break;
            }
        }

        private void LoadMacroBlockProperties(MacroBlock macroBlock)
        {
            if (!string.IsNullOrEmpty(macroBlock.MacroName))
            {
                var index = _macroComboBox.Items.IndexOf(macroBlock.MacroName);
                if (index >= 0)
                {
                    _macroComboBox.SelectedIndex = index;
                }
            }
            
            _loopCountNumeric.Value = Math.Max(1, Math.Min(10000, macroBlock.LoopCount));
            _infiniteLoopCheckBox.Checked = macroBlock.InfiniteLoop;
            _speedTrackBar.Value = Math.Max(1, Math.Min(20, (int)(macroBlock.SpeedFactor * 10)));
            SpeedTrackBar_ValueChanged(null, null);
        }

        private void LoadConditionalBlockProperties(ConditionalBlock conditionalBlock)
        {
            // Left source
            var leftSourceString = conditionalBlock.LeftSource.ToString();
            for (int i = 0; i < _leftSourceComboBox.Items.Count; i++)
            {
                if (_leftSourceComboBox.Items[i].ToString() == leftSourceString)
                {
                    _leftSourceComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            // Condition type
            var conditionTypeString = conditionalBlock.ConditionType.ToString();
            for (int i = 0; i < _conditionTypeComboBox.Items.Count; i++)
            {
                if (_conditionTypeComboBox.Items[i].ToString() == conditionTypeString)
                {
                    _conditionTypeComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            // Right source
            var rightSourceString = conditionalBlock.RightSource.ToString();
            for (int i = 0; i < _rightSourceComboBox.Items.Count; i++)
            {
                if (_rightSourceComboBox.Items[i].ToString() == rightSourceString)
                {
                    _rightSourceComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            _caseSensitiveCheckBox.Checked = conditionalBlock.CaseSensitive;
            
            // Trigger the event handlers to set up the UI controls
            LeftSourceComboBox_SelectedIndexChanged(null, null);
            RightSourceComboBox_SelectedIndexChanged(null, null);
            
            // AFTER setting up the UI, set the actual values
            // Set left value in appropriate control
            if (conditionalBlock.LeftSource == VariableSource.Variable)
            {
                _leftVariableComboBox.Text = conditionalBlock.LeftValue ?? "";
            }
            else if (conditionalBlock.LeftSource == VariableSource.Value)
            {
                _leftValueTextBox.Text = conditionalBlock.LeftStaticValue ?? "";
            }
            else
            {
                _leftValueTextBox.Text = conditionalBlock.LeftValue ?? "";
            }
            
            // Set right value in appropriate control
            if (conditionalBlock.RightSource == VariableSource.Variable)
            {
                _rightVariableComboBox.Text = conditionalBlock.RightValue ?? "";
            }
            else if (conditionalBlock.RightSource == VariableSource.Value)
            {
                _rightValueTextBox.Text = conditionalBlock.RightStaticValue ?? "";
            }
            else
            {
                _rightValueTextBox.Text = conditionalBlock.RightValue ?? "";
            }
        }

        private void LoadLoopBlockProperties(LoopBlock loopBlock)
        {
            // Loop type
            var loopTypeString = loopBlock.LoopType.ToString();
            for (int i = 0; i < _loopTypeComboBox.Items.Count; i++)
            {
                if (_loopTypeComboBox.Items[i].ToString() == loopTypeString)
                {
                    _loopTypeComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            _loopCountNumeric2.Value = Math.Max(1, Math.Min(10000, loopBlock.Count));
            
            var duration = loopBlock.Duration;
            _durationHoursNumeric.Value = duration.Hours;
            _durationMinutesNumeric.Value = duration.Minutes;
            _durationSecondsNumeric.Value = duration.Seconds;
            
            // Exit condition
            var exitConditionString = loopBlock.ExitCondition.ToString();
            for (int i = 0; i < _exitConditionComboBox.Items.Count; i++)
            {
                if (_exitConditionComboBox.Items[i].ToString() == exitConditionString)
                {
                    _exitConditionComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            _exitConditionValueTextBox.Text = loopBlock.ExitConditionValue ?? "";
            
            LoopTypeComboBox_SelectedIndexChanged(null, null);
        }

        private void LoadDelayBlockProperties(DelayBlock delayBlock)
        {
            _delaySecondsNumeric.Value = (decimal)Math.Max(0.1, Math.Min(3600, delayBlock.DelaySeconds));
            _randomDelayCheckBox.Checked = delayBlock.RandomDelay;
            _minDelayNumeric.Value = (decimal)Math.Max(0.1, Math.Min(3600, delayBlock.MinDelaySeconds));
            _maxDelayNumeric.Value = (decimal)Math.Max(0.1, Math.Min(3600, delayBlock.MaxDelaySeconds));
            RandomDelayCheckBox_CheckedChanged(null, null);
        }

        private void LoadVariableBlockProperties(VariableBlock variableBlock)
        {
            _variableNameTextBox.Text = variableBlock.VariableName ?? "";
            
            // Sync variable name with block name
            _nameTextBox.Text = variableBlock.VariableName ?? "";
            
            // Find and set the correct source by looking for matching enum string
            var sourceString = variableBlock.Source.ToString();
            for (int i = 0; i < _variableSourceComboBox.Items.Count; i++)
            {
                if (_variableSourceComboBox.Items[i].ToString() == sourceString)
                {
                    _variableSourceComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            // Set the source value in appropriate control
            if (variableBlock.Source == VariableSource.Variable)
            {
                _variableSourceComboBox2.Text = variableBlock.SourceValue ?? "";
            }
            else if (variableBlock.Source == VariableSource.Value)
            {
                _directValueTextBox.Text = variableBlock.DirectValue ?? "";
            }
            else
            {
                _variableSourceValueTextBox.Text = variableBlock.SourceValue ?? "";
            }
            
            VariableSourceComboBox_SelectedIndexChanged(null, null);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                this.DialogResult = DialogResult.None;
                return;
            }
            
            SaveBlockProperties();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for the block.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            if (_block is DelayBlock && _randomDelayCheckBox.Checked)
            {
                if (_minDelayNumeric.Value >= _maxDelayNumeric.Value)
                {
                    MessageBox.Show("Minimum delay must be less than maximum delay.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            
            return true;
        }

        private void SaveBlockProperties()
        {
            _block.Name = _nameTextBox.Text.Trim();
            _block.IsEnabled = _enabledCheckBox.Checked;
            
            switch (_block)
            {
                case MacroBlock macroBlock:
                    SaveMacroBlockProperties(macroBlock);
                    break;
                case ConditionalBlock conditionalBlock:
                    SaveConditionalBlockProperties(conditionalBlock);
                    break;
                case LoopBlock loopBlock:
                    SaveLoopBlockProperties(loopBlock);
                    break;
                case DelayBlock delayBlock:
                    SaveDelayBlockProperties(delayBlock);
                    break;
                case VariableBlock variableBlock:
                    SaveVariableBlockProperties(variableBlock);
                    break;
                case SwitchCaseBlock switchCaseBlock:
                    SaveSwitchCaseBlockProperties(switchCaseBlock);
                    break;
            }
        }

        private void SaveMacroBlockProperties(MacroBlock macroBlock)
        {
            if (_macroComboBox.SelectedIndex > 0)
            {
                macroBlock.MacroName = _macroComboBox.SelectedItem.ToString();
                var selectedMacro = _availableMacros.FirstOrDefault(m => m.Name == macroBlock.MacroName);
                if (selectedMacro != null)
                {
                    macroBlock.Actions = new List<RecordedAction>(selectedMacro.Actions);
                }
            }
            
            macroBlock.LoopCount = (int)_loopCountNumeric.Value;
            macroBlock.InfiniteLoop = _infiniteLoopCheckBox.Checked;
            macroBlock.SpeedFactor = _speedTrackBar.Value / 10.0;
        }

        private void SaveConditionalBlockProperties(ConditionalBlock conditionalBlock)
        {
            if (Enum.TryParse<VariableSource>(_leftSourceComboBox.SelectedItem?.ToString(), out var leftSource))
            {
                conditionalBlock.LeftSource = leftSource;
            }
            
            // Handle left value - simple and direct
            if (leftSource == VariableSource.Variable && _leftVariableComboBox.Visible)
            {
                var leftVariableValue = _leftVariableComboBox.Text ?? "";
                if (leftVariableValue == PLACEHOLDER_TYPE_VARIABLE)
                {
                    leftVariableValue = "";
                }
                conditionalBlock.LeftValue = leftVariableValue;
                conditionalBlock.LeftStaticValue = null;
            }
            else if (leftSource == VariableSource.Value)
            {
                var leftValue = _leftValueTextBox.Text ?? "";
                if (IsUIPlaceholderText(leftValue))
                {
                    leftValue = "";
                }
                conditionalBlock.LeftStaticValue = leftValue;
                conditionalBlock.LeftValue = "";
                System.Diagnostics.Debug.WriteLine($"Conditional block save: Left static value = '{leftValue}'");
            }
            else
            {
                var leftValue = _leftValueTextBox.Text ?? "";
                if (IsUIPlaceholderText(leftValue))
                {
                    leftValue = "";
                }
                conditionalBlock.LeftValue = leftValue;
                conditionalBlock.LeftStaticValue = null;
            }
            
            if (Enum.TryParse<ConditionType>(_conditionTypeComboBox.SelectedItem?.ToString(), out var conditionType))
            {
                conditionalBlock.ConditionType = conditionType;
            }
            
            if (Enum.TryParse<VariableSource>(_rightSourceComboBox.SelectedItem?.ToString(), out var rightSource))
            {
                conditionalBlock.RightSource = rightSource;
            }
            
            // Handle right value - simple and direct
            if (rightSource == VariableSource.Variable && _rightVariableComboBox.Visible)
            {
                var rightVariableValue = _rightVariableComboBox.Text ?? "";
                if (rightVariableValue == PLACEHOLDER_TYPE_VARIABLE)
                {
                    rightVariableValue = "";
                }
                conditionalBlock.RightValue = rightVariableValue;
                conditionalBlock.RightStaticValue = null;
            }
            else if (rightSource == VariableSource.Value)
            {
                var rightValue = _rightValueTextBox.Text ?? "";
                if (IsUIPlaceholderText(rightValue))
                {
                    rightValue = "";
                }
                conditionalBlock.RightStaticValue = rightValue;
                conditionalBlock.RightValue = "";
                System.Diagnostics.Debug.WriteLine($"Conditional block save: Right static value = '{rightValue}'");
            }
            else
            {
                var rightValue = _rightValueTextBox.Text ?? "";
                if (IsUIPlaceholderText(rightValue))
                {
                    rightValue = "";
                }
                conditionalBlock.RightValue = rightValue;
                conditionalBlock.RightStaticValue = null;
            }
            
            conditionalBlock.CaseSensitive = _caseSensitiveCheckBox.Checked;
        }

        private void SaveLoopBlockProperties(LoopBlock loopBlock)
        {
            if (Enum.TryParse<LoopType>(_loopTypeComboBox.SelectedItem?.ToString(), out var loopType))
            {
                loopBlock.LoopType = loopType;
            }
            loopBlock.Count = (int)_loopCountNumeric2.Value;
            loopBlock.Duration = new TimeSpan((int)_durationHoursNumeric.Value, (int)_durationMinutesNumeric.Value, (int)_durationSecondsNumeric.Value);
            
            if (Enum.TryParse<ConditionType>(_exitConditionComboBox.SelectedItem?.ToString(), out var exitCondition))
            {
                loopBlock.ExitCondition = exitCondition;
            }
            loopBlock.ExitConditionValue = _exitConditionValueTextBox.Text ?? "";
        }

        private void SaveDelayBlockProperties(DelayBlock delayBlock)
        {
            delayBlock.DelaySeconds = (double)_delaySecondsNumeric.Value;
            delayBlock.RandomDelay = _randomDelayCheckBox.Checked;
            delayBlock.MinDelaySeconds = (double)_minDelayNumeric.Value;
            delayBlock.MaxDelaySeconds = (double)_maxDelayNumeric.Value;
        }

        private void SaveVariableBlockProperties(VariableBlock variableBlock)
        {
            // Keep variable name and block name in sync
            var variableName = _variableNameTextBox.Text ?? "";
            variableBlock.VariableName = variableName;
            variableBlock.Name = variableName; // Same name for both
            
            if (Enum.TryParse<VariableSource>(_variableSourceComboBox.SelectedItem?.ToString(), out var source))
            {
                variableBlock.Source = source;
            }
            
            // Clear all source values first
            variableBlock.SourceValue = "";
            variableBlock.DirectValue = "";
            
            // Set the appropriate source value based on the selected source
            switch (source)
            {
                case VariableSource.Variable:
                    var variableValue = _variableSourceComboBox2.Text ?? "";
                    if (variableValue != PLACEHOLDER_TYPE_VARIABLE)
                    {
                        variableBlock.SourceValue = variableValue;
                    }
                    System.Diagnostics.Debug.WriteLine($"Variable block save: Variable source = '{variableBlock.SourceValue}'");
                    break;
                    
                case VariableSource.Value:
                    variableBlock.DirectValue = _directValueTextBox.Text ?? "";
                    System.Diagnostics.Debug.WriteLine($"Variable block save: Direct value = '{variableBlock.DirectValue}'");
                    break;
                    
                case VariableSource.Clipboard:
                case VariableSource.WindowTitle:
                case VariableSource.LastActiveWindow:
                    // These don't need source values
                    System.Diagnostics.Debug.WriteLine($"Variable block save: {source} source (no additional value needed)");
                    break;
            }
        }

        private void SpeedTrackBar_ValueChanged(object sender, EventArgs e)
        {
            double speedFactor = _speedTrackBar.Value / 10.0;
            _speedLabel.Text = $"{speedFactor:F1}x";
        }

        private void RandomDelayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool isRandom = _randomDelayCheckBox.Checked;
            _minDelayNumeric.Enabled = isRandom;
            _maxDelayNumeric.Enabled = isRandom;
            _delaySecondsNumeric.Enabled = !isRandom;
        }

        private void LoopTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Enum.TryParse<LoopType>(_loopTypeComboBox.SelectedItem?.ToString(), out var loopType))
            {
                _loopCountNumeric2.Enabled = (loopType == LoopType.Count);
                _durationHoursNumeric.Enabled = (loopType == LoopType.Duration);
                _durationMinutesNumeric.Enabled = (loopType == LoopType.Duration);
                _durationSecondsNumeric.Enabled = (loopType == LoopType.Duration);
                _exitConditionComboBox.Enabled = (loopType == LoopType.UntilCondition);
                _exitConditionValueTextBox.Enabled = (loopType == LoopType.UntilCondition);
            }
        }

        private void VariableSourceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Enum.TryParse<VariableSource>(_variableSourceComboBox.SelectedItem?.ToString(), out var source))
            {
                // Hide all controls first
                _variableSourceValueTextBox.Visible = false;
                _variableSourceComboBox2.Visible = false;
                _variableSelectWindowButton.Visible = false;
                _directValueTextBox.Visible = false;
                _setValueButton.Visible = false;
                
                switch (source)
                {
                    case VariableSource.Clipboard:
                        _variableSourceValueTextBox.Visible = true;
                        _variableSourceValueTextBox.Text = PLACEHOLDER_CLIPBOARD;
                        break;
                    case VariableSource.WindowTitle:
                        _variableSourceValueTextBox.Visible = true;
                        _variableSelectWindowButton.Visible = true;
                        _variableSourceValueTextBox.Enabled = true;
                        if (string.IsNullOrEmpty(_variableSourceValueTextBox.Text) || IsAnyPlaceholderText(_variableSourceValueTextBox.Text))
                        {
                            _variableSourceValueTextBox.Text = "";
                        }
                        break;
                    case VariableSource.Variable:
                        _variableSourceComboBox2.Visible = true;
                        break;
                    case VariableSource.Value:
                        _directValueTextBox.Visible = true;
                        _setValueButton.Visible = true;
                        break;
                    case VariableSource.LastActiveWindow:
                        // No additional controls needed - just shows placeholder text
                        _variableSourceValueTextBox.Visible = true;
                        _variableSourceValueTextBox.Text = PLACEHOLDER_LAST_ACTIVE_WINDOW;
                        _variableSourceValueTextBox.Enabled = false;
                        break;
                }
            }
        }

        private void LeftSourceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Enum.TryParse<VariableSource>(_leftSourceComboBox.SelectedItem?.ToString(), out var source))
            {
                switch (source)
                {
                    case VariableSource.Clipboard:
                        _leftValueTextBox.Visible = true;
                        _leftVariableComboBox.Visible = false;
                        _leftSelectWindowButton.Visible = false;
                        _leftValueTextBox.Enabled = false;
                        _leftValueTextBox.Text = PLACEHOLDER_CLIPBOARD;
                        break;
                    case VariableSource.WindowTitle:
                        _leftValueTextBox.Visible = true;
                        _leftVariableComboBox.Visible = false;
                        _leftSelectWindowButton.Visible = true;
                        _leftValueTextBox.Enabled = true;
                        if (string.IsNullOrEmpty(_leftValueTextBox.Text) || IsAnyPlaceholderText(_leftValueTextBox.Text))
                        {
                            _leftValueTextBox.Text = "";
                        }
                        break;
                    case VariableSource.Variable:
                        _leftValueTextBox.Visible = false;
                        _leftVariableComboBox.Visible = true;
                        _leftSelectWindowButton.Visible = false;
                        break;
                    case VariableSource.Value:
                        _leftValueTextBox.Visible = true;
                        _leftVariableComboBox.Visible = false;
                        _leftSelectWindowButton.Visible = false;
                        _leftValueTextBox.Enabled = true;
                        // Only set placeholder if the textbox is currently empty or has placeholder text
                        if (string.IsNullOrEmpty(_leftValueTextBox.Text) || IsAnyPlaceholderText(_leftValueTextBox.Text))
                        {
                            _leftValueTextBox.Text = PLACEHOLDER_STATIC_VALUE;
                        }
                        break;
                    case VariableSource.LastActiveWindow:
                        _leftValueTextBox.Visible = true;
                        _leftVariableComboBox.Visible = false;
                        _leftSelectWindowButton.Visible = false;
                        _leftValueTextBox.Enabled = false;
                        _leftValueTextBox.Text = PLACEHOLDER_LAST_ACTIVE_WINDOW;
                        break;
                }
            }
        }

        private void RightSourceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Enum.TryParse<VariableSource>(_rightSourceComboBox.SelectedItem?.ToString(), out var source))
            {
                switch (source)
                {
                    case VariableSource.Clipboard:
                        _rightValueTextBox.Visible = true;
                        _rightVariableComboBox.Visible = false;
                        _rightSelectWindowButton.Visible = false;
                        _rightValueTextBox.Enabled = false;
                        _rightValueTextBox.Text = PLACEHOLDER_CLIPBOARD;
                        break;
                    case VariableSource.WindowTitle:
                        _rightValueTextBox.Visible = true;
                        _rightVariableComboBox.Visible = false;
                        _rightSelectWindowButton.Visible = true;
                        _rightValueTextBox.Enabled = true;
                        if (string.IsNullOrEmpty(_rightValueTextBox.Text) || IsAnyPlaceholderText(_rightValueTextBox.Text))
                        {
                            _rightValueTextBox.Text = "";
                        }
                        break;
                    case VariableSource.Variable:
                        _rightValueTextBox.Visible = false;
                        _rightVariableComboBox.Visible = true;
                        _rightSelectWindowButton.Visible = false;
                        break;
                    case VariableSource.Value:
                        _rightValueTextBox.Visible = true;
                        _rightVariableComboBox.Visible = false;
                        _rightSelectWindowButton.Visible = false;
                        _rightValueTextBox.Enabled = true;
                        // Only set placeholder if the textbox is currently empty or has placeholder text
                        if (string.IsNullOrEmpty(_rightValueTextBox.Text) || IsAnyPlaceholderText(_rightValueTextBox.Text))
                        {
                            _rightValueTextBox.Text = PLACEHOLDER_STATIC_VALUE;
                        }
                        break;
                    case VariableSource.LastActiveWindow:
                        _rightValueTextBox.Visible = true;
                        _rightVariableComboBox.Visible = false;
                        _rightSelectWindowButton.Visible = false;
                        _rightValueTextBox.Enabled = false;
                        _rightValueTextBox.Text = PLACEHOLDER_LAST_ACTIVE_WINDOW;
                        break;
                }
            }
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // BlockPropertiesForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(453, 577);
            Margin = new Padding(4, 3, 4, 3);
            Name = "BlockPropertiesForm";
            Text = "Block Properties";
            ResumeLayout(false);
        }

        private List<string> GetAvailableVariables()
        {
            List<string> variables = [];
            
            if (_workflow != null)
            {
                // Get all variable names from VariableBlocks in the workflow
                foreach (var block in _workflow.Blocks)
                {
                    if (block is VariableBlock varBlock && !string.IsNullOrEmpty(varBlock.VariableName))
                    {
                        if (!variables.Contains(varBlock.VariableName))
                        {
                            variables.Add(varBlock.VariableName);
                        }
                    }
                }
                
                // Also include regular workflow variables, but exclude auto-generated ones
                foreach (var workflowVar in _workflow.Variables)
                {
                    // Skip auto-generated static value variables
                    if (!workflowVar.Key.StartsWith("StaticValue_") && !variables.Contains(workflowVar.Key))
                    {
                        variables.Add(workflowVar.Key);
                    }
                }
            }
            
            variables.Sort(); // Sort alphabetically for better UX
            return variables;
        }
        
        private void PopulateVariableComboBox(ComboBox comboBox)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add(PLACEHOLDER_TYPE_VARIABLE);
            
            var availableVariables = GetAvailableVariables();
            foreach (var variable in availableVariables)
            {
                comboBox.Items.Add(variable);
            }
            
            if (comboBox.Items.Count > 1)
            {
                comboBox.SelectedIndex = 0;
            }
        }
        
        private void PopulateMacroComboBox(ComboBox comboBox)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add("(No Macro)");
            
            if (_availableMacros != null && _availableMacros.Count > 0)
            {
                foreach (var macro in _availableMacros)
                {
                    if (!string.IsNullOrEmpty(macro.Name))
                    {
                        comboBox.Items.Add(macro.Name);
                    }
                }
            }
            
            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }
        
        private void SetLeftSourceComboBox(VariableSource source)
        {
            var sourceString = source.ToString();
            for (int i = 0; i < _leftSourceComboBox.Items.Count; i++)
            {
                if (_leftSourceComboBox.Items[i].ToString() == sourceString)
                {
                    _leftSourceComboBox.SelectedIndex = i;
                    break;
                }
            }
        }
        
        private void SetRightSourceComboBox(VariableSource source)
        {
            var sourceString = source.ToString();
            for (int i = 0; i < _rightSourceComboBox.Items.Count; i++)
            {
                if (_rightSourceComboBox.Items[i].ToString() == sourceString)
                {
                    _rightSourceComboBox.SelectedIndex = i;
                    break;
                }
            }
        }
        
        // Set Value button click handler
        private void SetValueButton_Click(object sender, EventArgs e)
        {
            var value = _directValueTextBox.Text ?? "";
            if (string.IsNullOrEmpty(value))
            {
                MessageBox.Show("Please enter a value before setting it.", "No Value", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Store the value directly in the block
            if (_block is VariableBlock variableBlock)
            {
                variableBlock.DirectValue = value;
                System.Diagnostics.Debug.WriteLine($"Set direct value '{value}' for variable '{variableBlock.VariableName}'");
                
                MessageBox.Show($"Value '{value}' has been set for variable '{variableBlock.VariableName}'.", "Value Set", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        // Event handlers for placeholder text management
        private void TextBox_Enter(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && IsAnyPlaceholderText(textBox.Text))
            {
                textBox.Text = "";
                textBox.ForeColor = SystemColors.WindowText;
            }
        }
        
        private void TextBox_Leave(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrEmpty(textBox.Text))
            {
                // Determine appropriate placeholder based on context
                if (textBox == _leftValueTextBox)
                {
                    if (Enum.TryParse<VariableSource>(_leftSourceComboBox.SelectedItem?.ToString(), out var leftSource))
                    {
                        switch (leftSource)
                        {
                            case VariableSource.Value:
                                textBox.Text = PLACEHOLDER_STATIC_VALUE;
                                textBox.ForeColor = SystemColors.GrayText;
                                break;
                        }
                    }
                }
                else if (textBox == _rightValueTextBox)
                {
                    if (Enum.TryParse<VariableSource>(_rightSourceComboBox.SelectedItem?.ToString(), out var rightSource))
                    {
                        switch (rightSource)
                        {
                            case VariableSource.Value:
                                textBox.Text = PLACEHOLDER_STATIC_VALUE;
                                textBox.ForeColor = SystemColors.GrayText;
                                break;
                        }
                    }
                }
                else if (textBox == _variableSourceValueTextBox)
                {
                    if (Enum.TryParse<VariableSource>(_variableSourceComboBox.SelectedItem?.ToString(), out var source))
                    {
                        switch (source)
                        {
                            case VariableSource.Value:
                                textBox.Text = PLACEHOLDER_STATIC_VALUE;
                                textBox.ForeColor = SystemColors.GrayText;
                                break;
                        }
                    }
                }
            }
        }
        
        private void VariableSourceValueTextBox_Enter(object sender, EventArgs e)
        {
            TextBox_Enter(sender, e);
        }
        
        private void VariableSourceValueTextBox_Leave(object sender, EventArgs e)
        {
            TextBox_Leave(sender, e);
        }

        private void LoadSwitchCaseBlockProperties(SwitchCaseBlock switchCaseBlock)
        {
            if (_switchSourceComboBox == null || _switchCaseSensitiveCheckBox == null || 
                _casesListBox == null) return;
            
            // Switch source
            var switchSourceString = switchCaseBlock.SwitchSource.ToString();
            for (int i = 0; i < _switchSourceComboBox.Items.Count; i++)
            {
                if (_switchSourceComboBox.Items[i].ToString() == switchSourceString)
                {
                    _switchSourceComboBox.SelectedIndex = i;
                    break;
                }
            }

            _switchCaseSensitiveCheckBox.Checked = switchCaseBlock.CaseSensitive;

            // Load initial macro
            if (_initialMacroComboBox != null && !string.IsNullOrEmpty(switchCaseBlock.InitialMacroName))
            {
                var macroIndex = _initialMacroComboBox.Items.IndexOf(switchCaseBlock.InitialMacroName);
                if (macroIndex >= 0)
                {
                    _initialMacroComboBox.SelectedIndex = macroIndex;
                }
            }

            // Load cases
            System.Diagnostics.Debug.WriteLine($"=== LoadSwitchCaseBlockProperties - Loading {switchCaseBlock.Cases.Count} cases ===");
            foreach (var switchCase in switchCaseBlock.Cases)
            {
                System.Diagnostics.Debug.WriteLine($"Loading case: ID={switchCase.Id}, Value='{switchCase.CaseValue}', IsDefault={switchCase.IsDefault}, Macro='{switchCase.MacroName}'");
                _casesListBox.Items.Add(switchCase);
            }

            // Update UI based on switch source
            UpdateSwitchCaseUI(switchCaseBlock.SwitchSource);
            UpdateCasesDisplay();
        }

        private void SaveSwitchCaseBlockProperties(SwitchCaseBlock switchCaseBlock)
        {
            if (_switchSourceComboBox == null || _switchCaseSensitiveCheckBox == null || _casesListBox == null) return;
            
            // Save the currently selected case before saving the entire block
            if (_previousSelectedCase != null)
            {
                System.Diagnostics.Debug.WriteLine($"Final save of current case: {_previousSelectedCase.Id}");
                SaveCaseFromEditor(_previousSelectedCase);
            }
            else if (_casesListBox.SelectedItem is SwitchCase selectedCase)
            {
                System.Diagnostics.Debug.WriteLine($"Final save of selected case: {selectedCase.Id}");
                SaveCaseFromEditor(selectedCase);
            }
            
            if (Enum.TryParse<SwitchSourceType>(_switchSourceComboBox.SelectedItem?.ToString(), out var switchSource))
            {
                switchCaseBlock.SwitchSource = switchSource;
            }

            switchCaseBlock.CaseSensitive = _switchCaseSensitiveCheckBox.Checked;

            // Save initial macro
            if (_initialMacroComboBox != null && _initialMacroComboBox.SelectedIndex > 0)
            {
                switchCaseBlock.InitialMacroName = _initialMacroComboBox.SelectedItem?.ToString() ?? "";
                var selectedMacro = _availableMacros?.FirstOrDefault(m => m.Name == switchCaseBlock.InitialMacroName);
                if (selectedMacro != null)
                {
                    switchCaseBlock.InitialMacroActions = new List<RecordedAction>(selectedMacro.Actions);
                }
            }
            else
            {
                switchCaseBlock.InitialMacroName = "";
                switchCaseBlock.InitialMacroActions.Clear();
            }

            // Save cases
            System.Diagnostics.Debug.WriteLine($"=== SaveSwitchCaseBlockProperties - Saving {_casesListBox.Items.Count} cases ===");
            switchCaseBlock.Cases.Clear();
            foreach (SwitchCase switchCase in _casesListBox.Items)
            {
                System.Diagnostics.Debug.WriteLine($"Saving case: ID={switchCase.Id}, Value='{switchCase.CaseValue}', IsDefault={switchCase.IsDefault}, Macro='{switchCase.MacroName}'");
                switchCaseBlock.Cases.Add(switchCase);
            }
            System.Diagnostics.Debug.WriteLine($"Total cases saved: {switchCaseBlock.Cases.Count}");
        }


        private SwitchCase? _previousSelectedCase = null;

        private void CasesListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_casesListBox == null || _removeCaseButton == null || _caseEditPanel == null || _updatingCasesDisplay) return;
            
            System.Diagnostics.Debug.WriteLine($"=== CasesListBox_SelectedIndexChanged ===\nPrevious case: {_previousSelectedCase?.Id}\nNew selection: {_casesListBox.SelectedItem?.GetType().Name}");
            
            // Save previous case if there was one
            if (_previousSelectedCase != null)
            {
                System.Diagnostics.Debug.WriteLine($"Saving previous case: {_previousSelectedCase.Id}");
                SaveCaseFromEditor(_previousSelectedCase);
            }
            
            bool hasSelection = _casesListBox.SelectedIndex >= 0;
            _removeCaseButton.Enabled = hasSelection;
            _caseEditPanel.Enabled = hasSelection;

            if (hasSelection && _casesListBox.SelectedItem is SwitchCase selectedCase)
            {
                System.Diagnostics.Debug.WriteLine($"Loading new case: {selectedCase.Id}");
                _previousSelectedCase = selectedCase;
                LoadCaseToEditor(selectedCase);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No case selected");
                _previousSelectedCase = null;
            }
        }

        private void LoadCaseToEditor(SwitchCase switchCase)
        {
            if (_isDefaultCaseCheckBox == null || _caseValueTextBox == null || _caseBlocksInfoLabel == null) return;
            
            System.Diagnostics.Debug.WriteLine($"=== LoadCaseToEditor ===\nCase ID: {switchCase.Id}\nCaseValue: '{switchCase.CaseValue}'\nIsDefault: {switchCase.IsDefault}\nConnected Blocks: {switchCase.CaseBlocks.Count}");
            
            // Temporarily disable event handlers to prevent triggering saves during load
            _isDefaultCaseCheckBox.CheckedChanged -= IsDefaultCaseCheckBox_CheckedChanged;
            _caseValueTextBox.TextChanged -= CaseValueTextBox_TextChanged;
            
            try
            {
                _isDefaultCaseCheckBox.Checked = switchCase.IsDefault;
                _caseValueTextBox.Text = switchCase.CaseValue ?? "";

                // Display connected blocks information
                if (switchCase.CaseBlocks.Count > 0)
                {
                    string blocksInfo = $"{switchCase.CaseBlocks.Count} block(s) connected";
                    _caseBlocksInfoLabel.Text = blocksInfo;
                    _caseBlocksInfoLabel.ForeColor = System.Drawing.Color.Black;
                    _caseBlocksInfoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
                }
                else
                {
                    // Check if has legacy macro for backward compatibility
                    if (!string.IsNullOrEmpty(switchCase.MacroName))
                    {
                        _caseBlocksInfoLabel.Text = $"Legacy macro: {switchCase.MacroName}";
                        _caseBlocksInfoLabel.ForeColor = System.Drawing.Color.DarkBlue;
                        _caseBlocksInfoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic);
                    }
                    else
                    {
                        _caseBlocksInfoLabel.Text = "(No blocks connected - use visual designer)";
                        _caseBlocksInfoLabel.ForeColor = System.Drawing.Color.Gray;
                        _caseBlocksInfoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic);
                    }
                }
            }
            finally
            {
                // Re-enable event handlers
                _isDefaultCaseCheckBox.CheckedChanged += IsDefaultCaseCheckBox_CheckedChanged;
                _caseValueTextBox.TextChanged += CaseValueTextBox_TextChanged;
            }
        }

        private void SaveCaseFromEditor(SwitchCase switchCase)
        {
            if (_caseValueTextBox == null || _isDefaultCaseCheckBox == null) return;
            
            System.Diagnostics.Debug.WriteLine($"=== SaveCaseFromEditor (Case ID: {switchCase.Id}) ===\nOld: Value='{switchCase.CaseValue}', Default={switchCase.IsDefault}, Connected Blocks={switchCase.CaseBlocks.Count}");
            
            // Save case value (simplified - just static value)
            var caseValue = _caseValueTextBox.Text ?? "";
            if (IsUIPlaceholderText(caseValue))
            {
                caseValue = "";
            }
            switchCase.CaseValue = caseValue;
            switchCase.IsDefault = _isDefaultCaseCheckBox.Checked;

            // Note: CaseBlocks are managed by the visual designer, not this form
            // Legacy macro properties are preserved for backward compatibility
            
            System.Diagnostics.Debug.WriteLine($"New: Value='{switchCase.CaseValue}', Default={switchCase.IsDefault}, Macro='{switchCase.MacroName}' ({switchCase.MacroActions.Count} actions)");
        }


        private void AddCaseButton_Click(object? sender, EventArgs e)
        {
            if (_casesListBox == null) return;
            
            var newCase = new SwitchCase
            {
                CaseValue = "New Case",
                IsDefault = false
            };

            _casesListBox.Items.Add(newCase);
            _casesListBox.SelectedItem = newCase;
            UpdateCasesDisplay();
        }

        private void AddDefaultCaseButton_Click(object? sender, EventArgs e)
        {
            if (_casesListBox == null) return;
            
            // Check if default case already exists
            foreach (SwitchCase existingCase in _casesListBox.Items)
            {
                if (existingCase.IsDefault)
                {
                    MessageBox.Show("A default case already exists.", "Default Case Exists", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            var defaultCase = new SwitchCase
            {
                CaseValue = "",
                IsDefault = true
            };

            _casesListBox.Items.Add(defaultCase);
            _casesListBox.SelectedItem = defaultCase;
            UpdateCasesDisplay();
        }

        private void RemoveCaseButton_Click(object? sender, EventArgs e)
        {
            if (_casesListBox?.SelectedItem is SwitchCase selectedCase)
            {
                _casesListBox.Items.Remove(selectedCase);
                UpdateCasesDisplay();
            }
        }

        private void CaseValueTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_updatingCasesDisplay) return;
            if (_casesListBox?.SelectedItem is SwitchCase selectedCase)
            {
                var oldDisplayText = selectedCase.ToString();
                SaveCaseFromEditor(selectedCase);
                var newDisplayText = selectedCase.ToString();
                
                // Only update display if the text representation changed
                if (oldDisplayText != newDisplayText)
                {
                    UpdateCasesDisplay();
                }
            }
        }

        private void CaseVariableComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // This method is no longer used since we simplified the switch case system
            // Cases now only use static values, not variable sources
        }

        private void IsDefaultCaseCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_updatingCasesDisplay) return;
            if (_casesListBox?.SelectedItem is SwitchCase selectedCase && _isDefaultCaseCheckBox != null)
            {
                if (_isDefaultCaseCheckBox.Checked)
                {
                    // Uncheck any existing default cases
                    foreach (SwitchCase existingCase in _casesListBox.Items)
                    {
                        if (existingCase != selectedCase && existingCase.IsDefault)
                        {
                            existingCase.IsDefault = false;
                        }
                    }
                }

                var oldDisplayText = selectedCase.ToString();
                SaveCaseFromEditor(selectedCase);
                var newDisplayText = selectedCase.ToString();
                
                // Always update display when default status changes since it affects button states
                UpdateCasesDisplay();
            }
        }

        // Legacy method - no longer needed as cases now use CaseBlocks instead of single macro
        // private void CaseMacroComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        // {
        //     if (_updatingCasesDisplay) return;
        //     if (_casesListBox?.SelectedItem is SwitchCase selectedCase)
        //     {
        //         SaveCaseFromEditor(selectedCase);
        //     }
        // }

        private bool _updatingCasesDisplay = false;

        private void UpdateCasesDisplay()
        {
            if (_casesListBox == null || _addDefaultCaseButton == null || _updatingCasesDisplay) return;
            
            _updatingCasesDisplay = true;
            try
            {
                // Update button states
                bool hasDefault = false;
                foreach (SwitchCase switchCase in _casesListBox.Items)
                {
                    if (switchCase.IsDefault)
                    {
                        hasDefault = true;
                        break;
                    }
                }
                _addDefaultCaseButton.Enabled = !hasDefault;
                
                // Force refresh of ListBox items display
                // The ListBox doesn't automatically update when object properties change
                var selectedIndex = _casesListBox.SelectedIndex;
                var selectedItem = _casesListBox.SelectedItem;
                
                // Temporarily disable event handlers to prevent recursion
                _casesListBox.SelectedIndexChanged -= CasesListBox_SelectedIndexChanged;
                
                // Recreate the items to force refresh
                List<SwitchCase> items = [];
                foreach (SwitchCase item in _casesListBox.Items)
                {
                    items.Add(item);
                }
                
                _casesListBox.Items.Clear();
                foreach (var item in items)
                {
                    _casesListBox.Items.Add(item);
                }
                
                // Restore selection
                if (selectedItem != null)
                {
                    _casesListBox.SelectedItem = selectedItem;
                }
                
                // Re-enable event handlers
                _casesListBox.SelectedIndexChanged += CasesListBox_SelectedIndexChanged;
            }
            finally
            {
                _updatingCasesDisplay = false;
            }
        }

        private void UpdateSwitchCaseUI(SwitchSourceType switchSource)
        {
            if (_caseSelectWindowButton == null) return;
            
            // Show window selection button only for ActiveWindow source
            bool showWindowSelector = switchSource == SwitchSourceType.ActiveWindow;
            _caseSelectWindowButton.Visible = showWindowSelector;
            
            // Update case value label text based on source
            var caseValueLabel = _caseEditPanel?.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("Match"));
            if (caseValueLabel != null)
            {
                caseValueLabel.Text = switchSource switch
                {
                    SwitchSourceType.Clipboard => "Match Text:",
                    SwitchSourceType.ActiveWindow => "Match Window:",
                    _ => "Match Value:"
                };
            }
        }

        private void SwitchSourceComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_switchSourceComboBox == null) return;
            
            if (Enum.TryParse<SwitchSourceType>(_switchSourceComboBox.SelectedItem?.ToString(), out var switchSource))
            {
                UpdateSwitchCaseUI(switchSource);
            }
        }

        private static string GetCaseDisplayText(SwitchCase switchCase)
        {
            if (switchCase.IsDefault)
            {
                return "Default";
            }

            var value = switchCase.CaseValue ?? "";
            return string.IsNullOrEmpty(value) ? "[Empty]" : value;
        }

        private Button CreateWindowSelectButton(Point location, string tag)
        {
            var button = new Button
            {
                Text = "Select...",
                Location = location,
                Size = new Size(80, 23),
                Visible = false,
                Tag = tag,
                Font = new Font("Segoe UI", 8, FontStyle.Regular)
            };
            button.Click += SelectWindowButton_Click;
            return button;
        }

        private void SelectWindowButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button button) return;

            using var windowSelector = new WindowSelectorForm();
            if (windowSelector.ShowDialog(this) == DialogResult.OK && 
                !string.IsNullOrEmpty(windowSelector.SelectedWindowTitle))
            {
                var tag = button.Tag?.ToString();
                switch (tag)
                {
                    case "left":
                        if (_leftValueTextBox != null)
                            _leftValueTextBox.Text = windowSelector.SelectedWindowTitle;
                        break;
                    case "right":
                        if (_rightValueTextBox != null)
                            _rightValueTextBox.Text = windowSelector.SelectedWindowTitle;
                        break;
                    case "variable":
                        if (_variableSourceValueTextBox != null)
                            _variableSourceValueTextBox.Text = windowSelector.SelectedWindowTitle;
                        break;
                    case "switch":
                        if (_switchValueTextBox != null)
                            _switchValueTextBox.Text = windowSelector.SelectedWindowTitle;
                        break;
                    case "case":
                        if (_caseValueTextBox != null)
                            _caseValueTextBox.Text = windowSelector.SelectedWindowTitle;
                        break;
                }
            }
        }
    }
}