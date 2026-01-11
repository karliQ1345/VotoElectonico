namespace VotoElectonico.DTOs.Votación
{
    public class EstadoVotoDto
    {
        public int ProcesoElectoralId { get; set; }
        public bool YaVoto { get; set; }
        public DateTime? FechaSufragioUtc { get; set; }
        public string? CodigoCertificado { get; set; }
    }
}
