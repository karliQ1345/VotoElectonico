namespace TuProyecto.DTOs.TwoFactor;

public class TwoFactorVerifyResponseDto
{
    public bool Verificado { get; set; }
    public string Token { get; set; } = null!;        
    public string RolPrincipal { get; set; } = null!;
    public string Redirect { get; set; } = null!;
}

