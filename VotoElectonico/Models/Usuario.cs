using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(10, MinimumLength = 10)]
        public string Cedula { get; set; } = default!; // ÚNICO (índice único)

        [Required]
        public RolUsuario Rol { get; set; }

        [Required, EmailAddress]
        public string CorreoElectronico { get; set; } = default!;

        // --- DATOS DEMOGRÁFICOS ---
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

        // --- FOTO PARA VERIFICACIÓN / VISUALIZACIÓN ---
        public string? FotoUrl { get; set; }

        // --- RELACIONES ---
        // 1 a 0..1: si es candidato, tiene info de candidatura
        public virtual Candidato? InfoCandidatura { get; set; }

        // Log de "votó / no votó" por proceso (NO guarda contenido del voto)
        public virtual ICollection<HistorialVotacion> HistorialDeVotaciones { get; set; } = new List<HistorialVotacion>();
    }
}
