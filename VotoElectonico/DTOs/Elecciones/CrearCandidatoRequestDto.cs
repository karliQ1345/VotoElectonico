namespace VotoElectonico.DTOs.Elecciones;

public class CrearCandidatoRequestDto
{
    public string EleccionId { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string? Cargo { get; set; }
    public string FotoUrl { get; set; } = null!;    // ya subida a storage
    public string? PartidoListaId { get; set; }
}

