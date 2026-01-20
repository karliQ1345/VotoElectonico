namespace VotoElectonico.DTOs.TwoFactor;

public class TwoFactorStartResponseDto
{
    public string TwoFactorSessionId { get; set; } = null!;
    public string EmailEnmascarado { get; set; } = null!;
    public int ExpiraEnSegundos { get; set; }
}

