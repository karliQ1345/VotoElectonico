using VotoElectonico.DTOs.Email;

namespace VotoElectonico.Services.Email;

public interface IEmailSender
{
    Task<(bool Sent, string? MessageId, string? Error)> SendAsync(SendEmailDto email, CancellationToken ct);
}

