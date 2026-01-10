namespace VotoElectonico.DTOs.Auth
{
    public class AuthVerifyResponseDto
    {
        public int UsuarioId { get; set; }
        public string Rol { get; set; } = default!;
        public string? AccessToken { get; set; } // si luego metes JWT
    }
}
