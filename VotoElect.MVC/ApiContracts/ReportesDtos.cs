namespace VotoElect.MVC.ApiContracts;

public class ReporteFiltroDto
{
    public string ProcesoElectoralId { get; set; } = "";
    public string EleccionId { get; set; } = "";
    public string Dimension { get; set; } = ""; // "Provincia" | "Canton" | "Parroquia" | "Genero" ...
}

public class ReporteItemDto
{
    public string DimensionValor { get; set; } = "";
    public string Opcion { get; set; } = ""; // candidato/opción
    public int Votos { get; set; }
}

public class ReporteResponseDto
{
    public string ProcesoElectoralId { get; set; } = "";
    public string EleccionId { get; set; } = "";
    public string Dimension { get; set; } = "";
    public List<ReporteItemDto> Items { get; set; } = new();
    public DateTime ActualizadoUtc { get; set; }
}

