using VotoElectonico.Models.Enums;

namespace VotoElectonico.Models
{
    public class TwoFactorSesion
    {
        public Guid Id { get; set; }

        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public TwoFactorCanal Canal { get; set; } = TwoFactorCanal.Email;

        public string CodigoHash { get; set; } = null!;     // nunca guardes el código en texto plano
        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiraUtc { get; set; }

        public bool Usado { get; set; } = false;
        public DateTime? UsadoUtc { get; set; }

        public int Intentos { get; set; } = 0;
        public int MaxIntentos { get; set; } = 5;

        public string? BrevoMessageId { get; set; }         // para trazabilidad
    }
}
