namespace VotoElectonico.DTOs.TwoFactor;

public class TwoFactorVerifyRequestDto
{
    public string TwoFactorSessionId { get; set; } = null!;
    public string Codigo { get; set; } = null!;
}

