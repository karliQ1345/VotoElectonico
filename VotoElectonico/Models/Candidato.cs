using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class Candidato
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreCandidato { get; set; } = string.Empty;

        [Required]
        public string PartidoPolitico { get; set; } = string.Empty;


        public string FotoUrl { get; set; } = string.Empty;
        // Ruta de la imagen

        // --- Relación con Proceso Electoral ---
        [Required]
        public int ProcesoElectoralId { get; set; }

        [ForeignKey("ProcesoElectoralId")]
        public ProcesoElectoral ProcesoElectoral { get; set; } = null!;
    }
}
