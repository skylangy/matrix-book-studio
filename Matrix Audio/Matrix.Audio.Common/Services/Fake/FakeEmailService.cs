using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Models;
using Microsoft.Extensions.Logging;

namespace Matrix.Audio.Common.Services.Fake;
public class FakeEmailService(ILogger<FakeEmailService> logger) : IEmailService
{
    private readonly ILogger<FakeEmailService> _logger = logger;

    public Task SendEmailAsync(EmailOptions emailOptions)
    {
        _logger.LogInformation("Sending email...");
        _logger.LogInformation("From: {From}", emailOptions.From ?? "no-reply@example.com");
        _logger.LogInformation("To: {To}", string.Join(", ", emailOptions.To));
        if (emailOptions.Cc?.Count > 0)
            _logger.LogInformation("CC: {Cc}", string.Join(", ", emailOptions.Cc));
        if (emailOptions.Bcc?.Count > 0)
            _logger.LogInformation("BCC: {Bcc}", string.Join(", ", emailOptions.Bcc));
        _logger.LogInformation("Subject: {Subject}", emailOptions.Subject);
        _logger.LogInformation("Body: {Body}", emailOptions.Body);
        if (emailOptions.Attachments?.Count > 0)
        {
            _logger.LogInformation("Attachments:");
            foreach (var attachment in emailOptions.Attachments)
            {
                _logger.LogInformation("- {Filename} ({Size} bytes)", attachment.Filename, attachment.Content.Length);
            }
        }
        return Task.CompletedTask;
    }
}
