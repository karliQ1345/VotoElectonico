using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        // La Cédula es la clave del Login. 
        // Admin: "A100200..." | Votante: "100200..."
        [Required]
        [MaxLength(20)]
        public string Cedula { get; set; }

        // Solo el Admin tiene clave hash. El votante puede tener esto null.
        public string? Clave { get; set; }

        [Required]
        public RolUsuario Rol { get; set; }

        // --- SEGURIDAD 2FA (DOBLE FACTOR) ---
        [Required]
        [EmailAddress]
        public string CorreoElectronico { get; set; } // Para enviar el código

        public string? CodigoOTP { get; set; } // El código temporal (ej: 123456)
        public DateTime? FechaExpiracionOTP { get; set; } // Validez del código

        // --- DATOS DEMOGRÁFICOS (Registro por Excel) ---
        // Estos son los datos de la persona.
        [Required]
        public string NombresCompletos { get; set; }

        [Required]
        public Genero Genero { get; set; }

        [Required]
        public string Provincia { get; set; } // Ej: "Imbabura"

        [Required]
        public string Canton { get; set; }    // Ej: "Ibarra"

        [Required]
        public string Parroquia { get; set; }

        // RELACIÓN DE AUDITORÍA: Solo sabemos SI votó, no POR QUIÉN.
        public ICollection<HistorialVotacion> HistorialDeVotaciones { get; set; }
    }
}
