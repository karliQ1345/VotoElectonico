using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class Voto
    {
        [Key]
        public int Id { get; set; }

        public int ProcesoElectoralId { get; set; }
        [ForeignKey("ProcesoElectoralId")]
        public virtual ProcesoElectoral Proceso { get; set; }

        public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;

        [Required]
        public string HashSeguridad { get; set; } // SHA256

        // COPIA DEMOGRÁFICA (Snapshot del momento del voto)
        public Genero GeneroVotante { get; set; }
        public string ProvinciaVotante { get; set; }
        public string CantonVotante { get; set; }
        public string ParroquiaVotante { get; set; }

        public virtual ICollection<DetalleVoto> Detalles { get; set; }
    }
}
