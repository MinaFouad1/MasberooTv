namespace MassperoTVAPI.Core.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Send a plain-text or HTML email.
    /// </summary>
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody);

    /// <summary>Sends an HTML email with an in-memory attachment.</summary>
    Task SendWithAttachmentAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        byte[] attachmentContent,
        string attachmentFileName,
        string? attachmentContentType);

    /// <summary>
    /// Convenience overload — sends the standard "complaint submitted" notification.
    /// </summary>
    Task SendComplaintConfirmationAsync(string toEmail, string toName, int ticketId, string message);
}
