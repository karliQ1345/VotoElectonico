using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class HistorialVotacion
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        // --- RELACIONES DE SEGURIDAD ---

        // 1. ¿Quién votó? (Sabemos quién fue para que no repita)
        [Required]
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; } = null!;

        // 2. ¿En qué elección participó?
        [Required]
        public int ProcesoElectoralId { get; set; }
        [ForeignKey("ProcesoElectoralId")]
        public ProcesoElectoral ProcesoElectoral { get; set; } = null!;
    }
}
