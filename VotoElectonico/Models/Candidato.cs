using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class Candidato
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombres { get; set; }
        public string FotoUrl { get; set; }
        public int? OrdenEnLista { get; set; } // 1, 2, 3... (Importante para Asambleístas)

        public int PartidoPoliticoId { get; set; }
        [ForeignKey("PartidoPoliticoId")]
        public PartidoPolitico Partido { get; set; }
    }
}
