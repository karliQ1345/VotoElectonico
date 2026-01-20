using Microsoft.Extensions.Options;
using VotoElectonico.DTOs.Email;
using VotoElectonico.DTOs.TwoFactor;
using VotoElectonico.Options;
using VotoElectonico.Services.Email;

namespace VotoElectonico.Services.Auth;

public class TwoFactorService : ITwoFactorService
{
    private readonly IEmailSender _email;
    private readonly TwoFactorOptions _opt;

    public TwoFactorService(IEmailSender email, IOptions<TwoFactorOptions> opt)
    {
        _email = email;
        _opt = opt.Value;
    }

    public async Task<TwoFactorStartResponseDto> StartAsync(string usuarioId, CancellationToken ct)
    {
        // 1) Generar código (6 dígitos)
        var code = GenerateNumericCode(_opt.CodeLength);

        // 2) Guardar sesión 2FA en BD (pendiente, hash, expira)
        // TODO: aquí conectas ApplicationDbContext y guardas TwoFactorSesion

        // 3) Enviar correo con Brevo
        // TODO: obtén email real del usuario desde BD. Por ahora placeholder:
        var userEmail = "test@example.com";
        var emailMasked = MaskEmail(userEmail);

        var html = $"<h3>Código de verificación</h3><p>Tu código es: <b>{code}</b></p>";

        var send = new SendEmailDto
        {
            ToEmail = userEmail,
            Subject = _opt.Subject,
            HtmlContent = html
        };

        var (sent, messageId, error) = await _email.SendAsync(send, ct);
        if (!sent)
            throw new Exception("Error enviando 2FA: " + error);

        return new TwoFactorStartResponseDto
        {
            TwoFactorSessionId = "PENDIENTE_GUID", // aquí devuelves el Guid real de la sesión creada
            EmailEnmascarado = emailMasked,
            ExpiraEnSegundos = _opt.ExpireMinutes * 60
        };
    }

    public async Task<TwoFactorVerifyResponseDto> VerifyAsync(TwoFactorVerifyRequestDto req, CancellationToken ct)
    {
        // TODO: validar sesión (existe, no expirada, no usada, intentos)
        // TODO: comparar hash del código
        // TODO: si correcto -> marcar usada y generar JWT (si ya usas JWT)

        return new TwoFactorVerifyResponseDto
        {
            Verificado = true,
            Token = "JWT_AQUI",
            RolPrincipal = "JefeJunta",
            Redirect = "/junta"
        };
    }

    private static string GenerateNumericCode(int length)
    {
        var rnd = new Random();
        var min = (int)Math.Pow(10, length - 1);
        var max = (int)Math.Pow(10, length) - 1;
        return rnd.Next(min, max).ToString();
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return "***" + email.Substring(at);
        var prefix = email.Substring(0, 1);
        return prefix + "****" + email.Substring(at);
    }
}

