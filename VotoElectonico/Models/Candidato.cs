using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class Candidato
    {
        [Key]
        public int Id { get; set; }

        // RELACIÓN 1 a 1 con USUARIO (único)
        public int UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; } = default!;

        // --- DATOS DE CAMPAÑA (lo que sale en papeleta) ---
        [Required]
        public string NombreEnPapeleta { get; set; } = default!;

        public string? FotoUrl { get; set; }

        /// <summary>
        /// Para asambleístas, orden dentro de la lista/plancha (opcional).
        /// </summary>
        public int? OrdenEnLista { get; set; }

        // RELACIÓN CON PARTIDO
        public int PartidoPoliticoId { get; set; }

        [ForeignKey(nameof(PartidoPoliticoId))]
        public virtual PartidoPolitico Partido { get; set; } = default!;
    }
}
