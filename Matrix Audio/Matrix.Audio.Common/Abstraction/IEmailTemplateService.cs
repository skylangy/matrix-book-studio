using Matrix.Audio.Models;

namespace Matrix.Audio.Common.Abstraction;
public interface IEmailTemplateService
{
    EmailTemplate? GetTemplate(string name);

    string ProcessPlaceholders(string body, Dictionary<string, string> placeholders);
}
