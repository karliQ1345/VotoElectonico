

using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.DTOs.Partidos

{
    public class PartidoPoliticoCreateDto
    {
        [Required]
        public int ProcesoElectoralId { get; set; }

        [Required]
        public string NombreLista { get; set; } = default!;

        [Required]
        public int NumeroLista { get; set; }

        public string? LogoUrl { get; set; }
    }
}
