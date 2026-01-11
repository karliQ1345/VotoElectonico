namespace VotoElectonico.DTOs.Votación
{
    public class EmitirVotoResponseDto
    {
        public int ProcesoElectoralId { get; set; }
        public DateTime FechaSufragioUtc { get; set; }
        public string CodigoCertificado { get; set; } = default!;
    }
}
