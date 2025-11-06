import os
import subprocess
import sys
from license_generator import get_public_key_base64

def install_dependencies():
    """Installs dependencies from requirements.txt."""
    print("--- Installing/updating dependencies from requirements.txt ---")
    try:
        subprocess.check_call([sys.executable, "-m", "pip", "install", "-r", "requirements.txt"])
        print("\n--- Dependencies installed successfully. ---")
    except subprocess.CalledProcessError as e:
        print(f"\n--- Error installing dependencies: {e} ---")
    except FileNotFoundError:
        print("\n--- Error: requirements.txt not found. ---")


def launch_bot():
    """Launches the Telegram bot."""
    print("\n--- Launching the Telegram bot... (Press Ctrl+C to stop) ---")
    try:
        subprocess.run([sys.executable, "bot.py"])
    except FileNotFoundError:
        print("\n--- Error: bot.py not found. Make sure it is in the same directory. ---")
    except Exception as e:
        print(f"\n--- An error occurred while trying to run the bot: {e} ---")


def delete_regenerate_key():
    """Deletes the private key file, so a new one is generated on next run."""
    key_file = "private_key.b64"
    print(f"\n--- Attempting to delete private key file: {key_file} ---")
    
    if os.path.exists(key_file):
        try:
            os.remove(key_file)
            print(f"--- Successfully deleted {key_file}. ---")
            print("--- A new private key will be automatically generated the next time the bot is launched. ---")
        except OSError as e:
            print(f"--- Error deleting key file: {e} ---")
    else:
        print(f"--- {key_file} not found. No key to delete. ---")
        print("--- A new private key will be generated automatically on the next run if one is not found. ---")


def show_public_key():
    """Displays the public key in Base64 format."""
    print("\n--- Public Key ---")
    try:
        public_key = get_public_key_base64()
        print(public_key)
        print("\n⚠️ Use this key in your app compilation code for license validation.")
    except Exception as e:
        print(f"--- Error retrieving public key: {e} ---")


def main_menu():
    """Displays the main menu and handles user input."""
    while True:
        print("\n" + "="*30)
        print("   Bot Management Menu")
        print("="*30)
        print("1. Install/Update Dependencies")
        print("2. Launch Bot")
        print("3. Delete and Regenerate Private Key")
        print("4. Show Public Key")
        print("5. Exit")
        print("="*30)
        
        choice = input("Enter your choice (1-5): ")

        if choice == '1':
            install_dependencies()
        elif choice == '2':
            launch_bot()
        elif choice == '3':
            delete_regenerate_key()
        elif choice == '4':
            show_public_key()
        elif choice == '5':
            print("\n--- Exiting. ---")
            break
        else:
            print("\n--- Invalid choice, please try again. ---")


if __name__ == "__main__":
    main_menu()