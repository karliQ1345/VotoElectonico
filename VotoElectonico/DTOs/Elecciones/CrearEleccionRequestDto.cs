namespace VotoElectonico.DTOs.Elecciones;

public class CrearEleccionRequestDto
{
    public string ProcesoElectoralId { get; set; } = null!;
    public string Tipo { get; set; } = null!; // "Nominal" / "Plurinominal"
    public string Titulo { get; set; } = null!;
    public int? MaxSeleccionIndividual { get; set; }
}

