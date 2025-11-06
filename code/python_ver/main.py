import tkinter as tk
from tkinter import ttk, filedialog, messagebox
import json
import os
import keyboard
import threading
import time
from utils import center_window, create_tooltip, show_about_dialog, show_help_dialog
from recorder import Recorder
from player import Player

class AutoClickerApp:
    def __init__(self, root):
        self.root = root
        self.root.title("PyClickerRecorder")
        self.root.geometry("650x500")
        center_window(self.root)
        
        # Application state
        self.is_recording = False
        self.is_playing = False
        self.recorded_actions = []
        self.current_file = None
        self.loop_count = tk.IntVar(value=1)
        self.delay_between_actions = tk.DoubleVar(value=1.0)
        self.infinite_loop_var = tk.BooleanVar(value=False)
        
        # Initialize components
        self.create_gui()
        self.recorder = Recorder(self)
        self.player = Player(self)
        
        self.last_record_press = 0
        self.last_play_press = 0
        self.debounce_delay = 0.3
        self.setup_hotkeys()
    
        # Setup closing handler
        self.root.protocol("WM_DELETE_WINDOW", self.on_close)
        
        # Link speed factor update
        self.delay_between_actions.trace_add("write", self.update_speed_label)

    def create_gui(self):
        # Create main menu
        self.create_main_menu()
        
        # Create notebook for tabs
        notebook = ttk.Notebook(self.root)
        notebook.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        # Create tabs
        self.create_record_tab(ttk.Frame(notebook))
        self.create_settings_tab(ttk.Frame(notebook))
        self.create_help_tab(ttk.Frame(notebook))
        
        notebook.add(self.record_tab, text="Record & Play")
        notebook.add(self.settings_tab, text="Settings")
        
        # Status bar
        self.status_var = tk.StringVar(value="Ready")
        status_bar = ttk.Label(self.root, textvariable=self.status_var, relief=tk.SUNKEN, anchor=tk.W)
        status_bar.pack(side=tk.BOTTOM, fill=tk.X)

    def create_main_menu(self):
        menu_bar = tk.Menu(self.root)
        
        # File menu
        file_menu = tk.Menu(menu_bar, tearoff=0)
        file_menu.add_command(label="New", command=self.clear_actions)
        file_menu.add_command(label="Save", command=self.save_macro)
        file_menu.add_command(label="Load", command=self.load_macro)
        file_menu.add_separator()
        file_menu.add_command(label="Exit", command=self.on_close)
        
        # Edit menu
        edit_menu = tk.Menu(menu_bar, tearoff=0)
        edit_menu.add_command(label="Clear Actions", command=self.clear_actions)
        edit_menu.add_command(label="Insert Delay", command=self.insert_delay)
        
        # Help menu
        help_menu = tk.Menu(menu_bar, tearoff=0)
        help_menu.add_command(label="Help", command=lambda: show_help_dialog(self.root))
        help_menu.add_command(label="About", command=lambda: show_about_dialog(self.root))
        
        menu_bar.add_cascade(label="File", menu=file_menu)
        menu_bar.add_cascade(label="Edit", menu=edit_menu)
        menu_bar.add_cascade(label="Help", menu=help_menu)
        
        self.root.config(menu=menu_bar)

    def create_record_tab(self, parent):
        self.record_tab = parent
        
        # Control frame
        control_frame = ttk.LabelFrame(parent, text="Controls")
        control_frame.pack(fill=tk.X, padx=10, pady=10)
        
        # Record/Play buttons
        self.record_button = ttk.Button(control_frame, text="Start Recording (F6)", command=self.toggle_recording)
        self.record_button.grid(row=0, column=0, padx=5, pady=5)
        create_tooltip(self.record_button, "Record mouse and keyboard actions")
        
        self.play_button = ttk.Button(control_frame, text="Start Playback (F7)", command=self.toggle_playback)
        self.play_button.grid(row=0, column=1, padx=5, pady=5)
        create_tooltip(self.play_button, "Play recorded actions")
        
        # Actions list
        actions_frame = ttk.LabelFrame(parent, text="Recorded Actions")
        actions_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        scrollbar = ttk.Scrollbar(actions_frame)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        self.actions_listbox = tk.Listbox(actions_frame, yscrollcommand=scrollbar.set)
        self.actions_listbox.pack(fill=tk.BOTH, expand=True)
        scrollbar.config(command=self.actions_listbox.yview)
        
        # Context menu
        self.context_menu = tk.Menu(self.root, tearoff=0)
        self.context_menu.add_command(label="Delete", command=self.delete_selected_action)
        self.context_menu.add_command(label="Insert Delay", command=self.insert_delay)
        self.actions_listbox.bind("<Button-3>", self.show_context_menu)

    def create_settings_tab(self, parent):
        self.settings_tab = parent
        
        # Playback settings
        loop_frame = ttk.LabelFrame(parent, text="Playback Settings")
        loop_frame.pack(fill=tk.X, padx=10, pady=10)
        
        ttk.Label(loop_frame, text="Loop Count:").grid(row=0, column=0, padx=5, pady=5)
        loop_entry = ttk.Entry(loop_frame, textvariable=self.loop_count, width=5)
        loop_entry.grid(row=0, column=1, padx=5, pady=5)
        create_tooltip(loop_entry, "Number of repetitions (0 = infinite)")
        
        ttk.Checkbutton(loop_frame, text="Infinite Loop", variable=self.infinite_loop_var,
                      command=self.toggle_infinite_loop).grid(row=0, column=2, padx=5, pady=5)
        
        # Speed settings
        speed_frame = ttk.LabelFrame(parent, text="Playback Speed")
        speed_frame.pack(fill=tk.X, padx=10, pady=10)
        
        ttk.Label(speed_frame, text="Speed Factor:").grid(row=0, column=0, padx=5, pady=5)
        speed_scale = ttk.Scale(speed_frame, from_=0.1, to=2.0, variable=self.delay_between_actions, length=200)
        speed_scale.grid(row=0, column=1, padx=5, pady=5)
        self.speed_label = ttk.Label(speed_frame, text="1.0x")
        self.speed_label.grid(row=0, column=2, padx=5, pady=5)

    def create_help_tab(self, parent):
        self.help_tab = parent
        help_text = """PyClickerRecorder - Quick Guide

Recording:
1. Press F6 to start/stop recording
2. Perform mouse/keyboard actions
3. Actions will appear in the list

Playback:
1. Press F7 to start/stop playback
2. Adjust loop count for repetitions
3. Use speed slider to control playback speed

Editing:
- Right-click actions to:
  • Delete selected action
  • Insert custom delay
- Click on edit tool bar option to clear al records
- Use File menu to save/load macros

Hotkeys:
- F6: Toggle recording
- F7: Toggle playback
- Esc: Emergency stop"""

        text_widget = tk.Text(parent, wrap=tk.WORD, bg=self.root.cget("bg"), relief=tk.FLAT)
        text_widget.insert(tk.END, help_text)
        text_widget.config(state=tk.DISABLED)
        text_widget.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

    def setup_hotkeys(self):
        keyboard.hook_key('f6', self.handle_f6, suppress=True)
        keyboard.hook_key('f7', self.handle_f7, suppress=True)
        keyboard.add_hotkey('esc', self.stop_all, suppress=True)

    def handle_f6(self, event):
        if event.event_type == 'down':
            current_time = time.time()
            if current_time - self.last_record_press > self.debounce_delay:
                self.last_record_press = current_time
                self.toggle_recording()

    def handle_f7(self, event):
        if event.event_type == 'down':
            current_time = time.time()
            if current_time - self.last_play_press > self.debounce_delay:
                self.last_play_press = current_time
                self.toggle_playback()

    def toggle_recording(self):
        if self.is_recording:
            # stop recording
            self.recorder.stop_recording()
            self.is_recording = False
            self.record_button.config(text="Start Recording (F6)")
            self.status_var.set("Recording stopped")
            self.update_actions_list()
        else:
            # start recording
            if self.is_playing:
                self.toggle_playback()  # Detener playback si está activo
            self.is_recording = True
            self.record_button.config(text="Stop Recording (F6)")
            self.status_var.set("Recording... Press F6 to stop")
            self.recorder.start_recording()


    def toggle_playback(self):
        if not self.is_playing:
            # Iniciar reproducción
            if not self.recorded_actions:
                messagebox.showinfo("No Actions", "Record some actions first!")
                return
            self.is_playing = True
            self.play_button.config(text="Stop Playback (F7)")
            self.status_var.set("Playing... Press F7 to stop")
            self.player.start_playback()
        else:
            # Detener reproducción
            self.player.stop_playback()
            self.is_playing = False
            self.play_button.config(text="Start Playback (F7)")
            self.status_var.set("Playback stopped")

    def on_close(self):
        keyboard.unhook_all()  # Limpiar todas las hotkeys
        self.root.destroy()

    def update_speed_label(self, *args):
        self.speed_label.config(text=f"{self.delay_between_actions.get():.1f}x")

    def toggle_infinite_loop(self):
        if self.infinite_loop_var.get():
            self.loop_count.set(0)
        else:
            self.loop_count.set(1)

    def show_context_menu(self, event):
        try:
            index = self.actions_listbox.nearest(event.y)
            self.actions_listbox.selection_clear(0, tk.END)
            self.actions_listbox.selection_set(index)
            self.context_menu.tk_popup(event.x_root, event.y_root)
        finally:
            self.context_menu.grab_release()

    def save_macro(self):
        filename = filedialog.asksaveasfilename(
            defaultextension=".json",
            filetypes=[("JSON files", "*.json"), ("All files", "*.*")]
        )
        if filename:
            try:
                with open(filename, 'w') as f:
                    json.dump(self.recorded_actions, f, indent=2)
                self.status_var.set(f"Saved: {os.path.basename(filename)}")
            except Exception as e:
                messagebox.showerror("Save Error", str(e))

    def load_macro(self):
        filename = filedialog.askopenfilename(
            filetypes=[("JSON files", "*.json"), ("All files", "*.*")]
        )
        if filename:
            try:
                with open(filename, 'r') as f:
                    self.recorded_actions = json.load(f)
                self.update_actions_list()
                self.status_var.set(f"Loaded: {os.path.basename(filename)}")
            except Exception as e:
                messagebox.showerror("Load Error", str(e))

    def clear_actions(self):
        if messagebox.askyesno("Clear Actions", "Clear all recorded actions?"):
            self.recorded_actions = []
            self.actions_listbox.delete(0, tk.END)
            self.status_var.set("Actions cleared")

    def update_actions_list(self):
        self.actions_listbox.delete(0, tk.END)
        for action in self.recorded_actions:
            self.actions_listbox.insert(tk.END, self.format_action(action))

    def format_action(self, action):
        action_type = action.get("type", "")
        if action_type == "delay":
            return f"Delay: {action.get('duration', 0):.2f}s"
        elif action_type == "mouse_move":
            x, y = action.get("position", (0, 0))
            return f"Move to ({x}, {y})"
        elif action_type == "mouse_click":
            btn = action.get("button", "left").capitalize()
            state = "Press" if action.get("state") == "down" else "Release"
            return f"Mouse {btn} {state}"
        elif action_type == "mouse_scroll":
            return f"Scroll: {action.get('amount', 0)}"
        elif action_type == "key_press":
            return f"Key Press: {action.get('key', '')}"
        elif action_type == "key_release":
            return f"Key Release: {action.get('key', '')}"
        return "Unknown Action"

    def delete_selected_action(self):
        selected = self.actions_listbox.curselection()
        if selected:
            index = selected[0]
            self.recorded_actions.pop(index)
            self.actions_listbox.delete(index)
            self.status_var.set(f"Action {index+1} deleted")

    def insert_delay(self):
        selected = self.actions_listbox.curselection()
        index = selected[0] if selected else len(self.recorded_actions)
        
        dialog = tk.Toplevel(self.root)
        dialog.title("Insert Delay")
        dialog.geometry("250x100")
        ttk.Label(dialog, text="Delay (seconds):").pack(pady=5)
        
        delay_var = tk.DoubleVar(value=1.0)
        ttk.Entry(dialog, textvariable=delay_var).pack(pady=5)
        
        def add_delay():
            duration = delay_var.get()
            self.recorded_actions.insert(index, {
                "type": "delay",
                "duration": duration,
                "time": 0
            })
            self.actions_listbox.insert(index, f"Delay: {duration:.2f}s")
            dialog.destroy()
        
        ttk.Button(dialog, text="Add", command=add_delay).pack(pady=5)

    def stop_all(self):
        if self.is_recording: self.toggle_recording()
        if self.is_playing: self.toggle_playback()

    def on_close(self):
        self.stop_all()
        keyboard.clear_all_hotkeys()
        self.root.destroy()

def main():
    root = tk.Tk()
    app = AutoClickerApp(root)
    root.mainloop()

if __name__ == "__main__":
    main()