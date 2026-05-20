using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Models;
using Microsoft.Extensions.Options;

namespace Matrix.Audio.Common.Services;
public class EmailTemplateService(
    IOptions<List<EmailTemplate>> templates
    ) : IEmailTemplateService
{
    private readonly List<EmailTemplate> _templates = templates.Value;

    public EmailTemplate? GetTemplate(string name)
    {
        return _templates.FirstOrDefault(x => x.Name == name);
    }

    public string ProcessPlaceholders(string body, Dictionary<string, string> placeholders)
    {
        foreach (var placeholder in placeholders)
        {
            body = body.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        }
        return body;
    }
}
