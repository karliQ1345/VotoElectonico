using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.TwoFactor;
using VotoElectonico.Services.Auth;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/twofactor")]
    public class TwoFactorController : ControllerBase
    {
        private readonly ITwoFactorService _twoFactor;

        public TwoFactorController(ITwoFactorService twoFactor)
        {
            _twoFactor = twoFactor;
        }

        [HttpPost("start")]
        public async Task<ActionResult<ApiResponse<TwoFactorStartResponseDto>>> Start([FromBody] TwoFactorStartRequestDto req, CancellationToken ct)
        {
            var result = await _twoFactor.StartAsync(req.UsuarioId, ct);
            return Ok(ApiResponse<TwoFactorStartResponseDto>.Success(result));
        }

        [HttpPost("verify")]
        public async Task<ActionResult<ApiResponse<TwoFactorVerifyResponseDto>>> Verify([FromBody] TwoFactorVerifyRequestDto req, CancellationToken ct)
        {
            var result = await _twoFactor.VerifyAsync(req, ct);
            return Ok(ApiResponse<TwoFactorVerifyResponseDto>.Success(result));
        }
    }
}
