namespace VotoElect.MVC.ApiContracts;

public class CrearProcesoRequestDto
{
    public string Nombre { get; set; } = "";
    public DateTime InicioUtc { get; set; }
    public DateTime FinUtc { get; set; }
}

public class ProcesoResumenDto
{
    public string ProcesoElectoralId { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Estado { get; set; } = ""; // Pendiente|Activo|Finalizado
    public DateTime InicioUtc { get; set; }
    public DateTime FinUtc { get; set; }
    public bool PadronCargado { get; set; }
    public int PadronTotal { get; set; }
}

public class IdResponseDto
{
    public string Id { get; set; } = "";
}

