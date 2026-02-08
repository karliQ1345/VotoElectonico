using System.ComponentModel.DataAnnotations.Schema;
using VotoElectonico.Models.Enums;

namespace VotoElectonico.Models
{
    public class ComprobanteVoto
    {
        public Guid Id { get; set; }

        public Guid ProcesoElectoralId { get; set; }
     
        public Guid EleccionId { get; set; }


        public Guid UsuarioId { get; set; }                 // aquí SÍ va identidad (es el comprobante)
        public Usuario Usuario { get; set; } = null!;

        public Guid JuntaId { get; set; }
        public Junta Junta { get; set; } = null!;

        public Guid JefeJuntaUsuarioId { get; set; }
        public Usuario JefeJuntaUsuario { get; set; } = null!;

        public DateTime GeneradoUtc { get; set; } = DateTime.UtcNow;

        // Puedes guardar el PDF en storage y/o enviarlo como adjunto con Brevo
        public string? PdfUrl { get; set; }                 // opcional pero útil

        public ComprobanteEstado EstadoEnvio { get; set; } = ComprobanteEstado.Pendiente;
        public DateTime? EnviadoUtc { get; set; }
        public string? BrevoMessageId { get; set; }
        public string? ErrorEnvio { get; set; }
        public string? PublicToken { get; set; }
        public DateTime? PublicTokenExpiraUtc { get; set; }
     
    }
}
