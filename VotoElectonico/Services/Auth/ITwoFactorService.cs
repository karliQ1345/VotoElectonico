using VotoElectonico.DTOs.TwoFactor;

namespace VotoElectonico.Services.Auth;

public interface ITwoFactorService
{
    Task<TwoFactorStartResponseDto> StartAsync(string usuarioId, CancellationToken ct);
    Task<TwoFactorVerifyResponseDto> VerifyAsync(TwoFactorVerifyRequestDto req, CancellationToken ct);
}

