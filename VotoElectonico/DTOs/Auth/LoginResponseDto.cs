namespace VotoElectonico.DTOs.Auth;

public class LoginResponseDto
{
    public bool RequiereTwoFactor { get; set; }
    public string? TwoFactorSessionId { get; set; }  

    public string RolPrincipal { get; set; } = null!; 
    public string Redirect { get; set; } = null!;     

    public string? EmailEnmascarado { get; set; }     
}

