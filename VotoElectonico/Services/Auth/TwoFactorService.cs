using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using VotoElectonico.Options;

namespace VotoElectonico.Services.Auth
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly TwoFactorOptions _opt;

        public TwoFactorService(IOptions<TwoFactorOptions> opt)
        {
            _opt = opt.Value;
        }

        public string GenerateOtp(int digits = 6)
        {
            // OTP numérico
            var max = (int)Math.Pow(10, digits);
            var value = RandomNumberGenerator.GetInt32(0, max);
            return value.ToString().PadLeft(digits, '0');
        }

        public string HashOtp(string otp)
        {
            // Hash = SHA256(otp + pepper)
            var input = $"{otp}:{_opt.Pepper}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }

        public bool VerifyOtp(string otp, string storedHash)
        {
            var hash = HashOtp(otp);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(hash),
                Convert.FromBase64String(storedHash)
            );
        }
    }
}
