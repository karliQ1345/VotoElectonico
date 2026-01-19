namespace TuProyecto.DTOs.Procesos;

public class CrearProcesoRequestDto
{
    public string Nombre { get; set; } = null!;
    public DateTime InicioUtc { get; set; }
    public DateTime FinUtc { get; set; }
}

