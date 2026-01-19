using VotoElectonico.Models.Enums;

namespace VotoElectonico.Models
{
    public class UsuarioRol
    {
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public RolTipo Rol { get; set; }
    }
}
