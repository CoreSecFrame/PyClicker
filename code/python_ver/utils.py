import os
import platform
import ctypes
import sys
import tkinter as tk
from tkinter import messagebox

def get_system_info():
    """Get information about the operating system"""
    system_info = {
        "os": platform.system(),
        "release": platform.release(),
        "version": platform.version(),
        "architecture": platform.architecture(),
        "machine": platform.machine(),
        "processor": platform.processor(),
        "python_version": platform.python_version()
    }
    return system_info

def is_admin():
    """Check if the script is running with administrator privileges"""
    try:
        if os.name == 'nt':
            return ctypes.windll.shell32.IsUserAnAdmin() != 0
        else:
            return os.geteuid() == 0
    except:
        return False

def require_admin():
    """Restart the script with administrator privileges if needed (Windows only)"""
    if os.name == 'nt' and not is_admin():
        ctypes.windll.shell32.ShellExecuteW(None, "runas", sys.executable, " ".join(sys.argv), None, 1)
        sys.exit(0)

def is_module_available(module_name):
    """Check if a Python module is available"""
    try:
        __import__(module_name)
        return True
    except ImportError:
        return False

def check_required_modules():
    """Check if all required modules are available"""
    required_modules = {
        "pyautogui": "pyautogui",
        "keyboard": "keyboard",
        "PIL": "Pillow",
        "pynput": "pynput"
    }

    missing_modules = []
    for module, package in required_modules.items():
        try:
            __import__(module)
        except ImportError:
            missing_modules.append(package)
    return missing_modules

def install_missing_modules(modules):
    """Try to install missing modules using pip"""
    try:
        # Asegurarse que el script se ejecuta con permisos de admin si es Windows
        require_admin()
        for module in modules:
            print(f"Installing {module} ...")
            subprocess.check_call([sys.executable, "-m", "pip", "install", module])
        return True
    except Exception as e:
        print(f"Failed to install modules: {e}")
        return False

def center_window(window):
    """Center a tkinter window on the screen"""
    window.update_idletasks()
    width = window.winfo_width()
    height = window.winfo_height()
    x = (window.winfo_screenwidth() // 2) - (width // 2)
    y = (window.winfo_screenheight() // 2) - (height // 2)
    window.geometry('{}x{}+{}+{}'.format(width, height, x, y))

def create_tooltip(widget, text):
    """Create a tooltip for a widget"""
    def enter(event):
        x, y, _, _ = widget.bbox("insert")
        x += widget.winfo_rootx() + 25
        y += widget.winfo_rooty() + 20
        
        # Create top level window
        tooltip = tk.Toplevel(widget)
        tooltip.wm_overrideredirect(True)
        tooltip.wm_geometry(f"+{x}+{y}")
        
        # Create label
        label = tk.Label(tooltip, text=text, justify=tk.LEFT,
                          background="#ffffe0", relief=tk.SOLID, borderwidth=1,
                          font=("tahoma", "8", "normal"))
        label.pack(ipadx=1)
        
        widget.tooltip = tooltip
        
    def leave(event):
        if hasattr(widget, "tooltip"):
            widget.tooltip.destroy()
            
    widget.bind("<Enter>", enter)
    widget.bind("<Leave>", leave)

def format_time(seconds):
    """Format seconds into a human-readable time string"""
    if seconds < 60:
        return f"{seconds:.1f} seconds"
    elif seconds < 3600:
        minutes = seconds // 60
        sec = seconds % 60
        return f"{int(minutes)} min, {int(sec)} sec"
    else:
        hours = seconds // 3600
        minutes = (seconds % 3600) // 60
        sec = seconds % 60
        return f"{int(hours)} hr, {int(minutes)} min, {int(sec)} sec"

def show_about_dialog(parent):
    """Display an about dialog for the application"""
    about_text = """Python AutoClicker

A cross-platform automation tool for recording and playing back mouse and keyboard actions.

Features:
- Record mouse movements, clicks, and keyboard inputs
- Play back recordings with customizable speed
- Save and load recordings
- Export recordings as Python scripts

This software is open source and provided as-is without warranty.
"""
    messagebox.showinfo("About Python AutoClicker", about_text, parent=parent)

def show_help_dialog(parent):
    """Display a help dialog for the application"""
    help_text = """Quick Start Guide:

Recording:
1. Click "Start Recording" or press F6
2. Perform the actions you want to record
3. Click "Stop Recording" or press F6 again

Playback:
1. Click "Start Playback" or press F7
2. The recorded actions will be played back
3. To stop, click "Stop Playback" or press F7

Hotkeys:
- F6: Start/Stop recording
- F7: Start/Stop playback
- Esc: Emergency stop (stops both recording and playback)

Settings:
- Loop Count: Number of times to repeat the playback (0 for infinite)
- Delay: Time between actions (affects playback speed)

Tips:
- You can save recordings for later use
- Export to Python script creates a standalone automation script
- Right-click on actions in the list to edit or delete them
"""
    messagebox.showinfo("Help", help_text, parent=parent)