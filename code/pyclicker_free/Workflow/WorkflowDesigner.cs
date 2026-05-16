using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PyClickerRecorder.Workflow
{
    public partial class WorkflowDesigner : Form
    {
        private Workflow _currentWorkflow;
        private WorkflowBlock? _selectedBlock;
        private WorkflowBlock? _draggedBlock;
        private Point _dragOffset;
        private bool _isDragging;
        private bool _isConnecting;
        private WorkflowBlock? _connectionSource;
        private Point _mousePosition;
        private readonly List<SavedMacro> _availableMacros;

        // UI Constants - Improved for better visibility and usability
        private const int BlockWidth = 160;
        private const int BlockHeight = 80;
        private const int GridSize = 25;
        private const int CanvasMargin = 75;

        public Workflow CurrentWorkflow => _currentWorkflow;

        public WorkflowDesigner(List<SavedMacro> availableMacros)
        {
            InitializeComponent();
            _availableMacros = availableMacros ?? new List<SavedMacro>();
            _currentWorkflow = new Workflow();
            SetupDesigner();
        }

        public WorkflowDesigner(Workflow? workflow, List<SavedMacro> availableMacros)
        {
            InitializeComponent();
            _availableMacros = availableMacros ?? new List<SavedMacro>();
            _currentWorkflow = workflow ?? new Workflow();
            SetupDesigner();
        }

        private void SetupDesigner()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.BackColor = Color.WhiteSmoke;
            this.Size = new Size(1000, 700);
            this.Text = $"Workflow Designer - {_currentWorkflow.Name}";
            
            CreateToolPanel();
            CreatePropertiesPanel();
            SetupCanvasEvents();
        }

        private void CreateToolPanel()
        {
            var toolPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.LightGray
            };

            var lblTools = new Label
            {
                Text = "Toolbox",
                Location = new Point(10, 10),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            toolPanel.Controls.Add(lblTools);

            // Add block type buttons
            var btnMacro = CreateToolButton("Macro Block", WorkflowBlockType.MacroBlock, 40);
            var btnCondition = CreateToolButton("Conditional", WorkflowBlockType.ConditionalBlock, 70);
            var btnSwitchCase = CreateToolButton("Switch Case", WorkflowBlockType.SwitchCaseBlock, 100);
            var btnLoop = CreateToolButton("Loop Block", WorkflowBlockType.LoopBlock, 130);
            var btnDelay = CreateToolButton("Delay Block", WorkflowBlockType.DelayBlock, 160);
            var btnVariable = CreateToolButton("Variable", WorkflowBlockType.VariableBlock, 190);

            toolPanel.Controls.Add(btnMacro);
            toolPanel.Controls.Add(btnCondition);
            toolPanel.Controls.Add(btnSwitchCase);
            toolPanel.Controls.Add(btnLoop);
            toolPanel.Controls.Add(btnDelay);
            toolPanel.Controls.Add(btnVariable);

            // Add workflow controls
            var btnSave = new Button
            {
                Text = "Save Workflow",
                Location = new Point(10, 230),
                Size = new Size(180, 30)
            };
            btnSave.Click += SaveWorkflow_Click;
            toolPanel.Controls.Add(btnSave);

            var btnLoad = new Button
            {
                Text = "Load Workflow",
                Location = new Point(10, 270),
                Size = new Size(180, 30)
            };
            btnLoad.Click += LoadWorkflow_Click;
            toolPanel.Controls.Add(btnLoad);

            var btnRun = new Button
            {
                Text = "Run Workflow",
                Location = new Point(10, 310),
                Size = new Size(180, 30),
                BackColor = Color.LightGreen
            };
            btnRun.Click += RunWorkflow_Click;
            toolPanel.Controls.Add(btnRun);

            this.Controls.Add(toolPanel);
        }

        private Button CreateToolButton(string text, WorkflowBlockType blockType, int yPosition)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(10, yPosition),
                Size = new Size(180, 25),
                Tag = blockType
            };
            button.Click += ToolButton_Click;
            return button;
        }

        private void CreatePropertiesPanel()
        {
            var propertiesPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 250,
                BackColor = Color.LightGray
            };

            var lblProperties = new Label
            {
                Text = "Properties",
                Location = new Point(10, 10),
                Size = new Size(230, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            propertiesPanel.Controls.Add(lblProperties);

            this.Controls.Add(propertiesPanel);
        }

        private void SetupCanvasEvents()
        {
            this.MouseDown += Canvas_MouseDown;
            this.MouseMove += Canvas_MouseMove;
            this.MouseUp += Canvas_MouseUp;
            this.MouseDoubleClick += Canvas_MouseDoubleClick;
            this.Paint += Canvas_Paint;
            this.KeyDown += Canvas_KeyDown;
            this.KeyPreview = true;
        }

        private void ToolButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var blockType = (WorkflowBlockType)button.Tag;
            
            // Create new block at a default position with better spacing
            var newBlock = CreateBlock(blockType);
            
            // Auto-arrange blocks to avoid overlapping
            ArrangeNewBlock(newBlock);
            
            _currentWorkflow.Blocks.Add(newBlock);
            
            if (_currentWorkflow.Blocks.Count == 1)
            {
                _currentWorkflow.StartBlockId = newBlock.Id;
            }
            
            this.Invalidate();
        }

        private WorkflowBlock CreateBlock(WorkflowBlockType blockType)
        {
            return blockType switch
            {
                WorkflowBlockType.MacroBlock => new MacroBlock { Name = "New Macro" },
                WorkflowBlockType.ConditionalBlock => new ConditionalBlock { Name = "New Condition" },
                WorkflowBlockType.SwitchCaseBlock => new SwitchCaseBlock { Name = "New Switch Case" },
                WorkflowBlockType.LoopBlock => new LoopBlock { Name = "New Loop" },
                WorkflowBlockType.DelayBlock => new DelayBlock { Name = "New Delay" },
                WorkflowBlockType.VariableBlock => new VariableBlock { Name = "New Variable" },
                _ => throw new ArgumentException("Unknown block type")
            };
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            // Ensure the form has focus
            this.Focus();
            
            var clickedBlock = GetBlockAtPosition(e.Location);
            System.Diagnostics.Debug.WriteLine($"Mouse down: Button={e.Button}, Location={e.Location}, ClickedBlock={clickedBlock?.Name ?? "null"}");
            
            if (e.Button == MouseButtons.Left)
            {
                if (Control.ModifierKeys == Keys.Control && _selectedBlock != null)
                {
                    // Start connection mode
                    _isConnecting = true;
                    _connectionSource = _selectedBlock;
                }
                else if (clickedBlock != null)
                {
                    // Start dragging
                    _selectedBlock = clickedBlock;
                    _draggedBlock = clickedBlock;
                    _isDragging = true;
                    _dragOffset = new Point(e.X - clickedBlock.X, e.Y - clickedBlock.Y);
                }
                else
                {
                    _selectedBlock = null;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                System.Diagnostics.Debug.WriteLine($"Right click detected on block: {clickedBlock?.Name ?? "null"}");
                if (clickedBlock != null)
                {
                    _selectedBlock = clickedBlock; // Select the block first
                    ShowContextMenu(clickedBlock, e.Location);
                }
            }
            
            this.Invalidate();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            _mousePosition = e.Location;
            
            if (_isDragging && _draggedBlock != null)
            {
                _draggedBlock.X = SnapToGrid(e.X - _dragOffset.X);
                _draggedBlock.Y = SnapToGrid(e.Y - _dragOffset.Y);
                this.Invalidate();
            }
            else if (_isConnecting)
            {
                this.Invalidate(); // Redraw to show connection line
            }
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _draggedBlock = null;
            }
            else if (_isConnecting)
            {
                var targetBlock = GetBlockAtPosition(e.Location);
                if (targetBlock != null && targetBlock != _connectionSource)
                {
                    // Create connection
                    if (!_connectionSource.NextBlocks.Contains(targetBlock.Id))
                    {
                        _connectionSource.NextBlocks.Add(targetBlock.Id);
                    }
                }
                _isConnecting = false;
                _connectionSource = null;
                this.Invalidate();
            }
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(this.BackColor);
            
            DrawGrid(g);
            DrawConnections(g);
            DrawBlocks(g);
            
            if (_isConnecting && _connectionSource != null)
            {
                DrawConnectionLine(g, GetBlockCenter(_connectionSource), _mousePosition, Color.Blue);
            }
        }

        private void DrawGrid(Graphics g)
        {
            using (var pen = new Pen(Color.LightBlue, 1))
            {
                for (int x = 0; x < this.Width; x += GridSize)
                {
                    g.DrawLine(pen, x, 0, x, this.Height);
                }
                for (int y = 0; y < this.Height; y += GridSize)
                {
                    g.DrawLine(pen, 0, y, this.Width, y);
                }
            }
        }

        private void DrawBlocks(Graphics g)
        {
            var allBlocks = GetAllAccessibleBlocks();
            foreach (var block in allBlocks)
            {
                DrawBlock(g, block);
            }
        }

        private void DrawBlock(Graphics g, WorkflowBlock block)
        {
            var rect = new Rectangle(block.X, block.Y, BlockWidth, BlockHeight);
            var isSelected = block == _selectedBlock;
            var isStartBlock = block.Id == _currentWorkflow.StartBlockId;
            
            // Choose colors based on block type and state
            Color fillColor = GetBlockColor(block.Type);
            Color borderColor = isSelected ? Color.Red : (isStartBlock ? Color.Green : Color.Black);
            int borderWidth = isSelected ? 3 : (isStartBlock ? 2 : 1);
            
            // Draw block background
            using (var brush = new SolidBrush(fillColor))
            {
                g.FillRectangle(brush, rect);
            }
            
            // Draw block border
            using (var pen = new Pen(borderColor, borderWidth))
            {
                g.DrawRectangle(pen, rect);
            }
            
            // Draw block text with additional information for switch cases
            var textRect = new Rectangle(rect.X + 8, rect.Y + 8, rect.Width - 16, rect.Height - 16);
            using (var brush = new SolidBrush(Color.Black))
            using (var font = new Font("Segoe UI", 9, FontStyle.Regular))
            using (var smallFont = new Font("Segoe UI", 7, FontStyle.Regular))
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                
                string displayText = block.Name;
                
                // Add additional info for switch case blocks
                if (block is SwitchCaseBlock switchCaseBlock)
                {
                    displayText = $"{block.Name}\n({switchCaseBlock.Cases.Count} cases)";
                    
                    // Draw case count indicator
                    var caseCountRect = new Rectangle(rect.Right - 25, rect.Y + 2, 20, 15);
                    using (var caseBrush = new SolidBrush(Color.Navy))
                    {
                        g.FillEllipse(caseBrush, caseCountRect);
                        using (var whiteBrush = new SolidBrush(Color.White))
                        {
                            g.DrawString(switchCaseBlock.Cases.Count.ToString(), smallFont, whiteBrush, caseCountRect, format);
                        }
                    }
                }
                // Add additional info for conditional blocks
                else if (block is ConditionalBlock conditionalBlock)
                {
                    int totalConnections = conditionalBlock.TrueBlocks.Count + conditionalBlock.FalseBlocks.Count;
                    if (!string.IsNullOrEmpty(conditionalBlock.TrueBlock)) totalConnections++;
                    if (!string.IsNullOrEmpty(conditionalBlock.FalseBlock)) totalConnections++;
                    
                    if (totalConnections > 0)
                    {
                        displayText = $"{block.Name}\n({totalConnections} branches)";
                    }
                }
                
                g.DrawString(displayText, font, brush, textRect, format);
            }
            
            // Draw connection points
            DrawConnectionPoints(g, block);
        }

        private void DrawConnectionPoints(Graphics g, WorkflowBlock block)
        {
            var center = GetBlockCenter(block);
            var pointSize = 8;
            
            // Output point (bottom) - more visible
            var outputPoint = new Point(center.X, block.Y + BlockHeight);
            var outputRect = new Rectangle(outputPoint.X - pointSize/2, outputPoint.Y - pointSize/2, pointSize, pointSize);
            g.FillEllipse(Brushes.DodgerBlue, outputRect);
            g.DrawEllipse(Pens.DarkBlue, outputRect);
            
            // Input point (top) - more visible
            var inputPoint = new Point(center.X, block.Y);
            var inputRect = new Rectangle(inputPoint.X - pointSize/2, inputPoint.Y - pointSize/2, pointSize, pointSize);
            g.FillEllipse(Brushes.LimeGreen, inputRect);
            g.DrawEllipse(Pens.DarkGreen, inputRect);
        }

        private void DrawConnections(Graphics g)
        {
            foreach (var block in _currentWorkflow.Blocks)
            {
                // Draw standard NextBlocks connections
                foreach (var nextBlockId in block.NextBlocks)
                {
                    var nextBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == nextBlockId);
                    if (nextBlock != null)
                    {
                        var fromPoint = new Point(GetBlockCenter(block).X, block.Y + BlockHeight);
                        var toPoint = new Point(GetBlockCenter(nextBlock).X, nextBlock.Y);
                        DrawConnectionLine(g, fromPoint, toPoint, Color.Black);
                    }
                }
                
                // Draw specific connections for switch case blocks
                if (block is SwitchCaseBlock switchCaseBlock)
                {
                    foreach (var switchCase in switchCaseBlock.Cases)
                    {
                        foreach (var caseBlockId in switchCase.CaseBlocks)
                        {
                            var caseBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == caseBlockId);
                            if (caseBlock != null)
                            {
                                var fromPoint = new Point(GetBlockCenter(switchCaseBlock).X, switchCaseBlock.Y + BlockHeight);
                                var toPoint = new Point(GetBlockCenter(caseBlock).X, caseBlock.Y);
                                // Use different colors for different cases
                                var connectionColor = switchCase.IsDefault ? Color.Orange : Color.DarkBlue;
                                DrawConnectionLine(g, fromPoint, toPoint, connectionColor);
                                
                                // Draw case label near the connection
                                var labelText = switchCase.IsDefault ? "default" : switchCase.CaseValue;
                                if (!string.IsNullOrEmpty(labelText))
                                {
                                    var midPoint = new Point((fromPoint.X + toPoint.X) / 2, (fromPoint.Y + toPoint.Y) / 2);
                                    DrawConnectionLabel(g, midPoint, labelText, connectionColor);
                                }
                            }
                        }
                    }
                }
                
                // Draw specific connections for conditional blocks
                if (block is ConditionalBlock conditionalBlock)
                {
                    // Draw TrueBlocks connections in green
                    foreach (var trueBlockId in conditionalBlock.TrueBlocks)
                    {
                        var trueBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == trueBlockId);
                        if (trueBlock != null)
                        {
                            var fromPoint = new Point(GetBlockCenter(conditionalBlock).X, conditionalBlock.Y + BlockHeight);
                            var toPoint = new Point(GetBlockCenter(trueBlock).X, trueBlock.Y);
                            DrawConnectionLine(g, fromPoint, toPoint, Color.Green);
                            
                            // Draw true label
                            var midPoint = new Point((fromPoint.X + toPoint.X) / 2, (fromPoint.Y + toPoint.Y) / 2);
                            DrawConnectionLabel(g, midPoint, "true", Color.Green);
                        }
                    }
                    
                    // Draw FalseBlocks connections in red
                    foreach (var falseBlockId in conditionalBlock.FalseBlocks)
                    {
                        var falseBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == falseBlockId);
                        if (falseBlock != null)
                        {
                            var fromPoint = new Point(GetBlockCenter(conditionalBlock).X, conditionalBlock.Y + BlockHeight);
                            var toPoint = new Point(GetBlockCenter(falseBlock).X, falseBlock.Y);
                            DrawConnectionLine(g, fromPoint, toPoint, Color.Red);
                            
                            // Draw false label
                            var midPoint = new Point((fromPoint.X + toPoint.X) / 2, (fromPoint.Y + toPoint.Y) / 2);
                            DrawConnectionLabel(g, midPoint, "false", Color.Red);
                        }
                    }
                    
                    // Draw legacy single block connections
                    if (!string.IsNullOrEmpty(conditionalBlock.TrueBlock))
                    {
                        var trueBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == conditionalBlock.TrueBlock);
                        if (trueBlock != null)
                        {
                            var fromPoint = new Point(GetBlockCenter(conditionalBlock).X, conditionalBlock.Y + BlockHeight);
                            var toPoint = new Point(GetBlockCenter(trueBlock).X, trueBlock.Y);
                            DrawConnectionLine(g, fromPoint, toPoint, Color.Green);
                            
                            // Draw true label
                            var midPoint = new Point((fromPoint.X + toPoint.X) / 2, (fromPoint.Y + toPoint.Y) / 2);
                            DrawConnectionLabel(g, midPoint, "true", Color.Green);
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(conditionalBlock.FalseBlock))
                    {
                        var falseBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == conditionalBlock.FalseBlock);
                        if (falseBlock != null)
                        {
                            var fromPoint = new Point(GetBlockCenter(conditionalBlock).X, conditionalBlock.Y + BlockHeight);
                            var toPoint = new Point(GetBlockCenter(falseBlock).X, falseBlock.Y);
                            DrawConnectionLine(g, fromPoint, toPoint, Color.Red);
                            
                            // Draw false label
                            var midPoint = new Point((fromPoint.X + toPoint.X) / 2, (fromPoint.Y + toPoint.Y) / 2);
                            DrawConnectionLabel(g, midPoint, "false", Color.Red);
                        }
                    }
                }
            }
        }

        private void DrawConnectionLine(Graphics g, Point from, Point to, Color color)
        {
            using (var pen = new Pen(color, 2))
            {
                // Draw curved connection line
                var controlPoint1 = new Point(from.X, from.Y + (to.Y - from.Y) / 3);
                var controlPoint2 = new Point(to.X, to.Y - (to.Y - from.Y) / 3);
                
                g.DrawBezier(pen, from, controlPoint1, controlPoint2, to);
                
                // Draw arrowhead
                DrawArrowHead(g, pen, controlPoint2, to);
            }
        }

        private void DrawArrowHead(Graphics g, Pen pen, Point from, Point to)
        {
            const int arrowLength = 10;
            const double arrowAngle = Math.PI / 6; // 30 degrees
            
            double angle = Math.Atan2(to.Y - from.Y, to.X - from.X);
            
            var arrowPoint1 = new Point(
                (int)(to.X - arrowLength * Math.Cos(angle - arrowAngle)),
                (int)(to.Y - arrowLength * Math.Sin(angle - arrowAngle))
            );
            
            var arrowPoint2 = new Point(
                (int)(to.X - arrowLength * Math.Cos(angle + arrowAngle)),
                (int)(to.Y - arrowLength * Math.Sin(angle + arrowAngle))
            );
            
            g.DrawLine(pen, to, arrowPoint1);
            g.DrawLine(pen, to, arrowPoint2);
        }
        
        private void DrawConnectionLabel(Graphics g, Point position, string text, Color color)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            using (var textBrush = new SolidBrush(color))
            using (var borderBrush = new SolidBrush(Color.FromArgb(128, color)))
            {
                var textSize = g.MeasureString(text, font);
                var labelRect = new RectangleF(
                    position.X - textSize.Width / 2 - 2,
                    position.Y - textSize.Height / 2 - 1,
                    textSize.Width + 4,
                    textSize.Height + 2
                );
                
                // Draw label background
                g.FillRectangle(brush, labelRect);
                g.DrawRectangle(new Pen(borderBrush), Rectangle.Round(labelRect));
                
                // Draw label text
                var textPosition = new PointF(
                    position.X - textSize.Width / 2,
                    position.Y - textSize.Height / 2
                );
                g.DrawString(text, font, textBrush, textPosition);
            }
        }

        private Color GetBlockColor(WorkflowBlockType blockType)
        {
            return blockType switch
            {
                WorkflowBlockType.MacroBlock => Color.LightBlue,
                WorkflowBlockType.ConditionalBlock => Color.LightYellow,
                WorkflowBlockType.SwitchCaseBlock => Color.LightCoral,
                WorkflowBlockType.LoopBlock => Color.LightGreen,
                WorkflowBlockType.DelayBlock => Color.LightPink,
                WorkflowBlockType.VariableBlock => Color.LightCyan,
                _ => Color.White
            };
        }

        private WorkflowBlock? GetBlockAtPosition(Point position)
        {
            // Check all blocks, including those that might be in conditional branches
            var allBlocks = GetAllAccessibleBlocks();
            return allBlocks
                .FirstOrDefault(b => new Rectangle(b.X, b.Y, BlockWidth, BlockHeight).Contains(position));
        }
        
        private List<WorkflowBlock> GetAllAccessibleBlocks()
        {
            var allBlocks = new List<WorkflowBlock>();
            
            // Add all main workflow blocks
            allBlocks.AddRange(_currentWorkflow.Blocks);
            
            // Add blocks that might be referenced in conditionals but not in main list
            foreach (var block in _currentWorkflow.Blocks.OfType<ConditionalBlock>())
            {
                // Add blocks from TrueBlocks that aren't already included
                foreach (var trueBlockId in block.TrueBlocks)
                {
                    var trueBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == trueBlockId);
                    if (trueBlock != null && !allBlocks.Contains(trueBlock))
                    {
                        allBlocks.Add(trueBlock);
                    }
                }
                
                // Add blocks from FalseBlocks that aren't already included
                foreach (var falseBlockId in block.FalseBlocks)
                {
                    var falseBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == falseBlockId);
                    if (falseBlock != null && !allBlocks.Contains(falseBlock))
                    {
                        allBlocks.Add(falseBlock);
                    }
                }
                
                // Add legacy single blocks
                if (!string.IsNullOrEmpty(block.TrueBlock))
                {
                    var trueBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == block.TrueBlock);
                    if (trueBlock != null && !allBlocks.Contains(trueBlock))
                    {
                        allBlocks.Add(trueBlock);
                    }
                }
                
                if (!string.IsNullOrEmpty(block.FalseBlock))
                {
                    var falseBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == block.FalseBlock);
                    if (falseBlock != null && !allBlocks.Contains(falseBlock))
                    {
                        allBlocks.Add(falseBlock);
                    }
                }
            }
            
            // Add blocks that might be referenced in switch case blocks but not in main list
            foreach (var block in _currentWorkflow.Blocks.OfType<SwitchCaseBlock>())
            {
                foreach (var switchCase in block.Cases)
                {
                    // Add blocks from each case's CaseBlocks that aren't already included
                    foreach (var caseBlockId in switchCase.CaseBlocks)
                    {
                        var caseBlock = _currentWorkflow.Blocks.FirstOrDefault(b => b.Id == caseBlockId);
                        if (caseBlock != null && !allBlocks.Contains(caseBlock))
                        {
                            allBlocks.Add(caseBlock);
                        }
                    }
                }
            }
            
            return allBlocks;
        }

        private Point GetBlockCenter(WorkflowBlock block)
        {
            return new Point(block.X + BlockWidth / 2, block.Y + BlockHeight / 2);
        }
        
        private void ArrangeNewBlock(WorkflowBlock newBlock)
        {
            var allExistingBlocks = GetAllAccessibleBlocks();
            
            // Start with a default position
            int startX = CanvasMargin;
            int startY = CanvasMargin;
            
            // If there are existing blocks, try to place it in a good spot
            if (allExistingBlocks.Count > 0)
            {
                // Try to place it in the next logical position
                var lastBlock = allExistingBlocks.OrderBy(b => b.X).ThenBy(b => b.Y).LastOrDefault();
                if (lastBlock != null)
                {
                    // Place it to the right of the last block
                    startX = lastBlock.X + BlockWidth + 50;
                    startY = lastBlock.Y;
                    
                    // If it would go off screen or overlap, move to next row
                    if (startX + BlockWidth > this.ClientSize.Width - CanvasMargin)
                    {
                        startX = CanvasMargin;
                        startY = lastBlock.Y + BlockHeight + 50;
                    }
                }
            }
            
            // Ensure the position doesn't conflict with existing blocks
            var finalPosition = FindNonOverlappingPosition(startX, startY, allExistingBlocks);
            newBlock.X = SnapToGrid(finalPosition.X);
            newBlock.Y = SnapToGrid(finalPosition.Y);
        }
        
        private Point FindNonOverlappingPosition(int startX, int startY, List<WorkflowBlock> existingBlocks)
        {
            int x = startX;
            int y = startY;
            int spacing = BlockWidth + 30;
            int maxAttempts = 20;
            int attempts = 0;
            
            while (attempts < maxAttempts)
            {
                var testRect = new Rectangle(x, y, BlockWidth, BlockHeight);
                bool overlaps = existingBlocks.Any(b => 
                {
                    var blockRect = new Rectangle(b.X, b.Y, BlockWidth, BlockHeight);
                    return blockRect.IntersectsWith(testRect);
                });
                
                if (!overlaps)
                {
                    return new Point(x, y);
                }
                
                // Try next position
                x += spacing;
                if (x + BlockWidth > this.ClientSize.Width - CanvasMargin)
                {
                    x = CanvasMargin;
                    y += BlockHeight + 30;
                }
                
                attempts++;
            }
            
            // If we can't find a good spot, just use the original position
            return new Point(startX, startY);
        }

        private int SnapToGrid(int value)
        {
            return ((value + GridSize / 2) / GridSize) * GridSize;
        }

        private void ShowContextMenu(WorkflowBlock block, Point location)
        {
            System.Diagnostics.Debug.WriteLine($"ShowContextMenu called for block: {block.Name} at {location}");
            
            var contextMenu = new ContextMenuStrip();
            
            var editItem = new ToolStripMenuItem("Edit Properties");
            editItem.Click += (s, e) => {
                System.Diagnostics.Debug.WriteLine($"Edit Properties clicked for {block.Name}");
                EditBlockProperties(block);
            };
            contextMenu.Items.Add(editItem);
            
            var deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (s, e) => {
                System.Diagnostics.Debug.WriteLine($"Delete clicked for {block.Name}");
                DeleteBlock(block);
            };
            contextMenu.Items.Add(deleteItem);
            
            if (block.Id != _currentWorkflow.StartBlockId)
            {
                var setStartItem = new ToolStripMenuItem("Set as Start Block");
                setStartItem.Click += (s, e) => {
                    System.Diagnostics.Debug.WriteLine($"Set as Start Block clicked for {block.Name}");
                    SetStartBlock(block);
                };
                contextMenu.Items.Add(setStartItem);
            }
            
            // Convert to screen coordinates
            var screenLocation = this.PointToScreen(location);
            System.Diagnostics.Debug.WriteLine($"Showing context menu at screen coordinates: {screenLocation}");
            contextMenu.Show(screenLocation);
        }

        private void Canvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Double click at {e.Location}");
            
            // Prevent any default behavior that might copy text
            var clickedBlock = GetBlockAtPosition(e.Location);
            if (clickedBlock != null)
            {
                System.Diagnostics.Debug.WriteLine($"Double clicking block: {clickedBlock.Name}");
                
                // Clear any potential clipboard interference
                try
                {
                    // Don't use the clipboard at all during double-click
                    System.Diagnostics.Debug.WriteLine("Opening properties for block");
                    EditBlockProperties(clickedBlock);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in double-click handler: {ex.Message}");
                }
            }
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && _selectedBlock != null)
            {
                DeleteBlock(_selectedBlock);
                e.Handled = true;
            }
        }

        private void EditBlockProperties(WorkflowBlock block)
        {
            using (var propertiesForm = new BlockPropertiesForm(block, _availableMacros, _currentWorkflow))
            {
                if (propertiesForm.ShowDialog() == DialogResult.OK)
                {
                    this.Invalidate();
                }
            }
        }

        private void DeleteBlock(WorkflowBlock block)
        {
            if (MessageBox.Show($"Delete block '{block.Name}'?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _currentWorkflow.Blocks.Remove(block);
                
                // Remove references to this block from NextBlocks
                foreach (var otherBlock in _currentWorkflow.Blocks)
                {
                    otherBlock.NextBlocks.RemoveAll(id => id == block.Id);
                }
                
                // Remove references from ConditionalBlocks
                foreach (var conditionalBlock in _currentWorkflow.Blocks.OfType<ConditionalBlock>())
                {
                    conditionalBlock.TrueBlocks.RemoveAll(id => id == block.Id);
                    conditionalBlock.FalseBlocks.RemoveAll(id => id == block.Id);
                    
                    // Remove legacy single block references
                    if (conditionalBlock.TrueBlock == block.Id)
                        conditionalBlock.TrueBlock = "";
                    if (conditionalBlock.FalseBlock == block.Id)
                        conditionalBlock.FalseBlock = "";
                }
                
                // Remove references from SwitchCaseBlocks
                foreach (var switchCaseBlock in _currentWorkflow.Blocks.OfType<SwitchCaseBlock>())
                {
                    foreach (var switchCase in switchCaseBlock.Cases)
                    {
                        switchCase.CaseBlocks.RemoveAll(id => id == block.Id);
                    }
                }
                
                // Update start block if necessary
                if (_currentWorkflow.StartBlockId == block.Id)
                {
                    _currentWorkflow.StartBlockId = _currentWorkflow.Blocks.FirstOrDefault()?.Id ?? "";
                }
                
                if (_selectedBlock == block)
                {
                    _selectedBlock = null;
                }
                
                this.Invalidate();
            }
        }

        private void SetStartBlock(WorkflowBlock block)
        {
            _currentWorkflow.StartBlockId = block.Id;
            this.Invalidate();
        }

        private void SaveWorkflow_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Workflow Files (*.wf)|*.wf|All Files (*.*)|*.*";
                saveDialog.DefaultExt = "wf";
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var storage = new WorkflowStorage();
                        storage.ExportWorkflowAsync(_currentWorkflow.Id, saveDialog.FileName).Wait();
                        MessageBox.Show("Workflow saved successfully!", "Save Workflow", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save workflow: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadWorkflow_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "Workflow Files (*.wf)|*.wf|All Files (*.*)|*.*";
                
                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var storage = new WorkflowStorage();
                        _currentWorkflow = storage.ImportWorkflowAsync(openDialog.FileName).Result;
                        this.Text = $"Workflow Designer - {_currentWorkflow.Name}";
                        this.Invalidate();
                        MessageBox.Show("Workflow loaded successfully!", "Load Workflow", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load workflow: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void RunWorkflow_Click(object sender, EventArgs e)
        {
            if (_currentWorkflow.Blocks.Count == 0)
            {
                MessageBox.Show("Workflow is empty. Add some blocks first.", "Run Workflow", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // This would integrate with the main form's workflow execution
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1000, 700);
            this.Name = "WorkflowDesigner";
            this.Text = "Workflow Designer";
            this.ResumeLayout(false);
        }
    }
}