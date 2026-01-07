using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.Models
{
    public class ProcesoElectoral
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } = default!;

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        [Required]
        public EstadoProcesoElectoral Estado { get; set; } = EstadoProcesoElectoral.Pendiente;

        [Required]
        public TipoEleccion Tipo { get; set; }

        // --- REGLAS DEL PROCESO ---
        // Presidencial:
        public ModalidadPresidencial? ModalidadPresidencial { get; set; }

        // Asambleístas:
        public bool PermitePlancha { get; set; } = true;
        public bool PermiteNominal { get; set; } = true;

        /// <summary>
        /// Número máximo de asambleístas nominales a elegir (p.ej. 8).
        /// Aplica cuando Tipo = Asambleistas.
        /// </summary>
        public int? MaxSeleccionNominal { get; set; }

        // Relaciones
        public virtual ICollection<PartidoPolitico> PartidosInscritos { get; set; } = new List<PartidoPolitico>();
        public virtual ICollection<Voto> UrnaDeVotos { get; set; } = new List<Voto>();

        // Relación para saber quiénes ya sufragaron
        public virtual ICollection<HistorialVotacion> HistorialLog { get; set; } = new List<HistorialVotacion>();
    }
}
