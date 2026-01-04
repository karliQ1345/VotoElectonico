using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.Models
{
    public class ProcesoElectoral
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreProceso { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public bool Estado { get; set; } = true; // Por defecto activo

        // Relación: Un proceso tiene muchos candidatos
        public List<Candidato> Candidatos { get; set; } = null!;
    }
}
