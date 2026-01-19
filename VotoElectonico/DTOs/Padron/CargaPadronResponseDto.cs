namespace TuProyecto.DTOs.Padron;

public class CargaPadronResponseDto
{
    public int Total { get; set; }
    public int Insertados { get; set; }
    public int Actualizados { get; set; }
    public int ConError { get; set; }

    public List<string> Errores { get; set; } = new();
}
