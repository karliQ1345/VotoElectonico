using VotoElectonico.Models;

namespace VotoElectonico.DTOs.Usuarios
{
    public class UsuarioListDto
    {
        public int Id { get; set; }
        public string Cedula { get; set; } = default!;
        public RolUsuario Rol { get; set; }

        public string CorreoElectronico { get; set; } = default!;
        public string NombresCompletos { get; set; } = default!;

        public Genero Genero { get; set; }
        public string Provincia { get; set; } = default!;
        public string Canton { get; set; } = default!;
        public string Parroquia { get; set; } = default!;

        public string? FotoUrl { get; set; }

        public bool EsCandidato { get; set; }
    }
}
