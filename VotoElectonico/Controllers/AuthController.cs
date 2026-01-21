using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Auth;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Email;
using VotoElectonico.Models.Enums;
using VotoElectonico.Options;
using VotoElectonico.Services.Email;
using VotoElectonico.Utils;
using VotoElectonico.Models;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _email;
        private readonly TwoFactorOptions _twoOpt;

        public AuthController(ApplicationDbContext db, IEmailSender email, IOptions<TwoFactorOptions> twoOpt)
        {
            _db = db;
            _email = email;
            _twoOpt = twoOpt.Value;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto req, CancellationToken ct)
        {
            var cedula = req.Cedula?.Trim();
            if (string.IsNullOrWhiteSpace(cedula))
                return BadRequest(ApiResponse<LoginResponseDto>.Fail("Cédula requerida."));

            var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Cedula == cedula && x.Activo, ct);
            if (user == null)
                return NotFound(ApiResponse<LoginResponseDto>.Fail("Usuario no encontrado o inactivo."));

            var roles = await _db.UsuarioRoles.Where(r => r.UsuarioId == user.Id).Select(r => r.Rol).ToListAsync(ct);
            if (roles.Count == 0)
                return BadRequest(ApiResponse<LoginResponseDto>.Fail("Usuario sin roles asignados."));

            var rolPrincipal = roles.Contains(RolTipo.Administrador) ? RolTipo.Administrador
                            : roles.Contains(RolTipo.JefeJunta) ? RolTipo.JefeJunta
                            : RolTipo.Votante;

            var requiere2FA = rolPrincipal == RolTipo.Administrador || rolPrincipal == RolTipo.JefeJunta;

            if (!requiere2FA)
            {
                // Votante sin 2FA: aquí puedes decidir si entregas token directo o solo redirect.
                // Recomendado: entregar token también.
                var resp = new LoginResponseDto
                {
                    RequiereTwoFactor = false,
                    TwoFactorSessionId = null,
                    RolPrincipal = rolPrincipal.ToString(),
                    Redirect = "/votante",
                    EmailEnmascarado = null
                };
                return Ok(ApiResponse<LoginResponseDto>.Success(resp));
            }

            // Si requiere 2FA, creamos sesión + enviamos mail
            var code = SecurityHelpers.GenerateNumericCode(_twoOpt.CodeLength);

            var ses = new TwoFactorSesion
            {
                Id = Guid.NewGuid(),
                UsuarioId = user.Id,
                Canal = TwoFactorCanal.Email,
                CodigoHash = SecurityHelpers.HashWithSalt(code),
                CreadoUtc = DateTime.UtcNow,
                ExpiraUtc = DateTime.UtcNow.AddMinutes(_twoOpt.ExpireMinutes),
                Usado = false,
                Intentos = 0,
                MaxIntentos = _twoOpt.MaxAttempts
            };

            _db.TwoFactorSesiones.Add(ses);
            await _db.SaveChangesAsync(ct);

            var html = $"<p>Tu código de verificación es: <b>{code}</b></p>";

            var send = new SendEmailDto
            {
                ToEmail = user.Email,
                ToName = user.NombreCompleto,
                Subject = _twoOpt.Subject,
                HtmlContent = html
            };

            var (sent, _, error) = await _email.SendAsync(send, ct);
            if (!sent)
                return StatusCode(500, ApiResponse<LoginResponseDto>.Fail("No se pudo enviar el 2FA: " + error));

            var resp2 = new LoginResponseDto
            {
                RequiereTwoFactor = true,
                TwoFactorSessionId = ses.Id.ToString(),
                RolPrincipal = rolPrincipal.ToString(),
                Redirect = "/twofactor",
                EmailEnmascarado = SecurityHelpers.MaskEmail(user.Email)
            };

            return Ok(ApiResponse<LoginResponseDto>.Success(resp2));
        }
    }
}
