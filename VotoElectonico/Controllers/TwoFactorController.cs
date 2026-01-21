using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.TwoFactor;
using VotoElectonico.Services.Auth;
using VotoElectonico.Utils;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/twofactor")]
    public class TwoFactorController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;

        public TwoFactorController(ApplicationDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("verify")]
        public async Task<ActionResult<ApiResponse<TwoFactorVerifyResponseDto>>> Verify([FromBody] TwoFactorVerifyRequestDto req, CancellationToken ct)
        {
            if (!Guid.TryParse(req.TwoFactorSessionId, out var sid))
                return BadRequest(ApiResponse<TwoFactorVerifyResponseDto>.Fail("TwoFactorSessionId inválido."));

            var ses = await _db.TwoFactorSesiones.FirstOrDefaultAsync(x => x.Id == sid, ct);
            if (ses == null) return NotFound(ApiResponse<TwoFactorVerifyResponseDto>.Fail("Sesión no encontrada."));
            if (ses.Usado) return BadRequest(ApiResponse<TwoFactorVerifyResponseDto>.Fail("Sesión ya usada."));
            if (DateTime.UtcNow > ses.ExpiraUtc) return BadRequest(ApiResponse<TwoFactorVerifyResponseDto>.Fail("Código expirado."));
            if (ses.Intentos >= ses.MaxIntentos) return BadRequest(ApiResponse<TwoFactorVerifyResponseDto>.Fail("Máximo de intentos alcanzado."));

            ses.Intentos++;

            if (!SecurityHelpers.VerifyHashWithSalt(req.Codigo.Trim(), ses.CodigoHash))
            {
                await _db.SaveChangesAsync(ct);
                return BadRequest(ApiResponse<TwoFactorVerifyResponseDto>.Fail("Código incorrecto."));
            }

            ses.Usado = true;
            ses.UsadoUtc = DateTime.UtcNow;

            var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == ses.UsuarioId, ct);
            if (user == null) return NotFound(ApiResponse<TwoFactorVerifyResponseDto>.Fail("Usuario no encontrado."));

            var token = await _tokenService.CreateTokenAsync(user, ct);
            await _db.SaveChangesAsync(ct);

            // Rol principal para el front
            var roles = await _db.UsuarioRoles.Where(r => r.UsuarioId == user.Id).Select(r => r.Rol.ToString()).ToListAsync(ct);
            var principal = roles.Contains("Administrador") ? "Administrador"
                         : roles.Contains("JefeJunta") ? "JefeJunta"
                         : "Votante";

            var redirect = principal == "Administrador" ? "/admin"
                        : principal == "JefeJunta" ? "/junta"
                        : "/votante";

            var resp = new TwoFactorVerifyResponseDto
            {
                Verificado = true,
                Token = token,
                RolPrincipal = principal,
                Redirect = redirect
            };

            return Ok(ApiResponse<TwoFactorVerifyResponseDto>.Success(resp));
        }
    }
}
