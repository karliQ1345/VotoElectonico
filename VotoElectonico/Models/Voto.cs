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
        public ProcesoElectoral Proceso { get; set; }

        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        // --- ENCRIPTACIÓN DE INTEGRIDAD ---
        // Hash SHA256 generado por el Backend para validar que el voto no fue manipulado.
        [Required]
        public string HashSeguridad { get; set; }

        // --- COPIA DEMOGRÁFICA ANÓNIMA ---
        // Estos datos se llenan copiando del Usuario al momento de votar.
        // Sirven para el reporte: "Mujeres de Ibarra votaron por X"
        // NO guardamos nombre ni cédula aquí.
        public Genero GeneroVotante { get; set; }
        public string ProvinciaVotante { get; set; }
        public string CantonVotante { get; set; }
        public string ParroquiaVotante { get; set; }

        // Contenido del voto
        public ICollection<DetalleVoto> Detalles { get; set; }
    }
}
