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
        public Voto Voto { get; set; }

        // CASO 1: VOTO EN PLANCHA
        // Si tiene valor, el voto cuenta para toda la lista.
        public int? PartidoPoliticoId { get; set; }
        [ForeignKey("PartidoPoliticoId")]
        public PartidoPolitico? Partido { get; set; }

        // CASO 2: VOTO NOMINAL (Por persona)
        // Si tiene valor, el voto es para este candidato específico.
        public int? CandidatoId { get; set; }
        [ForeignKey("CandidatoId")]
        public Candidato? Candidato { get; set; }
    }
}
