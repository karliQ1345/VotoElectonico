using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class HistorialVotacion
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; } // ¿Quién votó? (Admin, Candidato o Votante)
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        public int ProcesoElectoralId { get; set; }
        [ForeignKey("ProcesoElectoralId")]
        public virtual ProcesoElectoral Proceso { get; set; }

        public DateTime FechaSufragio { get; set; } = DateTime.UtcNow;
        public string CodigoCertificado { get; set; } // UUID único
    }
}
