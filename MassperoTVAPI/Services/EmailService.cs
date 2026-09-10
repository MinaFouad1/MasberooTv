using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MassperoTVAPI.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    // ── Core send ─────────────────────────────────────────────────────────────

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        => await SendInternalAsync(toEmail, toName, subject, htmlBody);

    public async Task SendWithAttachmentAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        byte[] attachmentContent,
        string attachmentFileName,
        string? attachmentContentType)
        => await SendInternalAsync(
            toEmail, toName, subject, htmlBody,
            attachmentContent, attachmentFileName, attachmentContentType);

    private async Task SendInternalAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        byte[]? attachmentContent = null,
        string? attachmentFileName = null,
        string? attachmentContentType = null)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(new MailboxAddress(toName, toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            if (attachmentContent is not null && !string.IsNullOrWhiteSpace(attachmentFileName))
                bodyBuilder.Attachments.Add(
                    attachmentFileName,
                    attachmentContent,
                    ContentType.Parse(attachmentContentType ?? "application/octet-stream"));

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            // Connect with STARTTLS (Gmail App Password, port 587)
            await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email} — Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    // ── Complaint confirmation helper ─────────────────────────────────────────

    public async Task SendComplaintConfirmationAsync(
        string toEmail,
        string toName,
        int ticketId,
        string message)
    {
        var subject = $"✅ Complaint Received — Ticket #{ticketId}";
        var htmlBody = BuildComplaintHtml(toName, ticketId, message);
        await SendAsync(toEmail, toName, subject, htmlBody);
    }


    private static string BuildComplaintHtml(string toName, int ticketId, string message)
    {
        var safeMessage = System.Net.WebUtility.HtmlEncode(message);
        var year        = DateTime.UtcNow.Year;

        return
            "<!DOCTYPE html>" +
            "<html lang=\"en\"><head><meta charset=\"UTF-8\"/><style>" +
            "body{font-family:'Segoe UI',Arial,sans-serif;background:#f4f6f9;margin:0;padding:0;}" +
            ".wrap{max-width:600px;margin:40px auto;background:#fff;border-radius:10px;overflow:hidden;" +
                  "box-shadow:0 4px 20px rgba(0,0,0,.10);}" +
            ".head{background:linear-gradient(135deg,#1a73e8,#0d47a1);padding:32px 40px;color:#fff;}" +
            ".head h1{margin:0;font-size:22px;font-weight:700;}" +
            ".head p{margin:4px 0 0;font-size:14px;opacity:.85;}" +
            ".body{padding:32px 40px;color:#374151;}" +
            ".badge{display:inline-block;background:#e8f0fe;color:#1a73e8;font-size:28px;font-weight:800;" +
                   "border-radius:8px;padding:10px 28px;margin:16px 0;letter-spacing:1px;}" +
            ".msg{background:#f9fafb;border-left:4px solid #1a73e8;border-radius:4px;" +
                 "padding:14px 18px;color:#555;font-size:14px;margin:20px 0;white-space:pre-wrap;}" +
            ".info{font-size:13px;color:#6b7280;margin-top:24px;}" +
            ".foot{background:#f4f6f9;padding:18px 40px;font-size:12px;color:#9ca3af;text-align:center;}" +
            "</style></head><body>" +
            "<div class=\"wrap\">" +
              "<div class=\"head\">" +
                "<h1>CIT — Complaint &amp; Inquiry Tracking</h1>" +
                "<p>Your complaint has been successfully submitted.</p>" +
              "</div>" +
              "<div class=\"body\">" +
                $"<p>Dear <strong>{System.Net.WebUtility.HtmlEncode(toName)}</strong>,</p>" +
                "<p>We have received your complaint. Please keep this reference number for follow-up:</p>" +
                $"<div class=\"badge\">Ticket #{ticketId}</div>" +
                "<p><strong>Your message:</strong></p>" +
                $"<div class=\"msg\">{safeMessage}</div>" +
                "<p class=\"info\">" +
                  "Our team will review your complaint and respond as soon as possible. " +
                  "You can track its progress using the ticket number above." +
                "</p>" +
              "</div>" +
              $"<div class=\"foot\">&copy; {year} CIT System &nbsp;|&nbsp; This is an automated message, please do not reply.</div>" +
            "</div>" +
            "</body></html>";
    }
}
