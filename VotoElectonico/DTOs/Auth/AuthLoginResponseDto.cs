namespace VotoElectonico.DTOs.Auth
{
    public class AuthLoginResponseDto
    {
        public Guid TwoFactorSessionId { get; set; }
        public string CorreoEnmascarado { get; set; } = default!;
        public DateTime ExpiraUtc { get; set; }
    }
}
