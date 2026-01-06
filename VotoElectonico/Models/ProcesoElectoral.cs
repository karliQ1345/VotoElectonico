using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.Models
{
    public class ProcesoElectoral
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } // Ej: "Elecciones Seccionales 2026"

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public bool Activo { get; set; } // Admin puede pausar/cerrar el proceso

        [Required]
        public TipoEleccion Tipo { get; set; }

        // Relaciones
        public ICollection<PartidoPolitico> PartidosInscritos { get; set; }
        // Aquí están los votos anónimos de este proceso
        public ICollection<Voto> UrnaDeVotos { get; set; }
    }
}
