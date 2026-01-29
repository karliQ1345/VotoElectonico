namespace VotoElect.MVC.ViewModels;

public class AccesoIndexVm
{
    public string Cedula { get; set; } = "";
    public string? Error { get; set; }
}

public class AccesoOtpVm
{
    public string Codigo { get; set; } = "";
    public string? EmailEnmascarado { get; set; }
    public string? Error { get; set; }
}
