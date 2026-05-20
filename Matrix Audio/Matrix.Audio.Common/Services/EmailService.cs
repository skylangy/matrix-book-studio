using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Matrix.Audio.Common.Services;
public class EmailService : IEmailService
{
    private readonly SmtpConfig _smtpConfig;
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpClient _smtpClient;
    private readonly string _defaultSender;

    public EmailService(IOptions<SmtpConfig> smtpConfig,
    ILogger<EmailService> logger)
    {
        _smtpConfig = smtpConfig.Value;
        _logger = logger;

        _smtpClient = new SmtpClient(_smtpConfig.Host, _smtpConfig.Port)
        {
            Credentials = new NetworkCredential(_smtpConfig.Username, _smtpConfig.Password),
            EnableSsl = _smtpConfig.EnableSsl
        };

        _defaultSender = _smtpConfig.From;
    }

    public async Task SendEmailAsync(EmailOptions emailOptions)
    {
        if (emailOptions.To == null || emailOptions.To.Count == 0)
        {
            throw new ArgumentException("Recipient email address is required.");
        }

        var mailMessage = new MailMessage
        {
            From = new MailAddress(emailOptions.From ?? _defaultSender),
            Subject = emailOptions.Subject,
            Body = emailOptions.Body,
            IsBodyHtml = true // Assume the body supports HTML
        };

        // Add recipients
        foreach (var to in emailOptions.To)
        {
            mailMessage.To.Add(to);
        }

        // Add CC
        if (emailOptions.Cc != null)
        {
            foreach (var cc in emailOptions.Cc)
            {
                mailMessage.CC.Add(cc);
            }
        }

        // Add BCC
        if (emailOptions.Bcc != null)
        {
            foreach (var bcc in emailOptions.Bcc)
            {
                mailMessage.Bcc.Add(bcc);
            }
        }

        // Add attachments
        if (emailOptions.Attachments != null)
        {
            foreach (var attachment in emailOptions.Attachments)
            {
                using var stream = new MemoryStream(attachment.Content);
                var mailAttachment = new Attachment(stream, attachment.Filename, attachment.ContentType);
                mailMessage.Attachments.Add(mailAttachment);
            }
        }

        // Send email
        try
        {
            await _smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {}.", emailOptions.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {} with {}.", emailOptions.To, ex.Message);
        }
        finally
        {
            // Dispose attachments to release resources
            if (emailOptions.Attachments != null)
            {
                foreach (var attachment in mailMessage.Attachments)
                {
                    attachment.Dispose();
                }
            }
        }
    }
}
