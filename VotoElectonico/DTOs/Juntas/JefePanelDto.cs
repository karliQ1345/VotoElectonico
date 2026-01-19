namespace TuProyecto.DTOs.Juntas;

public class JefePanelDto
{
    public string JuntaId { get; set; } = null!;
    public string JuntaCodigo { get; set; } = null!;
    public string Provincia { get; set; } = null!;
    public string Canton { get; set; } = null!;
    public string Parroquia { get; set; } = null!;
    public bool ProcesoActivo { get; set; }

    public bool BotonIrAVotarDisponible { get; set; } // para el jefe (solo una vez)
}

