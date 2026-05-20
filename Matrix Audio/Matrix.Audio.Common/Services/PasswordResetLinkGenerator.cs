using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Matrix.Audio.Common.Services;
public class PasswordResetLinkGenerator : IPasswordResetLinkGenerator
{
    private readonly AppConfiguration _appConfiguration;
    private readonly string _baseUrl;
    private readonly string _resetEndpoint;

    public PasswordResetLinkGenerator(IOptions<AppConfiguration> appConfiguration)
    {
        _appConfiguration = appConfiguration.Value;

        _baseUrl = _appConfiguration.BaseUrl.TrimEnd('/') + "/";
        _resetEndpoint = "api/v1/auth/process/reset";
    }

    public string GenerateResetLink(string userEmail)
    {
        string token = GenerateToken();

        string resetLink = $"{_baseUrl}{_resetEndpoint}?email={Uri.EscapeDataString(userEmail)}&token={Uri.EscapeDataString(token)}";

        return resetLink;
    }

    private static string GenerateToken()
    {
        byte[] tokenBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        string token = Convert.ToBase64String(tokenBytes);
        return token;
    }
}
