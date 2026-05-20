using Matrix.Audio.Models;

namespace Matrix.Audio.Common.Abstraction;
public interface IEmailService
{
    Task SendEmailAsync(EmailOptions emailOptions);
}
