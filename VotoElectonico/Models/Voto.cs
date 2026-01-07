using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class Voto
    {
        [Key]
        public int Id { get; set; }

        public int ProcesoElectoralId { get; set; }

        [ForeignKey(nameof(ProcesoElectoralId))]
        public virtual ProcesoElectoral Proceso { get; set; } = default!; 

        /// <summary>
        /// Recomendación de anonimato:
        /// Si vas a guardar hora exacta, puede ayudar a re-identificar en elecciones pequeñas.
        /// Puedes redondear en tu lógica o guardar solo fecha.
        /// </summary>
        public DateTime FechaIngresoUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Hash para integridad / anti-manipulación (NO es cifrado).
        /// Si implementas cifrado, esto puede ser hash del payload cifrado.
        /// </summary>
        [Required]
        public string HashSeguridad { get; set; } = default!;

        // Snapshot demográfico (opcional; cuidado con re-identificación)
        public Genero? GeneroVotante { get; set; }
        public string? ProvinciaVotante { get; set; }
        public string? CantonVotante { get; set; }
        public string? ParroquiaVotante { get; set; }

        /// <summary>
        /// Si quieres cumplir "voto encriptado" estrictamente, guarda aquí el payload cifrado (JSON o binario).
        /// Puedes mantener Detalles para conteo rápido, o usar solo PayloadCifrado y contar desde ahí.
        /// </summary>
        public string? PayloadCifradoBase64 { get; set; }

        public virtual ICollection<DetalleVoto> Detalles { get; set; } = new List<DetalleVoto>();
    }
}
