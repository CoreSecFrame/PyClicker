using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PyClickerRecorder
{
    public class Player
    {
        public async Task Play(List<RecordedAction> actions, int loopCount, double speedFactor, CancellationToken token)
        {
            try
            {
                int loopsToRun = loopCount <= 0 ? int.MaxValue : loopCount;

                for (int i = 0; i < loopsToRun; i++)
                {
                    // Check for cancellation at the start of each loop
                    token.ThrowIfCancellationRequested();

                    foreach (var action in actions)
                    {
                        // Check for cancellation before each action
                        token.ThrowIfCancellationRequested();
                        await ExecuteAction(action, speedFactor);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Playback was stopped by the user, this is expected.
            }
        }

        private async Task ExecuteAction(RecordedAction action, double speedFactor)
        {
            switch (action)
            {
                case MouseMoveAction move:
                    NativeMethods.SetCursorPos(move.Position.X, move.Position.Y);
                    break;

                case MouseClickAction click:
                    NativeMethods.SetCursorPos(click.Position.X, click.Position.Y);
                    uint flag = 0;
                    if (click.Button.ToLower() == "left")
                    {
                        flag = (click.State.ToLower() == "down") ? NativeMethods.MOUSEEVENTF_LEFTDOWN : NativeMethods.MOUSEEVENTF_LEFTUP;
                    }
                    else if (click.Button.ToLower() == "right")
                    {
                        flag = (click.State.ToLower() == "down") ? NativeMethods.MOUSEEVENTF_RIGHTDOWN : NativeMethods.MOUSEEVENTF_RIGHTUP;
                    }
                    NativeMethods.mouse_event(flag, 0, 0, 0, IntPtr.Zero);
                    break;

                case MouseScrollAction scroll:
                    NativeMethods.SetCursorPos(scroll.Position.X, scroll.Position.Y);
                    NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_WHEEL, 0, 0, (uint)scroll.Amount, IntPtr.Zero);
                    break;

                case KeyAction key:
                    if (key.State.ToLower() == "press")
                    {
                        PressModifiers(key.Modifiers, true);
                        NativeMethods.keybd_event((byte)key.Key, 0, NativeMethods.KEYEVENTF_KEYDOWN, IntPtr.Zero);
                    }
                    else // Release
                    {
                        NativeMethods.keybd_event((byte)key.Key, 0, NativeMethods.KEYEVENTF_KEYUP, IntPtr.Zero);
                        PressModifiers(key.Modifiers, false);
                    }
                    break;

                case DelayAction delay:
                    // Ensure speedFactor is not zero to avoid division errors
                    double safeSpeedFactor = speedFactor > 0 ? speedFactor : 1.0;
                    var adjustedDuration = delay.Duration / safeSpeedFactor;
                    await Task.Delay(TimeSpan.FromSeconds(adjustedDuration));
                    break;
            }
        }

        private void PressModifiers(List<string> modifiers, bool press)
        {
            uint flag = press ? NativeMethods.KEYEVENTF_KEYDOWN : NativeMethods.KEYEVENTF_KEYUP;

            if (modifiers.Contains("Control"))
            {
                NativeMethods.keybd_event((byte)Keys.ControlKey, 0, flag, IntPtr.Zero);
            }
            if (modifiers.Contains("Shift"))
            {
                NativeMethods.keybd_event((byte)Keys.ShiftKey, 0, flag, IntPtr.Zero);
            }
            if (modifiers.Contains("Alt"))
            {
                NativeMethods.keybd_event((byte)Keys.Menu, 0, flag, IntPtr.Zero); // Menu is Alt
            }
        }
    }
}
