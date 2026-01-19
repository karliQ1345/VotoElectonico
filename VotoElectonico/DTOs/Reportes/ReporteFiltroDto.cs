namespace TuProyecto.DTOs.Reportes;

public class ReporteFiltroDto
{
    public string ProcesoElectoralId { get; set; } = null!;
    public string EleccionId { get; set; } = null!;
    public string Dimension { get; set; } = null!; // "Provincia", "Canton", "Parroquia", "Genero"
}

