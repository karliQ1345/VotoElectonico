using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VotoElectonico.Options;

namespace VotoElectonico.Services.Email
{
    public class BrevoEmailSender : IEmailSender
    {
        private readonly HttpClient _http;
        private readonly BrevoOptions _opt;

        public BrevoEmailSender(HttpClient http, IOptions<BrevoOptions> opt)
        {
            _http = http;
            _opt = opt.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string textBody, CancellationToken ct = default)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            req.Headers.Add("api-key", _opt.ApiKey);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                sender = new { name = _opt.FromName, email = _opt.FromEmail },
                to = new[] { new { email = toEmail } },
                subject,
                textContent = textBody
            };

            var json = JsonSerializer.Serialize(payload);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"Brevo error ({(int)resp.StatusCode}): {body}");
            }
        }
    }
}
