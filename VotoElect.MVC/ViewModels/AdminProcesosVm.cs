using VotoElect.MVC.ApiContracts;

namespace VotoElect.MVC.ViewModels;

public class AdminProcesosVm
{
    public string? Error { get; set; }
    public string? Ok { get; set; }

    public List<ProcesoResumenDto> Procesos { get; set; } = new();

    public string Nombre { get; set; } = "";
    public DateTime InicioUtc { get; set; } = DateTime.UtcNow.AddHours(1);
    public DateTime FinUtc { get; set; } = DateTime.UtcNow.AddDays(1);
}

