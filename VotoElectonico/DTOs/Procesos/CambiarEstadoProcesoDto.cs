using System.ComponentModel.DataAnnotations;
using VotoElectonico.Models;

namespace VotoElectonico.DTOs.Procesos
{
    public class CambiarEstadoProcesoDto
    {
        [Required]
        public AccionEstadoProceso Accion { get; set; }
    }
}
