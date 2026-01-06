using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.Models
{
    public class ProcesoElectoral
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public bool Activo { get; set; }

        [Required]
        public TipoEleccion Tipo { get; set; }

        // Relaciones
        public virtual ICollection<PartidoPolitico> PartidosInscritos { get; set; }
        public virtual ICollection<Voto> UrnaDeVotos { get; set; }

        // Relación para saber quiénes ya sufragaron en este proceso
        public virtual ICollection<HistorialVotacion> HistorialLog { get; set; }
    }
}
