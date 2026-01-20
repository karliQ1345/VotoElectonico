namespace VotoElectonico.DTOs.Padron;

public class PadronExcelRowDto
{
    public string Cedula { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string Email { get; set; } = null!;

    public string Provincia { get; set; } = null!;
    public string Canton { get; set; } = null!;
    public string Parroquia { get; set; } = null!;
    public string Genero { get; set; } = null!;

    public string? FotoUrl { get; set; }

    public string JuntaCodigo { get; set; } = null!;
}

