namespace VotoElectonico.DTOs.Votacion;

public class IniciarVotacionResponseDto
{
    public bool Habilitado { get; set; }
    public string Mensaje { get; set; } = "";

    public string JuntaCodigo { get; set; } = "";
    public string? Recinto { get; set; }
}

