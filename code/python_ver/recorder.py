import time
import threading
import pyautogui
import keyboard
import json
from datetime import datetime
from pynput import mouse
from pynput.mouse import Listener as MouseListener


class ActionType:
    MOUSE_MOVE = "mouse_move"
    MOUSE_CLICK = "mouse_click"
    MOUSE_SCROLL = "mouse_scroll"
    KEY_PRESS = "key_press"
    KEY_RELEASE = "key_release"
    DELAY = "delay"

class Recorder:
    def __init__(self, app):
        self.app = app
        self.recording = False
        self.actions = []
        self.start_time = 0
        self.last_position = None
        self.recording_thread = None
        self.hotkey_thread = None
        self.stop_hotkey = "f6"  # Default hotkey to stop recording
        self.emergency_stop = "esc"  # Emergency stop key
        self.mouse_move_min_distance = 0.5  # Minimum pixel distance to record mouse movement
        self.mouse_move_min_time = 0.05  # Minimum time between recorded mouse movements
        self.last_recorded_time = 0
        
    def start_recording(self):
        """Start recording mouse and keyboard actions"""
        if self.recording:
            return
        
        self.recording = True
        self.actions = []
        self.start_time = time.time()
        self.last_position = pyautogui.position()
        self.last_recorded_time = time.time()
        
        # Start recording thread
        self.recording_thread = threading.Thread(target=self._record_loop)
        self.recording_thread.daemon = True
        self.recording_thread.start()
        
        # Start hotkey listener
        keyboard.add_hotkey(self.stop_hotkey, self.stop_recording)
        keyboard.add_hotkey(self.emergency_stop, self.stop_recording)
        
    def stop_recording(self):
        """Stop recording mouse and keyboard actions"""
        if not self.recording:
            return
            
        self.recording = False
        
        # Remove hotkeys
        try:
            keyboard.remove_hotkey(self.stop_hotkey)
            keyboard.remove_hotkey(self.emergency_stop)
        except:
            pass
            
        # Update UI in main thread
        if self.app.root:
            self.app.root.after(0, self._update_ui_after_stop)
    
    def _update_ui_after_stop(self):
        """Update UI after recording stops"""
        self.app.is_recording = False
        self.app.record_button.config(text="Start Recording")
        self.app.status_var.set("Recording stopped")
        self.app.recorded_actions = self.actions
        
        # Update actions listbox
        self._update_actions_listbox()
    
    def _update_actions_listbox(self):
        """Update the actions listbox with recorded actions"""
        self.app.actions_listbox.delete(0, "end")
        for i, action in enumerate(self.actions):
            action_type = action["type"]
            
            if action_type == ActionType.MOUSE_MOVE:
                x, y = action["position"]
                desc = f"Mouse Move: ({x}, {y})"
            elif action_type == ActionType.MOUSE_CLICK:
                x, y = action["position"]
                button = action["button"]
                state = "Down" if action["state"] == "down" else "Up"
                desc = f"Mouse {button.capitalize()} {state}: ({x}, {y})"
            elif action_type == ActionType.MOUSE_SCROLL:
                x, y = action["position"]
                desc = f"Mouse Scroll: {action['amount']} at ({x}, {y})"
            elif action_type == ActionType.KEY_PRESS:
                desc = f"Key Press: {action['key']}"
            elif action_type == ActionType.KEY_RELEASE:
                desc = f"Key Release: {action['key']}"
            elif action_type == ActionType.DELAY:
                desc = f"Delay: {action['duration']:.2f} seconds"
            else:
                desc = f"Unknown Action: {action_type}"
                
            self.app.actions_listbox.insert("end", desc)
    
    def _record_loop(self):
        """Main recording loop"""
        # Set up mouse and keyboard listeners
        self._setup_listeners()
        
        # Record mouse movements in a loop
        while self.recording:
            current_pos = pyautogui.position()
            current_time = time.time()
            
            # Record mouse movement if it's moved enough
            if (abs(current_pos[0] - self.last_position[0]) > self.mouse_move_min_distance or
                abs(current_pos[1] - self.last_position[1]) > self.mouse_move_min_distance) and \
               (current_time - self.last_recorded_time >= self.mouse_move_min_time):
                
                # Add delay since last action
                if self.actions and self.last_recorded_time > 0:
                    delay = current_time - self.last_recorded_time
                    if delay > 0.05:  # Only record delays greater than 50ms
                        self._add_delay(delay)
                
                # Record mouse position
                self._add_mouse_move(current_pos)
                self.last_position = current_pos
                self.last_recorded_time = current_time
            
            # Sleep to reduce CPU usage
            time.sleep(0.01)
    
    def _on_mouse_click(self, x, y, button, pressed):
        """Hook para clics del mouse reales"""
        if not self.recording:
            return

        current_time = time.time()
        if self.actions and self.last_recorded_time > 0:
            delay = current_time - self.last_recorded_time
            if delay > 0.05:
                self._add_delay(delay)

        state = "down" if pressed else "up"
        self._add_mouse_click((x, y), button.name, state)
        self.last_recorded_time = current_time

    def _on_mouse_scroll(self, x, y, dx, dy):
        """Hook para scroll del mouse real"""
        if not self.recording:
            return

        current_time = time.time()
        if self.actions and self.last_recorded_time > 0:
            delay = current_time - self.last_recorded_time
            if delay > 0.05:
                self._add_delay(delay)

        self._add_mouse_scroll((x, y), dy)
        self.last_recorded_time = current_time


    def _setup_listeners(self):
        """Set up mouse and keyboard event listeners using pynput"""
        # Inicia el listener de mouse
        self.mouse_listener = MouseListener(
            on_click=self._on_mouse_click,
            on_scroll=self._on_mouse_scroll
        )
        self.mouse_listener.start()

        # Teclado: mantiene el hook existente
        keyboard.hook(self._keyboard_hook)

    
    def _teardown_listeners(self):
        """Remove all hooks and restore original functions"""
        # Detener listener de mouse si existe
        if hasattr(self, "mouse_listener"):
            self.mouse_listener.stop()
        
        # Quitar hooks de teclado
        keyboard.unhook_all()

    
    def _mouse_down_hook(self, x=None, y=None, button='left'):
        """Hook for mouse down events"""
        if not self.recording:
            return self._original_mouseDown(x, y, button)
        
        current_time = time.time()
        position = pyautogui.position() if x is None or y is None else (x, y)
        
        # Add delay if needed
        if self.actions and self.last_recorded_time > 0:
            delay = current_time - self.last_recorded_time
            if delay > 0.05:
                self._add_delay(delay)
        
        # Record mouse down
        self._add_mouse_click(position, button, "down")
        self.last_recorded_time = current_time
        
        # Call original function
        return self._original_mouseDown(x, y, button)
    
    def _mouse_up_hook(self, x=None, y=None, button='left'):
        """Hook for mouse up events"""
        if not self.recording:
            return self._original_mouseUp(x, y, button)
        
        current_time = time.time()
        position = pyautogui.position() if x is None or y is None else (x, y)
        
        # Add delay if needed
        if self.actions and self.last_recorded_time > 0:
            delay = current_time - self.last_recorded_time
            if delay > 0.05:
                self._add_delay(delay)
        
        # Record mouse up
        self._add_mouse_click(position, button, "up")
        self.last_recorded_time = current_time
        
        # Call original function
        return self._original_mouseUp(x, y, button)
    
    def _click_hook(self, x=None, y=None, clicks=1, interval=0.0, button='left'):
        """Hook for mouse click events"""
        if not self.recording:
            return self._original_click(x, y, clicks, interval, button)
        
        position = pyautogui.position() if x is None or y is None else (x, y)
        current_time = time.time()
        
        # Add delay if needed
        if self.actions and self.last_recorded_time > 0:
            delay = current_time - self.last_recorded_time
            if delay > 0.05:
                self._add_delay(delay)
        
        # Record mouse down and up for each click
        for i in range(clicks):
            if i > 0 and interval > 0:
                self._add_delay(interval)
            
            self._add_mouse_click(position, button, "down")
            self._add_mouse_click(position, button, "up")
        
        self.last_recorded_time = time.time()
        
        # Call original function
        return self._original_click(x, y, clicks, interval, button)
    
    def _scroll_hook(self, clicks, x=None, y=None):
        """Hook for mouse scroll events"""
        if not self.recording:
            return self._original_scroll(clicks, x, y)
        
        position = pyautogui.position() if x is None or y is None else (x, y)
        current_time = time.time()
        
        # Add delay if needed
        if self.actions and self.last_recorded_time > 0:
            delay = current_time - self.last_recorded_time
            if delay > 0.05:
                self._add_delay(delay)
        
        # Record scroll
        self._add_mouse_scroll(position, clicks)
        self.last_recorded_time = current_time
        
        # Call original function
        return self._original_scroll(clicks, x, y)
    
    def _keyboard_hook(self, event):
        """Hook for keyboard events"""
        if not self.recording:
            return
        
        # Skip recording of our own hotkeys
        if event.name in [self.stop_hotkey, self.emergency_stop]:
            return
        
        current_time = time.time()
        
        # Add delay if needed
        if self.actions and self.last_recorded_time > 0:
            delay = current_time - self.last_recorded_time
            if delay > 0.05:
                self._add_delay(delay)
        
        # Record key press or release
        if event.event_type == keyboard.KEY_DOWN:
            self._add_key_press(event.name)

        
        self.last_recorded_time = current_time
    
    def _add_mouse_move(self, position):
        """Add a mouse move action"""
        action = {
            "type": ActionType.MOUSE_MOVE,
            "time": time.time() - self.start_time,
            "position": position
        }
        self.actions.append(action)
    
    def _add_mouse_click(self, position, button, state):
        """Add a mouse click action"""
        action = {
            "type": ActionType.MOUSE_CLICK,
            "time": time.time() - self.start_time,
            "position": position,
            "button": button,
            "state": state
        }
        self.actions.append(action)
    
    def _add_mouse_scroll(self, position, amount):
        """Add a mouse scroll action"""
        action = {
            "type": ActionType.MOUSE_SCROLL,
            "time": time.time() - self.start_time,
            "position": position,
            "amount": amount
        }
        self.actions.append(action)
    
    def _add_key_press(self, key):
        """Add a keyboard press action"""
        action = {
            "type": ActionType.KEY_PRESS,
            "time": time.time() - self.start_time,
            "key": key
        }
        self.actions.append(action)
    
    def _add_key_release(self, key):
        """Add a keyboard release action"""
        action = {
            "type": ActionType.KEY_RELEASE,
            "time": time.time() - self.start_time,
            "key": key
        }
        self.actions.append(action)
    
    def _add_delay(self, duration):
        """Add a delay action"""
        action = {
            "type": ActionType.DELAY,
            "time": time.time() - self.start_time,
            "duration": duration
        }
        self.actions.append(action)
    
    def get_actions(self):
        """Get the recorded actions"""
        return self.actions
    
    def set_actions(self, actions):
        """Set actions from loaded file"""
        self.actions = actions
        if self.app:
            self._update_actions_listbox()