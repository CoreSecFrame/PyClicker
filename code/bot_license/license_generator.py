import os
import base64
from cryptography.hazmat.primitives.asymmetric import rsa, padding
from cryptography.hazmat.primitives import serialization, hashes

# --- Ruta absoluta para que funcione desde cualquier lugar ---
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
PRIVATE_KEY_FILE = os.path.join(BASE_DIR, "private_key.b64")


def load_or_generate_private_key():
    """Carga o genera la clave privada compatible con C# RSA.ImportRSAPrivateKey()."""
    if os.path.exists(PRIVATE_KEY_FILE):
        with open(PRIVATE_KEY_FILE, "r") as f:
            private_key_b64 = f.read().strip()
        # Decodificar el formato RSAPrivateKey (PKCS#1) como lo hace C#
        private_key_der = base64.b64decode(private_key_b64)
        # Usar load_der_private_key que maneja correctamente PKCS#1
        private_key = serialization.load_der_private_key(
            private_key_der,
            password=None
        )
    else:
        private_key = rsa.generate_private_key(
            public_exponent=65537,
            key_size=2048,
        )
        # Exportar en formato PKCS#1 (RSAPrivateKey) exactamente como C#
        private_key_bytes = private_key.private_bytes(
            encoding=serialization.Encoding.DER,
            format=serialization.PrivateFormat.TraditionalOpenSSL,
            encryption_algorithm=serialization.NoEncryption()
        )
        private_key_b64 = base64.b64encode(private_key_bytes).decode('utf-8')
        with open(PRIVATE_KEY_FILE, "w") as f:
            f.write(private_key_b64)
    return private_key


# Cargamos la clave al importar
private_key = load_or_generate_private_key()


def get_public_key_base64():
    """
    Retorna la clave pública en Base64 compatible con C# RSA.ExportRSAPublicKey().
    Formato DER PKCS#1 (RSAPublicKey), exactamente como lo hace C#.
    """
    public_key = private_key.public_key()
    public_bytes = public_key.public_bytes(
        encoding=serialization.Encoding.DER,
        format=serialization.PublicFormat.PKCS1
    )
    return base64.b64encode(public_bytes).decode('utf-8')


def get_public_key_pem():
    """Opcional: retorna la clave pública en PEM (para pruebas o OpenSSL)."""
    public_key = private_key.public_key()
    pem = public_key.public_bytes(
        encoding=serialization.Encoding.PEM,
        format=serialization.PublicFormat.PKCS1
    )
    return pem.decode('utf-8')


def generate_license_key(encrypted_machine_id):
    """Genera una license key para un Machine ID cifrado."""
    encrypted_bytes = base64.b64decode(encrypted_machine_id)
    decrypted_bytes = private_key.decrypt(
        encrypted_bytes,
        padding.OAEP(
            mgf=padding.MGF1(algorithm=hashes.SHA256()),
            algorithm=hashes.SHA256(),
            label=None
        )
    )
    signature = private_key.sign(
        decrypted_bytes,
        padding.PKCS1v15(),
        hashes.SHA256()
    )
    return base64.b64encode(signature).decode('utf-8')


# Alias para compatibilidad con código antiguo (bot o run.py)
get_public_key = get_public_key_base64


if __name__ == '__main__':
    import sys

    if len(sys.argv) > 1:
        encrypted_machine_id = sys.argv[1]
        try:
            license_key = generate_license_key(encrypted_machine_id)
            print("License Key:")
            print(license_key)
        except Exception as e:
            print(f"Error generating license key: {e}")
    else:
        # Imprime la clave pública lista para copiar en C#
        print("Public Key Base64 (copiar directamente en C#):")
        print(get_public_key_base64())
        print("\nOpcional (PEM):")
        print(get_public_key_pem())
        print("\nUsage: python license_generator.py <encrypted_machine_id>")