using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Clave { get; set; } = string.Empty;

        [Required]
       
        public string Rol { get; set; } = string.Empty;// "Admin" o "Votante"
        public string FotoUrl { get; set; } = string.Empty;
    }
}
