using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class HistorialVotacion
    {
        [Key]
        public int Id { get; set; }

        // SABEMOS QUIÉN (Usuario)
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        // SABEMOS DÓNDE (Proceso)
        public int ProcesoElectoralId { get; set; }
        [ForeignKey("ProcesoElectoralId")]
        public ProcesoElectoral Proceso { get; set; }

        public DateTime FechaSufragio { get; set; } = DateTime.Now;

        // Opcional: Código único para que el usuario verifique su certificado
        public string CodigoCertificado { get; set; }
    }
}
