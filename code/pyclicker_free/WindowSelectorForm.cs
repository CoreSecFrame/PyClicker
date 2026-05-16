using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PyClickerRecorder
{
    public partial class WindowSelectorForm : Form
    {
        private ListView _windowsListView;
        private Button _okButton;
        private Button _cancelButton;
        private Button _refreshButton;
        private List<WindowInfo> _windows;

        public string? SelectedWindowTitle { get; private set; }

        public WindowSelectorForm()
        {
            _windows = new List<WindowInfo>();
            InitializeComponent();
            RefreshWindowsList();
        }

        private void InitializeComponent()
        {
            this.Text = "Select Window";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create ListView for windows
            _windowsListView = new ListView
            {
                Location = new Point(12, 12),
                Size = new Size(560, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };

            _windowsListView.Columns.Add("Window Title", 300);
            _windowsListView.Columns.Add("Process", 120);
            _windowsListView.Columns.Add("Class Name", 120);
            _windowsListView.DoubleClick += WindowsListView_DoubleClick;
            _windowsListView.SelectedIndexChanged += WindowsListView_SelectedIndexChanged;

            this.Controls.Add(_windowsListView);

            // Create buttons
            _refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(12, 325),
                Size = new Size(80, 30)
            };
            _refreshButton.Click += RefreshButton_Click;
            this.Controls.Add(_refreshButton);

            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(412, 325),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK,
                Enabled = false
            };
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(498, 325),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_cancelButton);

            // Set default and cancel buttons
            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void RefreshWindowsList()
        {
            _windows.Clear();
            _windowsListView.Items.Clear();

            // Enumerate all windows
            NativeMethods.EnumWindows(EnumWindowCallback, IntPtr.Zero);

            // Sort by window title
            _windows = _windows.OrderBy(w => w.Title).ToList();

            // Populate ListView
            foreach (var window in _windows)
            {
                var item = new ListViewItem(window.Title);
                item.SubItems.Add(window.ProcessName);
                item.SubItems.Add(window.ClassName);
                item.Tag = window;
                _windowsListView.Items.Add(item);
            }

            // Adjust column widths
            _windowsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private bool EnumWindowCallback(IntPtr hWnd, IntPtr lParam)
        {
            // Skip if window is not visible
            if (!NativeMethods.IsWindowVisible(hWnd))
                return true;

            // Skip child windows
            if (NativeMethods.GetParent(hWnd) != IntPtr.Zero)
                return true;

            // Get window title
            string? title = NativeMethods.GetWindowText(hWnd);
            if (string.IsNullOrEmpty(title))
                return true;

            // Skip certain system windows
            string? className = NativeMethods.GetWindowClassName(hWnd);
            if (IsSystemWindow(className, title))
                return true;

            // Get process information
            string processName = "Unknown";
            try
            {
                NativeMethods.GetWindowThreadProcessId(hWnd, out uint processId);
                using var process = Process.GetProcessById((int)processId);
                processName = process.ProcessName;
            }
            catch
            {
                // Ignore if we can't get process info
            }

            _windows.Add(new WindowInfo
            {
                Handle = hWnd,
                Title = title,
                ClassName = className ?? "Unknown",
                ProcessName = processName
            });

            return true;
        }

        private bool IsSystemWindow(string? className, string title)
        {
            if (string.IsNullOrEmpty(className))
                return false;

            // Skip common system windows and hidden windows
            string[] systemClasses = {
                "Shell_TrayWnd",           // Taskbar
                "DV2ControlHost",          // Desktop
                "WorkerW",                 // Desktop worker
                "Progman",                 // Program Manager
                "Button",                  // Various system buttons
                "Static",                  // Static controls
                "IME",                     // Input Method Editor
                "MSCTFIME UI",            // IME UI
                "ApplicationFrameWindow"   // UWP app frames (we want the content, not frame)
            };

            if (systemClasses.Any(sc => className.Contains(sc, StringComparison.OrdinalIgnoreCase)))
                return true;

            // Skip windows with certain titles
            string[] skipTitles = {
                "Default IME",
                "MSCTFIME UI",
                "GDI+ Window",
                "Hidden Window"
            };

            if (skipTitles.Any(st => title.Contains(st, StringComparison.OrdinalIgnoreCase)))
                return true;

            return false;
        }

        private void WindowsListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _okButton.Enabled = _windowsListView.SelectedItems.Count > 0;
        }

        private void WindowsListView_DoubleClick(object? sender, EventArgs e)
        {
            if (_windowsListView.SelectedItems.Count > 0)
            {
                SelectWindow();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            RefreshWindowsList();
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            SelectWindow();
        }

        private void SelectWindow()
        {
            if (_windowsListView.SelectedItems.Count > 0)
            {
                var selectedWindow = (WindowInfo)_windowsListView.SelectedItems[0].Tag;
                SelectedWindowTitle = selectedWindow.Title;
            }
        }
    }

    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string ProcessName { get; set; } = "";
    }
}