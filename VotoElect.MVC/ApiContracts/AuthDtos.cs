namespace VotoElect.MVC.ApiContracts;

public class LoginRequestDto
{
    public string Cedula { get; set; } = null!;
}

public class LoginResponseDto
{
    public bool RequiereTwoFactor { get; set; }
    public string? TwoFactorSessionId { get; set; }
    public string RolPrincipal { get; set; } = null!;
    public string Redirect { get; set; } = null!;
    public string? EmailEnmascarado { get; set; }
}

public class TwoFactorVerifyRequestDto
{
    public string TwoFactorSessionId { get; set; } = null!;
    public string Codigo { get; set; } = null!;
}

public class TwoFactorVerifyResponseDto
{
    public bool Verificado { get; set; }
    public string Token { get; set; } = null!;
    public string RolPrincipal { get; set; } = null!;
    public string Redirect { get; set; } = null!;
}

