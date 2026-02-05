namespace VotoElectonico.DTOs.Procesos;

public class ProcesoResumenDto
{
    public string ProcesoElectoralId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public DateTime InicioUtc { get; set; }
    public DateTime FinUtc { get; set; }
    public bool PadronCargado { get; set; }
    public bool CandidatosCargados { get; set; }
    public int PadronTotal { get; set; }
    public int CandidatosTotal { get; set; }
}

