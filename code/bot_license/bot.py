import logging
from telegram import Update, BotCommand
from telegram.ext import Application, CommandHandler, ContextTypes

from license_generator import generate_license_key, get_public_key_base64

# --- Configuration ---
TELEGRAM_API_KEY = "apikey"

# Enable logging
logging.basicConfig(
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s", level=logging.INFO
)
logger = logging.getLogger(__name__)


async def start(update: Update, context: ContextTypes.DEFAULT_TYPE) -> None:
    welcome_text = (
        "Welcome to the License Key Generator Bot!\n\n"
        "Commands available:\n"
        "• /generate <EncryptedMachineID> → Generate a license key\n"
        "• /publickey → Show the Public Key for app compilation\n\n"
        "Example:\n"
        "/generate ABCDE12345..."
    )
    await update.message.reply_text(welcome_text)


async def generate(update: Update, context: ContextTypes.DEFAULT_TYPE) -> None:
    if not context.args:
        await update.message.reply_text(
            "Please provide an Encrypted Machine ID after the command.\n"
            "Example: /generate ABCDE12345..."
        )
        return

    encrypted_machine_id = context.args[0]
    user = update.effective_user
    logger.info(f"User {user.username} ({user.id}) requested a key for machine ID: {encrypted_machine_id}")

    try:
        license_key = generate_license_key(encrypted_machine_id)
        response_text = f"License Key Generated Successfully:\n\n`{license_key}`"
        await update.message.reply_text(response_text, parse_mode='MarkdownV2')
        logger.info(f"Successfully generated key for machine ID: {encrypted_machine_id}")
    except Exception as e:
        logger.error(f"Failed to generate key for machine ID: {encrypted_machine_id}. Error: {e}")
        await update.message.reply_text(
            "An error occurred while generating the license key. "
            "Please ensure the Encrypted Machine ID is correct and try again."
        )


async def publickey(update: Update, context: ContextTypes.DEFAULT_TYPE) -> None:
    """Sends the public key in Base64 for app compilation."""
    try:
        public_key = get_public_key_base64()
        response_text = (
            "Public Key (Base64, ready for C#):\n\n"
            f"{public_key}"
        )
        # Use HTML format to avoid MarkdownV2 escaping issues
        await update.message.reply_text(response_text)
        logger.info("Public key sent successfully.")
    except Exception as e:
        logger.error(f"Error retrieving public key: {e}", exc_info=True)
        await update.message.reply_text("An error occurred while retrieving the public key.")


async def post_init(application: Application) -> None:
    commands = [
        BotCommand("start", "Show welcome message and instructions"),
        BotCommand("generate", "Generate a new license key from a machine ID"),
        BotCommand("publickey", "Show the public key for app compilation"),
    ]
    await application.bot.set_my_commands(commands)


def main() -> None:
    application = Application.builder().token(TELEGRAM_API_KEY).post_init(post_init).build()

    application.add_handler(CommandHandler("start", start))
    application.add_handler(CommandHandler("generate", generate))
    application.add_handler(CommandHandler("publickey", publickey))

    logger.info("Starting bot...")
    application.run_polling()


if __name__ == "__main__":
    main()