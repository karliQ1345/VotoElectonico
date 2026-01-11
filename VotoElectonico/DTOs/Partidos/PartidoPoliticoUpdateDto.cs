using Microsoft.Build.Framework;

namespace VotoElectonico.DTOs.Partidos
{
    public class PartidoPoliticoUpdateDto
    {
        [Required]
        public string NombreLista { get; set; } = default!;

        [Required]
        public int NumeroLista { get; set; }

        public string? LogoUrl { get; set; }
    }
}
