using System;
using System.Runtime.InteropServices;

namespace PyClickerRecorder
{
    /// <summary>
    /// Contains P/Invoke definitions for Win32 API functions.
    /// </summary>
    internal static class NativeMethods
    {
        // For simulating mouse movements and clicks
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);

        // Mouse event flags
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const uint MOUSEEVENTF_WHEEL = 0x0800;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        // For simulating keyboard presses
        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);

        // Keyboard event flags
        public const uint KEYEVENTF_KEYDOWN = 0x0000;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        // For mapping characters to virtual key codes
        [DllImport("user32.dll")]
        public static extern short VkKeyScan(char ch);

        // For getting the active window
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        // For getting window text
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowTextW(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        public static string? GetWindowText(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return null;

            const int maxLength = 256;
            var sb = new System.Text.StringBuilder(maxLength);
            int length = GetWindowTextW(hWnd, sb, maxLength);
            return length > 0 ? sb.ToString() : null;
        }

        // For enumerating windows
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // For checking if window is visible
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        // For checking if window has a parent (to exclude child windows)
        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        // For getting window class name
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetClassNameW(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        public static string? GetWindowClassName(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return null;

            const int maxLength = 256;
            var sb = new System.Text.StringBuilder(maxLength);
            int length = GetClassNameW(hWnd, sb, maxLength);
            return length > 0 ? sb.ToString() : null;
        }

        // For getting the process ID of a window
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    }
}
