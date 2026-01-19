namespace TuProyecto.DTOs.Juntas;

public class JefeVerificarVotanteResponseDto
{
    public bool Permitido { get; set; }
    public string Mensaje { get; set; } = "";

    public VotanteVerificacionDto? Votante { get; set; }

    // Código único se muestra al jefe solo si pasó todo
    public string? CodigoUnico { get; set; }
}

