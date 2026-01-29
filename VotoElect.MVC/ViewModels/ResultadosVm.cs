using VotoElect.MVC.ApiContracts;

namespace VotoElect.MVC.ViewModels;

public class ResultadosVm
{
    public string ProcesoElectoralId { get; set; } = "";
    public string EleccionId { get; set; } = "";

    public string Dimension { get; set; } = "Nacional"; // o Provincia/Canton/Parroquia/Genero
    public string? Error { get; set; }

    public ReporteResponseDto? Reporte { get; set; }
}
