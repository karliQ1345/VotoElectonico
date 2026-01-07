using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class PartidoPolitico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreLista { get; set; } = default!;

        public int NumeroLista { get; set; }

        public string? LogoUrl { get; set; }

        public int ProcesoElectoralId { get; set; }

        [ForeignKey(nameof(ProcesoElectoralId))]
        public virtual ProcesoElectoral Proceso { get; set; } = default!;

        public virtual ICollection<Candidato> Candidatos { get; set; } = new List<Candidato>();
    }

}
