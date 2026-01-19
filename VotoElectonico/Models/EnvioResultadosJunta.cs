using VotoElectonico.Models.Enums;

namespace VotoElectonico.Models
{
    public class EnvioResultadosJunta
    {
        public Guid Id { get; set; }

        public Guid ProcesoElectoralId { get; set; }
        public Guid JuntaId { get; set; }

        public Guid EnviadoPorJefeId { get; set; }          // usuario jefe
        public DateTime EnviadoUtc { get; set; } = DateTime.UtcNow;

        // Blob cifrado (ej: JSON con conteos + firma)
        public string PayloadCifradoBase64 { get; set; } = null!;
        public string KeyVersion { get; set; } = "v1";

        // Integridad
        public string? HashSha256 { get; set; }

        public EnvioResultadoEstado Estado { get; set; } = EnvioResultadoEstado.Recibido;
        public string? Observacion { get; set; }
    }
}
