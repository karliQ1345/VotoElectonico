using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace VotoElectonico.Utils
{
    public static class SecurityHelpers
    {
        public static string MaskEmail(string email)
        {
            var at = email.IndexOf('@');
            if (at <= 1) return "***" + (at >= 0 ? email.Substring(at) : "");
            return email.Substring(0, 1) + "****" + email.Substring(at);
        }

        // Guarda hash como "salt:hash"
        public static string HashWithSalt(string value)
        {
            var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            var hash = Sha256Base64(salt + ":" + value);
            return $"{salt}:{hash}";
        }

        public static bool VerifyHashWithSalt(string value, string storedSaltAndHash)
        {
            var parts = storedSaltAndHash.Split(':');
            if (parts.Length != 2) return false;
            var salt = parts[0];
            var expected = parts[1];
            var actual = Sha256Base64(salt + ":" + value);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(expected),
                Convert.FromBase64String(actual)
            );
        }

        private static string Sha256Base64(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }

        public static string GenerateNumericCode(int length)
        {
            // genera un número con longitud exacta (ej 6)
            var min = (int)Math.Pow(10, length - 1);
            var max = (int)Math.Pow(10, length) - 1;
            return RandomNumberGenerator.GetInt32(min, max + 1).ToString();
        }

        // AES-GCM (anónimo cifrado)
        public static (string cipherB64, string nonceB64, string tagB64) EncryptAesGcm(object payload, byte[] key)
        {
            var plaintext = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
            var nonce = RandomNumberGenerator.GetBytes(12);
            var cipher = new byte[plaintext.Length];
            var tag = new byte[16];

            using var aes = new AesGcm(key);
            aes.Encrypt(nonce, plaintext, cipher, tag);

            return (Convert.ToBase64String(cipher), Convert.ToBase64String(nonce), Convert.ToBase64String(tag));
        }
    }
}
