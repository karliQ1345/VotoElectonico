using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.DTOs.Auth
{
    public class AuthVerifyRequestDto
    {
        [Required]
        public Guid TwoFactorSessionId { get; set; }

        [Required, StringLength(6, MinimumLength = 4)]
        public string Codigo { get; set; } = default!;
    }
    
}
