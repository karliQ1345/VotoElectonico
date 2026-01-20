using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using VotoElectonico.DTOs.Email;
using VotoElectonico.Options;

namespace VotoElectonico.Services.Email;

public class BrevoEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly BrevoOptions _opt;

    public BrevoEmailSender(HttpClient http, IOptions<BrevoOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    public async Task<(bool Sent, string? MessageId, string? Error)> SendAsync(SendEmailDto email, CancellationToken ct)
    {
        // Endpoint oficial de Brevo (Transactional Emails)
        var url = "https://api.brevo.com/v3/smtp/email";

        var payload = new
        {
            sender = new { email = _opt.SenderEmail, name = _opt.SenderName },
            to = new[] { new { email = email.ToEmail, name = email.ToName } },
            subject = email.Subject,
            htmlContent = email.HtmlContent,
            attachment = email.Attachments?.Select(a => new
            {
                name = a.FileName,
                content = a.ContentBase64
            })
        };

        var json = JsonSerializer.Serialize(payload);

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req.Headers.Add("api-key", _opt.ApiKey);

        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var resp = await _http.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            return (false, null, body);

        // Brevo devuelve messageId normalmente en respuesta JSON
        // por seguridad parseamos soft
        string? msgId = null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("messageId", out var mid))
                msgId = mid.GetString();
        }
        catch { /* ignore */ }

        return (true, msgId, null);
    }
}

