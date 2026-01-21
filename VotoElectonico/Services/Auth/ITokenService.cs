using VotoElectonico.Models;

namespace VotoElectonico.Services.Auth
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(Usuario user, CancellationToken ct);
    }
}
