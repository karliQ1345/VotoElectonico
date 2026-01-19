namespace VotoElectonico.Models
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Cedula { get; set; } = null!;          // único
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Telefono { get; set; }

        // Para reportes (los que vienen del padrón)
        public string? Provincia { get; set; }
        public string? Canton { get; set; }
        public string? Parroquia { get; set; }
        public string? Genero { get; set; }                  // "M", "F", "X" o catálogo

        // Foto para verificación y visualización (URL en Supabase Storage, Azure Blob, etc.)
        public string? FotoUrl { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;

        public List<UsuarioRol> Roles { get; set; } = new();
    }
}
