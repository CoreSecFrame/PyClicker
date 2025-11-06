#!/usr/bin/env python3
import os
import sys
import subprocess
import tkinter as tk
from tkinter import messagebox
import traceback
import platform

from utils import check_required_modules, install_missing_modules

VENV_DIR = "venv"

def is_running_in_venv():
    return os.path.basename(sys.prefix) == VENV_DIR or \
           os.path.exists(os.path.join(sys.prefix, "pyvenv.cfg"))

def create_virtualenv():
    try:
        subprocess.check_call([sys.executable, "-m", "venv", VENV_DIR])
        return True
    except Exception as e:
        print(f"Error creating virtualenv: {e}")
        return False

def install_deps_in_venv():
    python_bin = get_venv_python()
    try:
        subprocess.check_call([python_bin, "-m", "pip", "install", "--upgrade", "pip"])
        
        # Dependencias base
        modules = ['pyautogui', 'keyboard', 'Pillow', 'pynput']
        
        # Solo en Windows se incluye pywin32
        if platform.system() == "Windows":
            modules.append('pywin32')

        subprocess.check_call([python_bin, "-m", "pip", "install"] + modules)
        return True
    except Exception as e:
        print(f"Error installing modules in venv: {e}")
        return False

def get_venv_python():
    if os.name == 'nt':
        return os.path.join(VENV_DIR, "Scripts", "python.exe")
    else:
        return os.path.join(VENV_DIR, "bin", "python")

def restart_in_venv():
    python_bin = get_venv_python()
    os.execv(python_bin, [python_bin] + sys.argv)

def create_launcher_bat():
    bat_path = os.path.abspath("launch_autoclicker.bat")
    venv_pythonw = os.path.abspath(os.path.join(VENV_DIR, "Scripts", "pythonw.exe"))
    script_path = os.path.abspath("run.py")

    # Print debugging info
    print(f"Creating batch file with:")
    print(f"- pythonw path: {venv_pythonw}")
    print(f"- script path: {script_path}")
    
    if not os.path.exists(venv_pythonw):
        print(f"WARNING: pythonw not found at {venv_pythonw}")
        # Try fallback to python.exe if pythonw.exe doesn't exist
        venv_pythonw = os.path.abspath(os.path.join(VENV_DIR, "Scripts", "python.exe"))
        print(f"Falling back to: {venv_pythonw}")

    bat_content = f'''@echo off
start "" "{venv_pythonw}" "{script_path}"
exit
'''

    with open(bat_path, "w") as f:
        f.write(bat_content)

    print(f"Batch file created at: {bat_path}")
    return bat_path

def create_windows_shortcut():
    """Create a Windows shortcut on the desktop"""
    try:
        # Import the win32com modules directly - they should already be installed at this point
        import win32com.client
        from win32com.shell import shell, shellcon
        
        # Get desktop path
        desktop = shell.SHGetFolderPath(0, shellcon.CSIDL_DESKTOP, None, 0)
        shortcut_path = os.path.join(desktop, "AutoClicker.lnk")
        
        # Show what we're doing
        print(f"Creating shortcut at: {shortcut_path}")

        if os.path.exists(shortcut_path):
            print("Shortcut already exists, skipping creation.")
            return False

        # Create bat file first
        bat_path = create_launcher_bat()
        working_dir = os.path.abspath(".")
        
        # Check if icon exists
        icon_path = os.path.abspath("icon.ico") 
        if not os.path.exists(icon_path):
            print(f"Warning: Icon file {icon_path} not found.")
            icon_path = ""  # Use default icon if file doesn't exist

        # Create shortcut
        shell_object = win32com.client.Dispatch("WScript.Shell")
        shortcut = shell_object.CreateShortcut(shortcut_path)
        shortcut.TargetPath = bat_path
        shortcut.WorkingDirectory = working_dir
        if icon_path:
            shortcut.IconLocation = icon_path
        shortcut.WindowStyle = 7  # Minimized
        shortcut.Description = "AutoClicker launcher"
        shortcut.Save()

        print(f"Shortcut successfully created at {shortcut_path}")
        return True
    except Exception as e:
        error_msg = f"Error creating shortcut: {e}\n{traceback.format_exc()}"
        print(error_msg)
        messagebox.showerror("Shortcut Creation Error", error_msg)
        return False

def create_linux_shortcut():
    desktop = os.path.join(os.path.expanduser("~"), "Desktop")
    if not os.path.isdir(desktop):
        desktop = os.path.expanduser("~")

    shortcut_path = os.path.join(desktop, "AutoClicker.desktop")

    if os.path.exists(shortcut_path):
        print("Shortcut already exists, skipping creation.")
        return False

    python_path = os.path.abspath(get_venv_python())
    script_path = os.path.abspath("run.py")
    icon_path = os.path.abspath("icon.png")

    display = os.environ.get("DISPLAY", ":0")
    xauthority = os.environ.get("XAUTHORITY", os.path.expanduser("~/.Xauthority"))

    content = f"""[Desktop Entry]
Type=Application
Name=AutoClicker
Exec=pkexec env DISPLAY={display} XAUTHORITY={xauthority} {python_path} {script_path}
Icon={icon_path}
Terminal=false
"""


    with open(shortcut_path, "w") as f:
        f.write(content)

    os.chmod(shortcut_path, 0o755)
    print(f"Shortcut created at {shortcut_path}")
    return True

def create_shortcut():
    if os.name == "nt":
        return create_windows_shortcut()
    elif os.name == "posix":
        return create_linux_shortcut()
    return False



def main():
    root = tk.Tk()
    root.withdraw()

    icon_path_win = os.path.abspath("icon.ico")
    icon_path_png = os.path.abspath("icon.png")

    # Check if icon files exist and provide debug information
    if os.name == 'nt':
        if not os.path.exists(icon_path_win):
            print(f"Warning: Windows icon file not found: {icon_path_win}")
    if not os.path.exists(icon_path_png):
        print(f"Warning: PNG icon file not found: {icon_path_png}")

    try:
        if os.name == 'nt' and os.path.exists(icon_path_win):
            root.iconbitmap(icon_path_win)
        elif os.path.exists(icon_path_png):
            icon = tk.PhotoImage(file=icon_path_png)
            root.iconphoto(False, icon)
    except Exception as e:
        print(f"Could not load icon: {e}")

    # First, ensure we're in the virtual environment
    if not is_running_in_venv():
        print("Not running in virtual environment...")

        if not os.path.isdir(VENV_DIR):
            msg = ("No virtual environment found.\n\n"
                   "The application will create one now and install dependencies.\n\n"
                   "Continue?")
            if not messagebox.askyesno("Create Virtual Environment", msg):
                sys.exit(1)
            if not create_virtualenv():
                messagebox.showerror("Error", "Failed to create virtual environment.")
                sys.exit(1)

        if not install_deps_in_venv():
            messagebox.showerror("Error", "Failed to install dependencies in virtual environment.")
            sys.exit(1)

        # We need to restart in the virtual environment
        messagebox.showinfo("Restarting", "Restarting application inside virtual environment.")
        restart_in_venv()
        sys.exit(0)

    # Now we're running in the virtual environment
    print("Running in virtual environment...")

    # Check for necessary modules first
    missing_modules = check_required_modules()
    if missing_modules:
        msg = f"The following required modules are missing:\n\n{', '.join(missing_modules)}\n\nWould you like to install them now?"
        if messagebox.askyesno("Missing Dependencies", msg):
            if not install_missing_modules(missing_modules):
                messagebox.showerror("Installation Failed",
                                     "Failed to install required modules manually, please use pip.")
                sys.exit(1)
            else:
                messagebox.showinfo("Installation Successful", "Modules installed successfully. Restarting...")
                os.execv(sys.executable, [sys.executable] + sys.argv)
        else:
            sys.exit(1)

    # Check if pywin32 is required and install it if needed (Windows only)
    if os.name == "nt":
        try:
            import win32com.client
            print("pywin32 is already installed.")
        except ImportError:
            print("pywin32 is not installed. Installing...")
            try:
                subprocess.check_call([sys.executable, "-m", "pip", "install", "pywin32"])
                messagebox.showinfo("Module Installed",
                                    "pywin32 was installed. The application will now restart.")
                os.execv(sys.executable, [sys.executable] + sys.argv)
            except Exception as e:
                print(f"Error installing pywin32: {e}")
                messagebox.showerror("Installation Error", f"Failed to install pywin32: {e}")
                sys.exit(1)

    # Check if shortcut already exists before asking
    shortcut_exists = False
    if os.name == "nt":
        try:
            from win32com.shell import shell, shellcon
            desktop = shell.SHGetFolderPath(0, shellcon.CSIDL_DESKTOP, None, 0)
            shortcut_path = os.path.join(desktop, "AutoClicker.lnk")
            shortcut_exists = os.path.exists(shortcut_path)
        except Exception as e:
            print(f"Error checking shortcut existence: {e}")
    elif os.name == "posix":
        desktop = os.path.join(os.path.expanduser("~"), "Desktop")
        shortcut_path = os.path.join(desktop, "AutoClicker.desktop")
        shortcut_exists = os.path.exists(shortcut_path)

    if not shortcut_exists:
        if messagebox.askyesno("Create Shortcut", "Would you like to create a desktop shortcut?"):
            try:
                print("Attempting to create desktop shortcut...")
                created = create_shortcut()
                if created:
                    messagebox.showinfo("Shortcut Created", "A desktop shortcut has been created for you.")
                else:
                    print("Shortcut was not created or already existed.")
            except Exception as e:
                error_msg = f"Failed to create shortcut: {e}\n{traceback.format_exc()}"
                print(error_msg)
                messagebox.showerror("Shortcut Creation Error", error_msg)
    else:
        print("Shortcut already exists. Skipping creation prompt.")

    # Continue with the main application
    root.deiconify()

    try:
        from main import AutoClickerApp, Recorder, Player

        print("Successfully loaded all modules and components")

        app = AutoClickerApp(root)
        app.recorder = Recorder(app)
        app.player = Player(app)

        app.start_recording = app.recorder.start_recording
        app.stop_recording = app.recorder.stop_recording
        app.start_playback = app.player.start_playback
        app.stop_playback = app.player.stop_playback

        root.mainloop()
    except Exception as e:
        err = traceback.format_exc()
        messagebox.showerror("Application Error", f"An unexpected error occurred:\n\n{e}\n\n{err}")
        print(err)
        sys.exit(1)


if __name__ == "__main__":
    main()