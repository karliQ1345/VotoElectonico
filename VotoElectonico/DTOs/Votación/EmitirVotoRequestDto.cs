using System.ComponentModel.DataAnnotations;
using VotoElectonico.Models;

namespace VotoElectonico.DTOs.Votación
{
    public class EmitirVotoDetalleDto
    {
        [Required]
        public TipoDetalleVoto Tipo { get; set; }

        public int? PartidoPoliticoId { get; set; }
        public int? CandidatoId { get; set; }
    }

    public class EmitirVotoRequestDto
    {
        [Required]
        public int ProcesoElectoralId { get; set; }

        [Required]
        public List<EmitirVotoDetalleDto> Detalles { get; set; } = new();
    }
}
