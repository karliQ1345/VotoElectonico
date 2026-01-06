using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class DetalleVoto
    {
        [Key]
        public int Id { get; set; }

        public int VotoId { get; set; }
        [ForeignKey("VotoId")]
        public virtual Voto Voto { get; set; }

        // Opción A: Voto por Partido (Plancha)
        public int? PartidoPoliticoId { get; set; }
        [ForeignKey("PartidoPoliticoId")]
        public virtual PartidoPolitico? Partido { get; set; }

        // Opción B: Voto por Candidato (Nominal)
        // NOTA: Aquí referenciamos a la tabla Candidato, no a Usuario.
        public int? CandidatoId { get; set; }
        [ForeignKey("CandidatoId")]
        public virtual Candidato? Candidato { get; set; }
    }
}
