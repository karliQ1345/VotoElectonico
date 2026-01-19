namespace TuProyecto.DTOs.Reportes;

public class ReporteResponseDto
{
    public string ProcesoElectoralId { get; set; } = null!;
    public string EleccionId { get; set; } = null!;
    public string Dimension { get; set; } = null!;
    public List<ReporteItemDto> Items { get; set; } = new();
    public DateTime ActualizadoUtc { get; set; }
}

