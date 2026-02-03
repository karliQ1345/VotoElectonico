namespace VotoElectonico.DTOs.Votacion;

public class EmitirVotoRequestDto
{
    public string ProcesoElectoralId { get; set; } = null!;
    public string EleccionId { get; set; } = null!;

    // validación presencial
    public string Cedula { get; set; } = null!;
    public string CodigoUnico { get; set; } = null!;
    public string? PresidenteCandidatoId { get; set; }

    public string? OpcionPresidente { get; set; }

    // Asambleístas:
    public string? PartidoListaId { get; set; }    // si votó plancha
    public List<string>? CandidatoIds { get; set; } // si votó individual
}

