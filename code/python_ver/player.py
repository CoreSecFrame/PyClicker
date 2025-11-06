import time
import threading
import pyautogui
import keyboard
from recorder import ActionType

class Player:
    def __init__(self, app):
        self.app = app
        self.playing = False
        self.thread = None
        self.stop_requested = False
        self.stop_hotkey = "f7"  # Default hotkey to stop playback
        self.emergency_stop = "esc"  # Emergency stop key
        
    def start_playback(self, actions=None, loop_count=1, delay_factor=1.0):
        """Start playing back recorded actions"""
        if self.playing:
            return
            
        if actions is None:
            actions = self.app.recorded_actions
            
        if not actions:
            return
            
        self.playing = True
        self.stop_requested = False
        
        # Get settings from app
        if hasattr(self.app, 'loop_count') and hasattr(self.app.loop_count, 'get'):
            loop_count = self.app.loop_count.get()
            
        if hasattr(self.app, 'infinite_loop_var') and hasattr(self.app.infinite_loop_var, 'get'):
            if self.app.infinite_loop_var.get():
                loop_count = -1  # Infinite loops
        
        if hasattr(self.app, 'delay_between_actions') and hasattr(self.app.delay_between_actions, 'get'):
            delay_factor = self.app.delay_between_actions.get()
        
        # Start playback thread
        self.thread = threading.Thread(
            target=self._playback_loop, 
            args=(actions, loop_count, delay_factor)
        )
        self.thread.daemon = True
        self.thread.start()
        
        # Register hotkeys
        keyboard.add_hotkey(self.stop_hotkey, self.stop_playback)
        keyboard.add_hotkey(self.emergency_stop, self.stop_playback)
        
        # Update UI in main thread
        if self.app.root:
            self.app.root.after(0, self._update_ui_start)
    
    def stop_playback(self):
        """Stop the playback"""
        if not self.playing:
            return
            
        self.stop_requested = True
        self.playing = False
        
        # Remove hotkeys
        try:
            keyboard.remove_hotkey(self.stop_hotkey)
            keyboard.remove_hotkey(self.emergency_stop)
        except:
            pass
        
        # Update UI in main thread
        if self.app.root:
            self.app.root.after(0, self._update_ui_stop)
    
    def _update_ui_start(self):
        """Update UI when playback starts"""
        self.app.is_playing = True
        self.app.play_button.config(text="Stop Playback")
        self.app.status_var.set("Playing...")
    
    def _update_ui_stop(self):
        """Update UI when playback stops"""
        self.app.is_playing = False
        self.app.play_button.config(text="Start Playback")
        self.app.status_var.set("Playback stopped")
    
    def _update_ui_progress(self, action_index, total_actions, current_loop, total_loops):
        """Update UI with playback progress"""
        if total_loops < 0:  # Infinite loops
            status = f"Playing: Action {action_index + 1}/{total_actions} (Loop {current_loop + 1}/∞)"
        else:
            status = f"Playing: Action {action_index + 1}/{total_actions} (Loop {current_loop + 1}/{total_loops})"
        
        self.app.status_var.set(status)
        
        # Highlight current action in listbox
        self.app.actions_listbox.selection_clear(0, "end")
        self.app.actions_listbox.selection_set(action_index)
        self.app.actions_listbox.see(action_index)
    
    def _playback_loop(self, actions, loop_count, delay_factor):
        """Main playback loop"""
        # Save original pyautogui settings
        original_pause = pyautogui.PAUSE
        original_failsafe = pyautogui.FAILSAFE
        
        try:
            # Configure pyautogui for playback
            pyautogui.PAUSE = 0  # We'll handle delays ourselves
            pyautogui.FAILSAFE = True  # Keep failsafe enabled
            
            # Disable pyautogui's internal delay between actions
            pyautogui.MINIMUM_DURATION = 0
            pyautogui.MINIMUM_SLEEP = 0
            
            # Start playback loops
            loops_to_run = float('inf') if loop_count < 0 else loop_count
            current_loop = 0
            
            while current_loop < loops_to_run and not self.stop_requested:
                # Execute all actions in sequence
                for i, action in enumerate(actions):
                    if self.stop_requested:
                        break
                    
                    # Update UI to show current action
                    if self.app.root:
                        self.app.root.after(0, lambda idx=i, total=len(actions), 
                                           loop=current_loop, loops=loop_count: 
                                           self._update_ui_progress(idx, total, loop, loops))
                    
                    # Execute the action
                    self._execute_action(action, delay_factor)
                
                # Increment loop counter
                current_loop += 1
        
        finally:
            # Restore original pyautogui settings
            pyautogui.PAUSE = original_pause
            pyautogui.FAILSAFE = original_failsafe
            
            # Make sure we're no longer playing
            if self.playing:
                self.stop_playback()
    
    def _execute_action(self, action, delay_factor=1.0):
        """Execute a single recorded action"""
        action_type = action["type"]
        
        try:
            if action_type == ActionType.MOUSE_MOVE:
                x, y = action["position"]
                pyautogui.moveTo(x, y)
                
            elif action_type == ActionType.MOUSE_CLICK:
                x, y = action["position"]
                button = action["button"]
                state = action["state"]
                
                # Move mouse to position
                pyautogui.moveTo(x, y)
                
                # Handle mouse button state
                if state == "down":
                    pyautogui.mouseDown(button=button)
                elif state == "up":
                    pyautogui.mouseUp(button=button)
                    
            elif action_type == ActionType.MOUSE_SCROLL:
                x, y = action["position"]
                amount = action["amount"]
                
                # Move mouse to position
                pyautogui.moveTo(x, y)
                
                # Scroll
                pyautogui.scroll(amount)
                
            elif action_type == ActionType.KEY_PRESS:
                key = action["key"]
                keyboard.press(key)
                
            elif action_type == ActionType.KEY_RELEASE:
                key = action["key"]
                keyboard.release(key)
                
            elif action_type == ActionType.DELAY:
                adjusted_duration = action["duration"] / delay_factor
                time.sleep(adjusted_duration)

                
        except Exception as e:
            print(f"Error executing action {action_type}: {e}")
            # Don't stop playback on error, just continue with next action