namespace VotoElectonico.DTOs.Elecciones;

public class CrearListaRequestDto
{
    public string EleccionId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string? LogoUrl { get; set; }
}

