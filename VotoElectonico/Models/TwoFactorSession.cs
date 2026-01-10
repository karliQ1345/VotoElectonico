using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.Models
{
    public class TwoFactorSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; } = default!;

        [Required]
        public string CodigoHash { get; set; } = default!; // hash del OTP, no el OTP

        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiraUtc { get; set; }

        public int Intentos { get; set; } = 0;
        public bool Usado { get; set; } = false;

        public string? Ip { get; set; }
        public string? UserAgent { get; set; }
    }
}
