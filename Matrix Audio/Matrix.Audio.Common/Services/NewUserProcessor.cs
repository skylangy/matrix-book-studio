using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Models;
using Microsoft.Extensions.Logging;

namespace Matrix.Audio.Common.Services;
public class NewUserProcessor(
    IEntityRepository entityRepository,
    IEmailService emailService,
    IEmailTemplateService emailTemplateService,
    ILogger<NewUserProcessor> logger) : INewUserProcessor
{
    private const int FreeTrialDays = 9;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IEmailService _emailService = emailService;
    private readonly IEmailTemplateService _emailTemplateService = emailTemplateService;
    private readonly ILogger<NewUserProcessor> _logger = logger;

    public async Task Process(string userId)
    {
        var user = await _entityRepository.GetAsync<User>(userId);

        // create 3 days trial subscription for user
        await CreateSubscription(user);

        // send email to user
        await SendEmail(user);
    }

    private async Task CreateSubscription(User user)
    {
        _logger.LogInformation("Creating subscription for user {UserId}", user.Id);
        var trialSubId = (await _entityRepository.GetAsync<AppSetting>(AppSettingKeys.AppTrialSubscriptionId)).Value;
        var subscription = await _entityRepository.GetAsync<SubscriptionPlan>(trialSubId);

        var now = DateTime.UtcNow;
        var userSubscription = new UserSubscription
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            Name = subscription.Name,
            SubscriptionId = trialSubId,
            StartDate = now,
            EndDate = now.AddDays(FreeTrialDays),
            DateCreated = now,
            IsActive = true,
            PeriodInDays = FreeTrialDays
        };
        await _entityRepository.UpdateAsync(userSubscription);

        user.Level = subscription.Level;
        await _entityRepository.UpdateAsync(user);
    }

    private async Task SendEmail(User user)
    {
        _logger.LogInformation("Sending email to user {UserId}", user.Id);

        var template = _emailTemplateService.GetTemplate(EmailTemplateNames.NewUser);
        if (template == null)
        {
            _logger.LogWarning("Email template {TemplateName} not found", EmailTemplateNames.NewUser);
            return;
        }

        var emailSubject = _emailTemplateService.ProcessPlaceholders(template.Subject, new Dictionary<string, string>
        {
            { "username", user.Name! }
        });
        var emailBody = _emailTemplateService.ProcessPlaceholders(template.Body, new Dictionary<string, string>
        {
            { "email", emailSubject },
            { "username", user.Name! }
        });

        var emailOptions = new EmailOptions
        {
            Subject = emailSubject,
            Body = emailBody,
            To = [user.Email!]
        };
        await _emailService.SendEmailAsync(emailOptions);
    }
}
