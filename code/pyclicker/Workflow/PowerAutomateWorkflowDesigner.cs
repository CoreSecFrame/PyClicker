using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PyClickerRecorder.Workflow
{
    public partial class PowerAutomateWorkflowDesigner : Form
    {
        private Workflow _currentWorkflow;
        private readonly List<SavedMacro> _availableMacros;
        private Panel _flowPanel = null!;
        private Button? _selectedAddButton;
        
        // Colors matching Power Automate style
        private readonly Color _macroColor = Color.FromArgb(0, 120, 212);
        private readonly Color _conditionalColor = Color.FromArgb(102, 102, 102);
        private readonly Color _switchCaseColor = Color.FromArgb(155, 89, 182);
        private readonly Color _loopColor = Color.FromArgb(16, 124, 16);
        private readonly Color _delayColor = Color.FromArgb(128, 57, 123);
        private readonly Color _variableColor = Color.FromArgb(255, 140, 0);
        private readonly Color _ifYesColor = Color.FromArgb(22, 160, 133);
        private readonly Color _ifNoColor = Color.FromArgb(231, 76, 60);

        public Workflow CurrentWorkflow => _currentWorkflow;

        public PowerAutomateWorkflowDesigner(Workflow? workflow, List<SavedMacro>? availableMacros)
        {
            InitializeComponent();
            _availableMacros = availableMacros ?? new List<SavedMacro>();
            _currentWorkflow = workflow ?? new Workflow();
            SetupDesigner();
            BuildFlowUI();
        }

        private void SetupDesigner()
        {
            this.Text = $"Flow Designer - {_currentWorkflow.Name}";
            this.Size = new Size(900, 700);
            this.BackColor = Color.FromArgb(248, 249, 250);
            
            CreateToolbar();
            CreateFlowPanel();
        }

        private void CreateToolbar()
        {
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White
            };

            var saveButton = new Button
            {
                Text = "Save",
                Location = new Point(10, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            saveButton.Click += SaveFlow_Click;
            toolbar.Controls.Add(saveButton);

            var runButton = new Button
            {
                Text = "Test Flow",
                Location = new Point(100, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(16, 124, 16),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            runButton.Click += RunFlow_Click;
            toolbar.Controls.Add(runButton);

            this.Controls.Add(toolbar);
        }

        private void CreateFlowPanel()
        {
            _flowPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20)
            };

            this.Controls.Add(_flowPanel);
        }

        private void BuildFlowUI()
        {
            System.Diagnostics.Debug.WriteLine($"=== BuildFlowUI starting - Total blocks in workflow: {_currentWorkflow.Blocks.Count} ===");
            foreach (var block in _currentWorkflow.Blocks)
            {
                System.Diagnostics.Debug.WriteLine($"  Block: {block.Name} (ID: {block.Id}, Type: {block.GetType().Name})");
            }
            
            _flowPanel.Controls.Clear();
            
            var container = new Panel
            {
                Width = _flowPanel.Width - 40,
                BackColor = Color.Transparent,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int yPosition = 10;
            
            // Add trigger block (always first)
            var triggerBlock = CreateTriggerBlock();
            triggerBlock.Location = new Point(20, yPosition);
            container.Controls.Add(triggerBlock);
            yPosition += triggerBlock.Height + 10;

            // Add flow blocks
            if (_currentWorkflow.Blocks.Count == 0)
            {
                // Add initial "Add an action" button
                var initialAddButton = CreateAddButton();
                initialAddButton.Location = new Point(20, yPosition);
                container.Controls.Add(initialAddButton);
                yPosition += initialAddButton.Height + 10;
            }
            else
            {
                // Filter out blocks that are contained in loops or conditionals for main flow display
                var mainFlowBlocks = _currentWorkflow.Blocks
                    .Where(block => !block.NextBlocks.Any(nb => 
                        nb == "__LOOP_CONTAINED__" || 
                        nb == "__CONDITIONAL_TRUE_CONTAINED__" || 
                        nb == "__CONDITIONAL_FALSE_CONTAINED__" ||
                        (nb.StartsWith("__SWITCHCASE_") && nb.EndsWith("_CONTAINED__"))))
                    .ToList();
                
                yPosition = BuildBlocksRecursive(container, mainFlowBlocks, yPosition, 20, null);
            }

            container.Height = yPosition;
            _flowPanel.Controls.Add(container);
        }

        private int BuildBlocksRecursive(Panel container, List<WorkflowBlock> blocks, int yPosition, int xOffset, WorkflowBlock? parent, bool suppressAddButtons = false)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                
                // Add connection arrow (except for first block after trigger)
                if (i > 0 || parent != null)
                {
                    var arrow = CreateArrow();
                    arrow.Location = new Point(xOffset + 200, yPosition - 5);
                    container.Controls.Add(arrow);
                    yPosition += 20;
                }

                var blockControl = CreateBlockControl(block);
                blockControl.Location = new Point(xOffset, yPosition);
                container.Controls.Add(blockControl);
                yPosition += blockControl.Height + 10;

                // Handle conditional blocks with nested structure
                if (block is ConditionalBlock conditional)
                {
                    var conditionalContainer = CreateConditionalContainer(conditional);
                    conditionalContainer.Location = new Point(xOffset, yPosition);
                    container.Controls.Add(conditionalContainer);
                    yPosition += conditionalContainer.Height + 10;
                }
                // Handle loop blocks with nested structure
                else if (block is LoopBlock loopBlock)
                {
                    var loopContainer = CreateLoopContainer(loopBlock);
                    loopContainer.Location = new Point(xOffset, yPosition);
                    container.Controls.Add(loopContainer);
                    yPosition += loopContainer.Height + 10;
                }
                // Handle switch case blocks with nested structure
                else if (block is SwitchCaseBlock switchCaseBlock)
                {
                    System.Diagnostics.Debug.WriteLine($"Found SwitchCaseBlock {switchCaseBlock.Id} in BuildBlocksRecursive");
                    var switchContainer = CreateSwitchCaseContainer(switchCaseBlock);
                    switchContainer.Location = new Point(xOffset, yPosition);
                    container.Controls.Add(switchContainer);
                    yPosition += switchContainer.Height + 10;
                    
                    // Add "Add an action" button after switch case blocks if not suppressing buttons
                    if (!suppressAddButtons)
                    {
                        var addButton = CreateAddButton();
                        addButton.Location = new Point(xOffset, yPosition);
                        addButton.Tag = block; // Store reference to parent block for insertion after switch
                        container.Controls.Add(addButton);
                        yPosition += addButton.Height + 10;
                    }
                }
                else if (!suppressAddButtons)
                {
                    // Add "Add an action" button after non-nested blocks
                    var addButton = CreateAddButton();
                    addButton.Location = new Point(xOffset, yPosition);
                    addButton.Tag = block; // Store reference to parent block
                    container.Controls.Add(addButton);
                    yPosition += addButton.Height + 10;
                }
            }

            // Only add final "Add an action" button if there are no blocks and not suppressing buttons
            if (blocks.Count == 0 && !suppressAddButtons)
            {
                var finalAddButton = CreateAddButton();
                finalAddButton.Location = new Point(xOffset, yPosition);
                finalAddButton.Tag = parent; // Reference to parent for insertion
                container.Controls.Add(finalAddButton);
                yPosition += finalAddButton.Height + 10;
            }

            return yPosition;
        }

        private Panel CreateTriggerBlock()
        {
            var panel = new Panel
            {
                Size = new Size(400, 50),
                BackColor = Color.FromArgb(0, 188, 144),
                BorderStyle = BorderStyle.FixedSingle
            };

            var icon = new Label
            {
                Text = "⚡",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 12),
                Size = new Size(30, 25),
                BackColor = Color.Transparent
            };

            var title = new Label
            {
                Text = "When workflow is triggered",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(45, 15),
                Size = new Size(300, 20),
                BackColor = Color.Transparent
            };

            panel.Controls.Add(icon);
            panel.Controls.Add(title);

            return panel;
        }

        private Panel CreateBlockControl(WorkflowBlock block)
        {
            var panel = new Panel
            {
                Size = new Size(400, 60),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            // Add colored left border
            var colorBar = new Panel
            {
                Size = new Size(4, 60),
                Location = new Point(0, 0),
                BackColor = GetBlockColor(block.Type)
            };
            panel.Controls.Add(colorBar);

            // Add icon
            var icon = new Label
            {
                Text = GetBlockIcon(block.Type),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = GetBlockColor(block.Type),
                Location = new Point(15, 15),
                Size = new Size(30, 30),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(icon);

            // Add title - adjust width to not overlap with options button
            var title = new Label
            {
                Text = block.Name,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(55, 10),
                Size = new Size(panel.Width - 100, 20), // Leave space for options button
                BackColor = Color.Transparent
            };
            panel.Controls.Add(title);

            // Add description - adjust width to not overlap with options button
            var description = new Label
            {
                Text = GetBlockDescription(block),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(55, 30),
                Size = new Size(panel.Width - 200, 25), // Leave space for options button
                BackColor = Color.Transparent
            };
            panel.Controls.Add(description);

            // Add options button - positioned relative to panel width
            var optionsButton = new Button
            {
                Text = "⋯",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(30, 25),
                Location = new Point(panel.Width - 40, 17), // Position relative to panel width
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right // Anchor to top-right
            };
            optionsButton.FlatAppearance.BorderSize = 0;
            optionsButton.Click += (s, e) => ShowBlockOptions(block, optionsButton);
            panel.Controls.Add(optionsButton);

            // Double-click to edit
            panel.DoubleClick += (s, e) => EditBlock(block);
            title.DoubleClick += (s, e) => EditBlock(block);
            description.DoubleClick += (s, e) => EditBlock(block);

            return panel;
        }

        private Panel CreateConditionalContainer(ConditionalBlock conditional)
        {
            var container = new Panel
            {
                Width = 800,
                BackColor = Color.Transparent,
                AutoSize = true
            };

            // Create "If yes" section
            var ifYesPanel = CreateConditionalSection("If yes", _ifYesColor, conditional.TrueBlock);
            ifYesPanel.Location = new Point(0, 0);
            container.Controls.Add(ifYesPanel);

            // Create "If no" section  
            var ifNoPanel = CreateConditionalSection("If no", _ifNoColor, conditional.FalseBlock);
            ifNoPanel.Location = new Point(410, 0);
            container.Controls.Add(ifNoPanel);

            container.Height = Math.Max(ifYesPanel.Height, ifNoPanel.Height);
            return container;
        }

        private Panel CreateLoopContainer(LoopBlock loopBlock)
        {
            var container = new Panel
            {
                Width = 800,
                BackColor = Color.Transparent,
                AutoSize = true
            };

            // Create loop body section
            var loopBodyPanel = CreateLoopSection("Loop Body", _loopColor, loopBlock.LoopBlocks, loopBlock.Id);
            loopBodyPanel.Location = new Point(0, 0);
            container.Controls.Add(loopBodyPanel);

            container.Height = loopBodyPanel.Height;
            return container;
        }

        private Panel CreateSwitchCaseContainer(SwitchCaseBlock switchCaseBlock)
        {
            System.Diagnostics.Debug.WriteLine($"=== CreateSwitchCaseContainer called for switch {switchCaseBlock.Id} with {switchCaseBlock.Cases.Count} cases ===");
            foreach (var switchCase in switchCaseBlock.Cases)
            {
                System.Diagnostics.Debug.WriteLine($"  Case: {switchCase.CaseValue} (ID: {switchCase.Id}, IsDefault: {switchCase.IsDefault}, Blocks: {switchCase.CaseBlocks.Count})");
                foreach (var blockId in switchCase.CaseBlocks)
                {
                    var block = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == blockId);
                    System.Diagnostics.Debug.WriteLine($"    Block ID: {blockId} -> Found: {block?.Name ?? "NOT FOUND"}");
                }
            }
            
            var container = new Panel
            {
                Width = 800,
                BackColor = Color.Transparent,
                AutoSize = false  // We'll set size manually for better control
            };

            int yPosition = 0;
            int xOffset = 0;
            const int caseSpacing = 10;

            // Create sections for each case
            var casePanels = new List<Panel>();
            
            // Ensure there's at least a default case for new switch blocks
            if (switchCaseBlock.Cases.Count == 0)
            {
                var defaultCase = new SwitchCase
                {
                    Id = Guid.NewGuid().ToString(),
                    CaseValue = "",
                    IsDefault = true
                };
                switchCaseBlock.Cases.Add(defaultCase);
            }
            
            // Layout all cases with proper row management
            int casesInCurrentRow = 0;
            int maxRowHeight = 0;
            
            for (int i = 0; i < switchCaseBlock.Cases.Count; i++)
            {
                var switchCase = switchCaseBlock.Cases[i];
                var caseTitle = switchCase.IsDefault ? "Default" : $"Case: {switchCase.CaseValue}";
                var casePanel = CreateSwitchCaseSection(caseTitle, _switchCaseColor, switchCase.CaseBlocks, switchCaseBlock.Id, switchCase.Id, switchCase.IsDefault);
                
                casePanel.Location = new Point(xOffset, yPosition);
                container.Controls.Add(casePanel);
                casePanels.Add(casePanel);
                
                // Track the maximum height in this row
                maxRowHeight = Math.Max(maxRowHeight, casePanel.Height);
                
                // Use the actual width of the panel for spacing
                xOffset += casePanel.Width + caseSpacing;
                casesInCurrentRow++;
                
                // Start new row after 3 cases or if we would exceed container width
                if (casesInCurrentRow == 3 && i < switchCaseBlock.Cases.Count - 1)
                {
                    yPosition += maxRowHeight + 20;
                    xOffset = 0;
                    casesInCurrentRow = 0;
                    maxRowHeight = 0;
                }
            }

            // Calculate optimal container size
            if (casePanels.Count > 0)
            {
                var maxRight = casePanels.Max(p => p.Right);
                var maxBottom = casePanels.Max(p => p.Bottom);
                container.Size = new Size(maxRight + 20, maxBottom + 20);
            }
            else
            {
                container.Size = new Size(280, 200); // Default size for empty switch
            }
            
            return container;
        }

        private Panel CreateConditionalSection(string title, Color borderColor, string blockId)
        {
            var panel = new Panel
            {
                Size = new Size(380, 200),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add colored header
            var header = new Panel
            {
                Size = new Size(378, 30),
                Location = new Point(1, 1),
                BackColor = borderColor
            };

            var headerLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 6),
                Size = new Size(300, 18),
                BackColor = Color.Transparent
            };
            header.Controls.Add(headerLabel);
            panel.Controls.Add(header);

            // Add content area
            var contentArea = new Panel
            {
                Location = new Point(10, 40),
                Size = new Size(360, 150),
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            // Find the conditional block to get the correct branch blocks
            var conditional = _currentWorkflow.Blocks.OfType<ConditionalBlock>()
                .FirstOrDefault(c => c.TrueBlock == blockId || c.FalseBlock == blockId || 
                                     c.TrueBlocks.Contains(blockId) || c.FalseBlocks.Contains(blockId));
            
            if (conditional == null)
            {
                // Try to find by the most recent conditional block
                conditional = _currentWorkflow.Blocks.OfType<ConditionalBlock>().LastOrDefault();
            }

            // Add blocks for this branch
            int yPos = 10;
            List<string> blockIds = new List<string>();
            
            if (conditional != null)
            {
                if (title == "If yes")
                {
                    blockIds.AddRange(conditional.TrueBlocks);
                    if (!string.IsNullOrEmpty(conditional.TrueBlock) && !blockIds.Contains(conditional.TrueBlock))
                    {
                        blockIds.Add(conditional.TrueBlock);
                    }
                }
                else if (title == "If no")
                {
                    blockIds.AddRange(conditional.FalseBlocks);
                    if (!string.IsNullOrEmpty(conditional.FalseBlock) && !blockIds.Contains(conditional.FalseBlock))
                    {
                        blockIds.Add(conditional.FalseBlock);
                    }
                }
            }

            foreach (var id in blockIds)
            {
                var block = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == id);
                if (block != null)
                {
                    // Add arrow for non-first blocks
                    if (yPos > 10)
                    {
                        var arrow = CreateArrow();
                        arrow.Location = new Point(175, yPos - 5);
                        contentArea.Controls.Add(arrow);
                        yPos += 20;
                    }

                    var blockControl = CreateBlockControl(block);
                    blockControl.Location = new Point(0, yPos);
                    blockControl.Size = new Size(350, 60); // Normal size for visibility and context menu
                    contentArea.Controls.Add(blockControl);
                    yPos += 70;
                }
            }

            // Add "Add an action" button
            var addButton = CreateAddButton();
            addButton.Location = new Point(0, yPos);
            addButton.Size = new Size(350, 30);
            
            // Store branch info for the conditional block
            var conditionalId = conditional?.Id ?? "";
            addButton.Tag = $"{title}:{conditionalId}";
            contentArea.Controls.Add(addButton);

            // Adjust panel height based on content
            var requiredHeight = Math.Max(200, yPos + 80);
            panel.Size = new Size(380, requiredHeight);
            contentArea.Size = new Size(360, requiredHeight - 50);

            panel.Controls.Add(contentArea);
            return panel;
        }

        private Panel CreateLoopSection(string title, Color borderColor, List<string> blockIds, string loopId)
        {
            var panel = new Panel
            {
                Size = new Size(800, 200),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add colored header
            var header = new Panel
            {
                Size = new Size(798, 30),
                Location = new Point(1, 1),
                BackColor = borderColor
            };

            var headerLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 6),
                Size = new Size(300, 18),
                BackColor = Color.Transparent
            };
            header.Controls.Add(headerLabel);
            panel.Controls.Add(header);

            // Add content area
            var contentArea = new Panel
            {
                Location = new Point(10, 40),
                Size = new Size(780, 150),
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            int yPos = 10;

            // Display blocks inside the loop
            foreach (var blockId in blockIds)
            {
                var block = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == blockId);
                if (block != null)
                {
                    // Add arrow for non-first blocks
                    if (yPos > 10)
                    {
                        var arrow = CreateArrow();
                        arrow.Location = new Point(350, yPos - 5);
                        contentArea.Controls.Add(arrow);
                        yPos += 20;
                    }

                    var blockControl = CreateBlockControl(block);
                    blockControl.Location = new Point(0, yPos);
                    blockControl.Size = new Size(750, 50); // Full width for loop blocks
                    contentArea.Controls.Add(blockControl);
                    yPos += 60;
                }
            }

            // Add "Add an action" button
            var addButton = CreateAddButton();
            addButton.Location = new Point(0, yPos);
            addButton.Size = new Size(750, 30);
            
            // Store loop info
            addButton.Tag = $"Loop Body:{loopId}";
            contentArea.Controls.Add(addButton);

            // Adjust panel height based on content
            var requiredHeight = Math.Max(200, yPos + 80);
            panel.Size = new Size(800, requiredHeight);
            contentArea.Size = new Size(780, requiredHeight - 50);

            panel.Controls.Add(contentArea);
            return panel;
        }

        private Panel CreateSwitchCaseSection(string title, Color borderColor, List<string> blockIds, string switchBlockId, string caseId, bool isDefault)
        {
            System.Diagnostics.Debug.WriteLine($"CreateSwitchCaseSection: Creating section '{title}' with {blockIds.Count} block IDs: [{string.Join(", ", blockIds)}]");
            
            // Calculate minimum width needed
            const int minWidth = 200;
            const int maxWidth = 400;
            int titleWidth = Math.Max(minWidth, title.Length * 8 + 40); // Rough estimate for title width
            
            var panel = new Panel
            {
                Size = new Size(titleWidth, 200), // Start with calculated width, height will be adjusted later
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add colored header with dynamic width
            var header = new Panel
            {
                Size = new Size(titleWidth - 2, 30),
                Location = new Point(1, 1),
                BackColor = borderColor
            };

            var headerLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(titleWidth - 4, 28),
                Location = new Point(2, 1),
                TextAlign = ContentAlignment.MiddleCenter
            };

            header.Controls.Add(headerLabel);
            panel.Controls.Add(header);

            // Content area for blocks with dynamic width
            var contentArea = new Panel
            {
                Location = new Point(5, 35),
                Size = new Size(titleWidth - 10, 160),
                BackColor = Color.White,
                AutoScroll = true
            };

            // Get the actual blocks for this case
            var caseBlocks = new List<WorkflowBlock>();
            foreach (var id in blockIds)
            {
                var block = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == id);
                if (block != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Found case block: {block.Name} (ID: {block.Id})");
                    caseBlocks.Add(block);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"WARNING: Could not find block with ID: {id}");
                }
            }

            // Use BuildBlocksRecursive to properly handle nested blocks
            int yPos = 10;
            if (caseBlocks.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Building {caseBlocks.Count} blocks recursively in case section");
                var switchBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == switchBlockId);
                yPos = BuildBlocksRecursive(contentArea, caseBlocks, yPos, 0, switchBlock, suppressAddButtons: true);
                System.Diagnostics.Debug.WriteLine($"After BuildBlocksRecursive, yPos = {yPos}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No blocks found for this case");
            }

            // Add "Add an action" button with dynamic width
            var addButton = CreateAddButton();
            addButton.Location = new Point(0, yPos);
            addButton.Size = new Size(titleWidth - 20, 30);
            
            // Store branch info for the switch case - format: "Case:caseId:switchBlockId" or "Default:switchBlockId"
            if (isDefault)
            {
                addButton.Tag = $"Default:{switchBlockId}";
                System.Diagnostics.Debug.WriteLine($"Creating Default case button with tag: Default:{switchBlockId}");
            }
            else
            {
                addButton.Tag = $"Case:{caseId}:{switchBlockId}";
                System.Diagnostics.Debug.WriteLine($"Creating Case button with tag: Case:{caseId}:{switchBlockId}");
            }
            contentArea.Controls.Add(addButton);
            yPos += addButton.Height + 10;

            // Calculate optimal width based on content if there are blocks
            int finalWidth = titleWidth;
            if (caseBlocks.Count > 0)
            {
                // Consider block content for width calculation - most blocks need at least 250px
                finalWidth = Math.Max(titleWidth, 250);
                finalWidth = Math.Min(finalWidth, maxWidth); // Don't exceed maximum
            }

            // Adjust panel dimensions based on actual content
            var requiredHeight = Math.Max(200, yPos + 50);
            System.Diagnostics.Debug.WriteLine($"Setting panel size to {finalWidth}x{requiredHeight} (yPos={yPos}, titleWidth={titleWidth})");
            
            panel.Size = new Size(finalWidth, requiredHeight);
            contentArea.Size = new Size(finalWidth - 10, requiredHeight - 50);
            
            // Update header and button widths if final width changed
            if (finalWidth != titleWidth)
            {
                header.Size = new Size(finalWidth - 2, 30);
                headerLabel.Size = new Size(finalWidth - 4, 28);
                addButton.Size = new Size(finalWidth - 20, 30);
            }

            // Enable AutoScroll for contentArea to handle overflow
            contentArea.AutoScroll = true;

            panel.Controls.Add(contentArea);
            return panel;
        }

        private Button CreateAddButton()
        {
            var button = new Button
            {
                Size = new Size(400, 40),
                Text = "＋ Add an action",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(0, 120, 212),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            button.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 212);
            button.FlatAppearance.BorderSize = 2;
            button.Click += AddButton_Click;

            return button;
        }

        private Label CreateArrow()
        {
            return new Label
            {
                Text = "↓",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Gray,
                Size = new Size(20, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
        }

        private Color GetBlockColor(WorkflowBlockType type)
        {
            return type switch
            {
                WorkflowBlockType.MacroBlock => _macroColor,
                WorkflowBlockType.VariableBlock => _variableColor,
                WorkflowBlockType.ConditionalBlock => _conditionalColor,
                WorkflowBlockType.SwitchCaseBlock => _switchCaseColor,
                WorkflowBlockType.LoopBlock => _loopColor,
                WorkflowBlockType.DelayBlock => _delayColor,
                _ => Color.Gray
            };
        }

        private string GetBlockIcon(WorkflowBlockType type)
        {
            return type switch
            {
                WorkflowBlockType.MacroBlock => "🎯",
                WorkflowBlockType.VariableBlock => "📝",
                WorkflowBlockType.ConditionalBlock => "❓",
                WorkflowBlockType.SwitchCaseBlock => "🔀",
                WorkflowBlockType.LoopBlock => "🔄",
                WorkflowBlockType.DelayBlock => "⏱️",
                _ => "📦"
            };
        }

        private string GetBlockDescription(WorkflowBlock block)
        {
            return block switch
            {
                MacroBlock macro => $"Execute macro: {macro.MacroName}",
                VariableBlock variable => $"Set {variable.VariableName} from {variable.Source}",
                ConditionalBlock condition => GetConditionalDescription(condition),
                SwitchCaseBlock switchCase => GetSwitchCaseDescription(switchCase),
                LoopBlock loop => $"Loop {loop.LoopType}: {loop.Count} times",
                DelayBlock delay => $"Wait for {delay.DelaySeconds} seconds",
                _ => "Block action"
            };
        }

        private string GetConditionalDescription(ConditionalBlock condition)
        {
            // Get the display values, handling empty/null cases
            var leftDisplay = GetConditionalValueDisplay(condition.LeftValue, condition.LeftStaticValue, condition.LeftSource);
            var rightDisplay = GetConditionalValueDisplay(condition.RightValue, condition.RightStaticValue, condition.RightSource);
            
            // Format the condition type for display
            var conditionText = condition.ConditionType switch
            {
                ConditionType.Equals => "equals",
                ConditionType.NotEquals => "not equals", 
                ConditionType.Contains => "contains",
                ConditionType.NotContains => "not contains",
                ConditionType.GreaterThan => "greater than",
                ConditionType.LessThan => "less than",
                ConditionType.IsEmpty => "is empty",
                ConditionType.IsNotEmpty => "is not empty",
                ConditionType.Always => "always",
                ConditionType.Never => "never",
                _ => condition.ConditionType.ToString().ToLower()
            };

            // Handle special cases where right value isn't needed
            if (condition.ConditionType is ConditionType.IsEmpty or ConditionType.IsNotEmpty or 
                ConditionType.Always or ConditionType.Never)
            {
                return $"If {leftDisplay} {conditionText}";
            }

            return $"If {leftDisplay} {conditionText} {rightDisplay}";
        }

        private string GetConditionalValueDisplay(string? value, string? staticValue, VariableSource source)
        {
            // Use static value if available (for Value source)
            if (!string.IsNullOrEmpty(staticValue))
            {
                return $"'{staticValue}'";
            }

            // Use the primary value if available
            if (!string.IsNullOrEmpty(value))
            {
                return source == VariableSource.Value ? $"'{value}'" : value;
            }

            // Return placeholder based on source type
            return source switch
            {
                VariableSource.Variable => "[variable]",
                VariableSource.Value => "[empty]",
                VariableSource.Clipboard => "[clipboard]",
                VariableSource.WindowTitle => "[window title]",
                VariableSource.LastActiveWindow => "[last active window]",
                _ => "[unset]"
            };
        }

        private static string GetSwitchSourceDisplay(SwitchSourceType source)
        {
            return source switch
            {
                SwitchSourceType.Clipboard => "Clipboard",
                SwitchSourceType.ActiveWindow => "Active Window",
                _ => source.ToString()
            };
        }

        private string GetSwitchCaseDescription(SwitchCaseBlock switchCase)
        {
            var switchDisplay = GetSwitchSourceDisplay(switchCase.SwitchSource);
            var caseCount = switchCase.Cases.Count;
            var hasDefault = switchCase.Cases.Any(c => c.IsDefault);
            
            if (caseCount == 0)
            {
                return $"Switch {switchDisplay} (no cases)";
            }
            
            var normalCaseCount = caseCount - (hasDefault ? 1 : 0);
            var description = $"Switch {switchDisplay} ({normalCaseCount} case{(normalCaseCount == 1 ? "" : "s")}";
            
            if (hasDefault)
            {
                description += " + default";
            }
            
            description += ")";
            return description;
        }

        private void AddButton_Click(object? sender, EventArgs e)
        {
            _selectedAddButton = sender as Button;
            System.Diagnostics.Debug.WriteLine($"AddButton_Click: Button tag = {_selectedAddButton?.Tag}");
            ShowAddActionMenu();
        }

        private void ShowAddActionMenu()
        {
            var menu = new ContextMenuStrip();
            
            menu.Items.Add("Macro Action", null, (s, e) => AddBlock(WorkflowBlockType.MacroBlock));
            menu.Items.Add("Set Variable", null, (s, e) => AddBlock(WorkflowBlockType.VariableBlock));
            menu.Items.Add("Condition", null, (s, e) => AddBlock(WorkflowBlockType.ConditionalBlock));
            menu.Items.Add("Switch Case", null, (s, e) => AddBlock(WorkflowBlockType.SwitchCaseBlock));
            menu.Items.Add("Loop", null, (s, e) => AddBlock(WorkflowBlockType.LoopBlock));
            menu.Items.Add("Delay", null, (s, e) => AddBlock(WorkflowBlockType.DelayBlock));

            if (_selectedAddButton != null)
            {
                menu.Show(_selectedAddButton, new Point(0, _selectedAddButton.Height));
            }
        }

        private void AddBlock(WorkflowBlockType blockType)
        {
            System.Diagnostics.Debug.WriteLine($"AddBlock called with {blockType}, selectedButton tag = {_selectedAddButton?.Tag}");
            var newBlock = CreateBlock(blockType);
            
            // Handle adding to specific positions or branches
            System.Diagnostics.Debug.WriteLine($"Tag type: {_selectedAddButton?.Tag?.GetType()}, Tag value: {_selectedAddButton?.Tag}");
            if (_selectedAddButton?.Tag is WorkflowBlock parentBlock)
            {
                System.Diagnostics.Debug.WriteLine("Taking WorkflowBlock branch");
                // Add after specific parent block
                var parentIndex = _currentWorkflow.Blocks.IndexOf(parentBlock);
                if (parentIndex >= 0)
                {
                    _currentWorkflow.Blocks.Insert(parentIndex + 1, newBlock);
                }
                else
                {
                    _currentWorkflow.Blocks.Add(newBlock);
                }
            }
            else if (_selectedAddButton?.Tag is string branchInfo)
            {
                System.Diagnostics.Debug.WriteLine("Taking string branchInfo branch");
                // Handle conditional branch additions - DON'T add to main blocks list
                // Parse branch info and set up conditional connections
                var parts = branchInfo.Split(':');
                System.Diagnostics.Debug.WriteLine($"Branch info parts: {parts.Length} parts: [{string.Join(", ", parts)}]");
                if (parts.Length >= 2)
                {
                    var branchType = parts[0]; // "If yes", "If no", or "Loop Body"
                    var blockId = parts[1];
                    
                    if (branchType == "Loop Body")
                    {
                        // Handle loop body additions
                        var loopBlock = _currentWorkflow.Blocks.OfType<LoopBlock>()
                            .FirstOrDefault(b => b.Id == blockId);
                        
                        if (loopBlock != null)
                        {
                            // Add to main workflow blocks list for execution engine to find
                            _currentWorkflow.Blocks.Add(newBlock);
                            
                            // Add to LoopBlocks list for UI display inside loop container
                            if (!loopBlock.LoopBlocks.Contains(newBlock.Id))
                            {
                                loopBlock.LoopBlocks.Add(newBlock.Id);
                            }
                            
                            // Mark this block as belonging to a loop container to exclude from main flow display
                            newBlock.NextBlocks.Add("__LOOP_CONTAINED__");
                            
                            // Also set legacy property for backward compatibility
                            if (string.IsNullOrEmpty(loopBlock.LoopBody))
                            {
                                loopBlock.LoopBody = newBlock.Id;
                            }
                        }
                    }
                    else if (branchType == "If yes" || branchType == "If no")
                    {
                        // Handle conditional branch additions
                        var conditionalBlock = _currentWorkflow.Blocks.OfType<ConditionalBlock>()
                            .FirstOrDefault(b => b.Id == blockId);
                        
                        if (conditionalBlock != null)
                        {
                            // Add to main workflow blocks list for execution engine to find
                            _currentWorkflow.Blocks.Add(newBlock);
                            
                            if (branchType == "If yes")
                            {
                                // Add to TrueBlocks list for UI display inside conditional container
                                if (!conditionalBlock.TrueBlocks.Contains(newBlock.Id))
                                {
                                    conditionalBlock.TrueBlocks.Add(newBlock.Id);
                                }
                                // Mark this block as belonging to a conditional container to exclude from main flow display
                                newBlock.NextBlocks.Add("__CONDITIONAL_TRUE_CONTAINED__");
                                
                                // Also set legacy property for backward compatibility
                                if (string.IsNullOrEmpty(conditionalBlock.TrueBlock))
                                {
                                    conditionalBlock.TrueBlock = newBlock.Id;
                                }
                            }
                            else if (branchType == "If no")
                            {
                                // Add to FalseBlocks list for UI display inside conditional container
                                if (!conditionalBlock.FalseBlocks.Contains(newBlock.Id))
                                {
                                    conditionalBlock.FalseBlocks.Add(newBlock.Id);
                                }
                                // Mark this block as belonging to a conditional container to exclude from main flow display
                                newBlock.NextBlocks.Add("__CONDITIONAL_FALSE_CONTAINED__");
                                
                                // Also set legacy property for backward compatibility
                                if (string.IsNullOrEmpty(conditionalBlock.FalseBlock))
                                {
                                    conditionalBlock.FalseBlock = newBlock.Id;
                                }
                            }
                        }
                    }
                    else if (branchType == "Case" || branchType == "Default")
                    {
                        // Handle switch case branch additions
                        // branchInfo was already split: parts = ["Case"/"Default", caseId/switchBlockId, switchBlockId (for Case)]
                        System.Diagnostics.Debug.WriteLine($"Processing switch case button click: {branchType}");
                        if (parts.Length >= 2)
                        {
                            var isDefaultCase = branchType == "Default";
                            string switchBlockId;
                            string? caseId = null;
                            
                            if (isDefaultCase)
                            {
                                // Format was "Default:switchBlockId", so parts[1] = switchBlockId
                                switchBlockId = parts[1];
                            }
                            else
                            {
                                // Format was "Case:caseId:switchBlockId", so parts[1] = caseId, parts[2] = switchBlockId
                                caseId = parts[1];
                                switchBlockId = parts.Length > 2 ? parts[2] : blockId;
                            }
                            
                            var switchCaseBlock = _currentWorkflow.Blocks.OfType<SwitchCaseBlock>()
                                .FirstOrDefault(b => b.Id == switchBlockId);
                            
                            if (switchCaseBlock != null)
                            {
                                // Add to main workflow blocks list for execution engine to find
                                _currentWorkflow.Blocks.Add(newBlock);
                                
                                // Find the appropriate case to add the block to
                                SwitchCase? targetCase = null;
                                if (isDefaultCase)
                                {
                                    targetCase = switchCaseBlock.Cases.FirstOrDefault(c => c.IsDefault);
                                    // Create default case if it doesn't exist
                                    if (targetCase == null)
                                    {
                                        targetCase = new SwitchCase
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            CaseValue = "",
                                            IsDefault = true
                                        };
                                        switchCaseBlock.Cases.Add(targetCase);
                                    }
                                }
                                else if (!string.IsNullOrEmpty(caseId))
                                {
                                    targetCase = switchCaseBlock.Cases.FirstOrDefault(c => c.Id == caseId);
                                    // If case doesn't exist, we can't create it without knowing the case value
                                    // This should only happen if there's a configuration error
                                }
                                
                                if (targetCase != null)
                                {
                                    // Add to CaseBlocks list for UI display inside switch case container
                                    if (!targetCase.CaseBlocks.Contains(newBlock.Id))
                                    {
                                        targetCase.CaseBlocks.Add(newBlock.Id);
                                        System.Diagnostics.Debug.WriteLine($"Added block {newBlock.Id} to case {targetCase.Id} (CaseBlocks count: {targetCase.CaseBlocks.Count})");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Block {newBlock.Id} already exists in case {targetCase.Id}");
                                    }
                                    
                                    // Mark this block as belonging to a switch case container to exclude from main flow display
                                    var containerTag = isDefaultCase ? "__SWITCHCASE_DEFAULT_CONTAINED__" : $"__SWITCHCASE_{caseId}_CONTAINED__";
                                    newBlock.NextBlocks.Add(containerTag);
                                    System.Diagnostics.Debug.WriteLine($"Added container tag: {containerTag}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"TARGET CASE IS NULL! isDefaultCase={isDefaultCase}, caseId={caseId}");
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Taking default 'Add to end of workflow' branch");
                // Add to end of workflow
                _currentWorkflow.Blocks.Add(newBlock);
            }
            
            // Set as start block if first block
            if (_currentWorkflow.Blocks.Count == 1)
            {
                _currentWorkflow.StartBlockId = newBlock.Id;
            }

            // Configure the block
            System.Diagnostics.Debug.WriteLine($"About to edit new block: {newBlock.Name} (ID: {newBlock.Id})");
            EditBlock(newBlock);
            
            // Rebuild UI
            System.Diagnostics.Debug.WriteLine("Calling BuildFlowUI() to refresh interface");
            BuildFlowUI();
            System.Diagnostics.Debug.WriteLine("BuildFlowUI() completed");
        }

        private WorkflowBlock CreateBlock(WorkflowBlockType blockType)
        {
            return blockType switch
            {
                WorkflowBlockType.MacroBlock => new MacroBlock { Name = "New Macro Action" },
                WorkflowBlockType.VariableBlock => new VariableBlock { Name = "Set Variable" },
                WorkflowBlockType.ConditionalBlock => new ConditionalBlock { Name = "New Condition" },
                WorkflowBlockType.SwitchCaseBlock => new SwitchCaseBlock { Name = "New Switch Case" },
                WorkflowBlockType.LoopBlock => new LoopBlock { Name = "New Loop" },
                WorkflowBlockType.DelayBlock => new DelayBlock { Name = "New Delay" },
                _ => throw new ArgumentException("Unknown block type")
            };
        }

        private void EditBlock(WorkflowBlock block)
        {
            using var propertiesForm = new BlockPropertiesForm(block, _availableMacros, _currentWorkflow);
            if (propertiesForm.ShowDialog() == DialogResult.OK)
            {
                BuildFlowUI(); // Refresh the UI
            }
        }

        private void ShowBlockOptions(WorkflowBlock block, Control sender)
        {
            var menu = new ContextMenuStrip();
            
            menu.Items.Add("Edit", null, (s, e) => EditBlock(block));
            menu.Items.Add("Delete", null, (s, e) => DeleteBlock(block));
            menu.Items.Add("-");
            
            if (block.Id != _currentWorkflow.StartBlockId)
            {
                menu.Items.Add("Set as Start", null, (s, e) => SetStartBlock(block));
            }

            menu.Show(sender, new Point(0, sender.Height));
        }

        private void DeleteBlock(WorkflowBlock block)
        {
            if (MessageBox.Show($"Delete '{block.Name}'?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _currentWorkflow.Blocks.Remove(block);
                
                // Update start block if necessary
                if (_currentWorkflow.StartBlockId == block.Id)
                {
                    _currentWorkflow.StartBlockId = _currentWorkflow.Blocks.FirstOrDefault()?.Id ?? "";
                }
                
                BuildFlowUI();
            }
        }

        private void SetStartBlock(WorkflowBlock block)
        {
            _currentWorkflow.StartBlockId = block.Id;
            BuildFlowUI();
        }

        private void SaveFlow_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private async void RunFlow_Click(object? sender, EventArgs e)
        {
            if (_currentWorkflow?.Blocks?.Count == 0)
            {
                MessageBox.Show("Add some actions to test the flow.", "Empty Flow", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Get workflow engine from MainForm
                var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
                if (mainForm?.WorkflowEngine == null)
                {
                    MessageBox.Show("Workflow engine not available. Please ensure the main application is running.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Show running status
                if (sender is Button btn)
                {
                    btn.Text = "Running...";
                    btn.Enabled = false;
                }

                await mainForm.WorkflowEngine.ExecuteWorkflowAsync(_currentWorkflow!);
                
                MessageBox.Show("Workflow executed successfully!", "Test Flow Complete", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing workflow: {ex.Message}", "Test Flow Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Restore button
                if (sender is Button btn)
                {
                    btn.Text = "Test Flow";
                    btn.Enabled = true;
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(900, 700);
            this.Name = "PowerAutomateWorkflowDesigner";
            this.Text = "Flow Designer";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ResumeLayout(false);
        }
    }
}