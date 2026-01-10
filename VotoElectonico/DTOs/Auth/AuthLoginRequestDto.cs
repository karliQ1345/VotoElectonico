using System.ComponentModel.DataAnnotations;

namespace VotoElectonico.DTOs.Auth
{
    public class AuthLoginRequestDto
    {
        [Required, StringLength(10, MinimumLength = 10)]
        public string Cedula { get; set; } = default!;
    }
}
