using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        // Identificador ÚNICO. 
       
        [Required]
        [MaxLength(20)]
        public string Cedula { get; set; }

        // NOTA: Eliminamos Clave y OTP. La seguridad es 100% Correo + RAM.

        [Required]
        public RolUsuario Rol { get; set; }

        [Required]
        [EmailAddress]
        public string CorreoElectronico { get; set; } // Vital para el Login

        // --- DATOS DEMOGRÁFICOS ---
        [Required]
        public string NombresCompletos { get; set; }
        [Required]
        public Genero Genero { get; set; }
        [Required]
        public string Provincia { get; set; }
        [Required]
        public string Canton { get; set; }
        [Required]
        public string Parroquia { get; set; }

        // --- RELACIONES ---

        // 1. Si es Candidato, aquí están sus datos de campaña (1 a 0..1)
        public virtual Candidato? InfoCandidatura { get; set; }

        // 2. Si ya votó, aquí se guarda el registro (para Admin, Candidato y Votante)
        public virtual ICollection<HistorialVotacion> HistorialDeVotaciones { get; set; }
    }
}
