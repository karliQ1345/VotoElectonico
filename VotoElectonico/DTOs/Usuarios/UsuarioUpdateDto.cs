using System.ComponentModel.DataAnnotations;
using VotoElectonico.Models;

namespace VotoElectonico.DTOs.Usuarios
{
    public class UsuarioUpdateDto
    {
        [Required]
        public RolUsuario Rol { get; set; }

        [Required, EmailAddress]
        public string CorreoElectronico { get; set; } = default!;

        [Required]
        public string NombresCompletos { get; set; } = default!;

        [Required]
        public Genero Genero { get; set; }

        [Required]
        public string Provincia { get; set; } = default!;

        [Required]
        public string Canton { get; set; } = default!;

        [Required]
        public string Parroquia { get; set; } = default!;

        public string? FotoUrl { get; set; }
    }
}
