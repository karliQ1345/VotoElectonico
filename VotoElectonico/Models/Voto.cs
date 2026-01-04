using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class Voto
    {
        [Key]
        public int Id { get; set; }

        // OJO: Aquí NO ponemos UsuarioId. Así se garantiza el anonimato.

        // 1. ¿Para qué candidato es el voto?
        [Required]
        public int CandidatoId { get; set; }
        [ForeignKey("CandidatoId")]
        public Candidato Candidato { get; set; } = null!;

        // 2. ¿A qué urna pertenece? (El proceso)
        [Required]
        public int ProcesoElectoralId { get; set; }
        [ForeignKey("ProcesoElectoralId")]
        public ProcesoElectoral ProcesoElectoral { get; set; } = null!;
    }
}
