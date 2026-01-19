namespace TuProyecto.DTOs.Juntas;

public class VotanteVerificacionDto
{
    public string UsuarioId { get; set; } = null!;
    public string Cedula { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Telefono { get; set; }
    public string? FotoUrl { get; set; }

    public string Provincia { get; set; } = "";
    public string Canton { get; set; } = "";
    public string Parroquia { get; set; } = "";
    public string Genero { get; set; } = "";

    public bool YaVoto { get; set; }
}
