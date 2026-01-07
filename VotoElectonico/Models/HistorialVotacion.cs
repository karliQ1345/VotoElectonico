using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class HistorialVotacion
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; } = default!;

        public int ProcesoElectoralId { get; set; }

        [ForeignKey(nameof(ProcesoElectoralId))]
        public virtual ProcesoElectoral Proceso { get; set; } = default!;

        public DateTime FechaSufragioUtc { get; set; } = DateTime.UtcNow;

        [Required]
        public string CodigoCertificado { get; set; } = default!; // UUID único
    }
}
