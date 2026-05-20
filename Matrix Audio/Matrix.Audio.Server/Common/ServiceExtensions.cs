using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Common.Services.RavenDb;
using Matrix.Audio.Payment.Stripe;

namespace Matrix.Audio.Server.Common;

public static class ServiceExtensions
{
    public static IServiceCollection AddDefaultServices(this IServiceCollection services)
    {
        services.UseRavenRepositories();

        services.AddSingleton<IPasswordResetLinkGenerator, PasswordResetLinkGenerator>();
        services.AddSingleton<IPasswordEncrypter, PasswordEncrypter>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ICacheKeyService, CacheKeyService>();
        services.AddSingleton<IOrderProcessor, OrderProcessor>();
        services.AddSingleton<IMetadataProcessor, MetadataProcessor>();
        services.AddSingleton<IOnlineUserTrackService, OnlineUserTrackService>();
        services.AddSingleton<INewUserProcessor, NewUserProcessor>();
        services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
        services.AddSingleton<IEmailService, EmailService>();

        services.AddSingleton<IPaymentProcessor, PaymentProcessorStripe>();
        //services.AddSingleton<IPaymentProcessor, FakePaymentProcessor>();

        services.AddScoped<ImageHeaderFilter>();

        return services;
    }
}
