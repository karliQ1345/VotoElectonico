using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Auth;
using VotoElectonico.Models;
using VotoElectonico.Options;
using VotoElectonico.Services.Auth;
using VotoElectonico.Services.Email;


namespace VotoElectonico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ITwoFactorService _twoFactor;
        private readonly IEmailSender _email;
        private readonly TwoFactorOptions _twoOpt;

        public AuthController(
            ApplicationDbContext db,
            ITwoFactorService twoFactor,
            IEmailSender email,
            IOptions<TwoFactorOptions> twoOpt)
        {
            _db = db;
            _twoFactor = twoFactor;
            _email = email;
            _twoOpt = twoOpt.Value;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthLoginResponseDto>> Login([FromBody] AuthLoginRequestDto dto, CancellationToken ct)
        {
            var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Cedula == dto.Cedula, ct);
            if (user == null) return Unauthorized("Cédula no registrada.");

            var masked = MaskEmail(user.CorreoElectronico);

            // Crear sesión 2FA
            var otp = _twoFactor.GenerateOtp(6);
            var otpHash = _twoFactor.HashOtp(otp);

            var session = new TwoFactorSession
            {
                UsuarioId = user.Id,
                CodigoHash = otpHash,
                ExpiraUtc = DateTime.UtcNow.AddMinutes(_twoOpt.CodeTtlMinutes),
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            };

            _db.TwoFactorSessions.Add(session);
            await _db.SaveChangesAsync(ct);

            // Enviar email
            var subject = "Código de verificación (2FA)";
            var body = $"Tu código de verificación es: {otp}\n\nEste código expira en {_twoOpt.CodeTtlMinutes} minutos.";
            await _email.SendAsync(user.CorreoElectronico, subject, body, ct);

            return Ok(new AuthLoginResponseDto
            {
                TwoFactorSessionId = session.Id,
                CorreoEnmascarado = masked,
                ExpiraUtc = session.ExpiraUtc
            });
        }

        [HttpPost("verify-2fa")]
        public async Task<ActionResult<AuthVerifyResponseDto>> Verify2FA([FromBody] AuthVerifyRequestDto dto, CancellationToken ct)
        {
            var session = await _db.TwoFactorSessions
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == dto.TwoFactorSessionId, ct);

            if (session == null) return Unauthorized("Sesión 2FA inválida.");
            if (session.Usado) return Unauthorized("Esta sesión 2FA ya fue usada.");
            if (DateTime.UtcNow > session.ExpiraUtc) return Unauthorized("Código expirado.");

            if (session.Intentos >= _twoOpt.MaxAttempts)
                return Unauthorized("Demasiados intentos. Vuelve a iniciar sesión.");

            var ok = _twoFactor.VerifyOtp(dto.Codigo, session.CodigoHash);
            session.Intentos++;

            if (!ok)
            {
                await _db.SaveChangesAsync(ct);
                return Unauthorized("Código incorrecto.");
            }

            session.Usado = true;
            await _db.SaveChangesAsync(ct);

            return Ok(new AuthVerifyResponseDto
            {
                UsuarioId = session.UsuarioId,
                Rol = session.Usuario.Rol.ToString(),
                AccessToken = null // luego lo cambias a JWT si quieres
            });
        }

        private static string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return "****";

            var user = parts[0];
            var domain = parts[1];

            if (user.Length <= 2) return $"**@{domain}";

            var masked = new string('*', user.Length - 2) + user[^2..];
            return $"{masked}@{domain}";
        }
    }

}
