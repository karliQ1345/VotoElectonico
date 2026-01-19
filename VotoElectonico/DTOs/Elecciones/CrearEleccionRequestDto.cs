namespace TuProyecto.DTOs.Elecciones;

public class CrearEleccionRequestDto
{
    public string ProcesoElectoralId { get; set; } = null!;
    public string Tipo { get; set; } = null!; // "Presidente_SiNoBlanco" / "Asambleistas"
    public string Titulo { get; set; } = null!;
    public int? MaxSeleccionIndividual { get; set; }
}

