namespace TuProyecto.DTOs.Votacion;

public class IniciarVotacionRequestDto
{
    public string ProcesoElectoralId { get; set; } = null!;
    public string Cedula { get; set; } = null!;
    public string CodigoUnico { get; set; } = null!;
}

