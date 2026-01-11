namespace VotoElectonico.Services.Auth
{
    public interface ITwoFactorService
    {
        string GenerateOtp(int digits = 6);
        string HashOtp(string otp);
        bool VerifyOtp(string otp, string storedHash);
    }
}
