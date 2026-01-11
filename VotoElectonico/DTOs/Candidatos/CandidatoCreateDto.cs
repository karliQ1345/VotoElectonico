using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.DTOs.Candidatos
{
    public class CandidatoCreateDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int PartidoPoliticoId { get; set; }

        [Required]
        public string NombreEnPapeleta { get; set; } = default!;

        public string? FotoUrl { get; set; }

        public int? OrdenEnLista { get; set; }
    }
}
