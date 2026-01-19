namespace TuProyecto.DTOs.Email;

public class SendEmailDto
{
    public string ToEmail { get; set; } = null!;
    public string ToName { get; set; } = "";
    public string Subject { get; set; } = null!;
    public string HtmlContent { get; set; } = null!;

    public List<EmailAttachmentDto>? Attachments { get; set; }
}

public class EmailAttachmentDto
{
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = "application/pdf";
    public string ContentBase64 { get; set; } = null!;
}

