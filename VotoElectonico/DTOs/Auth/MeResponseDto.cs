namespace VotoElectonico.Auth;

public class MeResponseDto
{
    public string UsuarioId { get; set; } = null!;
    public string Cedula { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}

