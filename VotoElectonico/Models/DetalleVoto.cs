using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class DetalleVoto
    {
        [Key]
        public int Id { get; set; }

        public int VotoId { get; set; }

        [ForeignKey(nameof(VotoId))]
        public virtual Voto Voto { get; set; } = default!;

        [Required]
        public TipoDetalleVoto Tipo { get; set; }

        // Para plancha:
        public int? PartidoPoliticoId { get; set; }

        [ForeignKey(nameof(PartidoPoliticoId))]
        public virtual PartidoPolitico? Partido { get; set; }

        // Para nominal:
        public int? CandidatoId { get; set; }

        [ForeignKey(nameof(CandidatoId))]
        public virtual Candidato? Candidato { get; set; }
    }
}
