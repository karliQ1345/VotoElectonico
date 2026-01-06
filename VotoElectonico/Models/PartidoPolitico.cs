using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class PartidoPolitico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreLista { get; set; } // Ej: "Revolución Ciudadana"
        public int NumeroLista { get; set; }    // Ej: 5
        public string LogoUrl { get; set; }

        // FK: Un partido pertenece a un proceso específico
        public int ProcesoElectoralId { get; set; }
        [ForeignKey("ProcesoElectoralId")]
        public ProcesoElectoral Proceso { get; set; }

        public ICollection<Candidato> Candidatos { get; set; }
    }
}
