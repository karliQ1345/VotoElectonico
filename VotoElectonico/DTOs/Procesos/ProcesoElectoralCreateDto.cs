using System.ComponentModel.DataAnnotations;
using VotoElectonico.Models;

namespace VotoElectonico.DTOs.Procesos
{
    public class ProcesoElectoralCreateDto
    {
        [Required]
        public string Titulo { get; set; } = default!;

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public TipoEleccion Tipo { get; set; }
        public ModalidadPresidencial? ModalidadPresidencial { get; set; }

        // Reglas (asambleístas)
        public bool PermitePlancha { get; set; } = true;
        public bool PermiteNominal { get; set; } = true;

        public int? MaxSeleccionNominal { get; set; }
    }

}
