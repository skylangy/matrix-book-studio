using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Matrix.Audio.Server.Common;

public class PreAuthProcessMiddleware(RequestDelegate next,
    IOptions<JwtConfig> jwtConfig,
    IOnlineUserTrackService onlineUserTrackService,
    ILogger<PreAuthProcessMiddleware> logger)
{
    private const string BearPrefix = "Bearer ";
    private readonly RequestDelegate _next = next;
    private readonly ILogger<PreAuthProcessMiddleware> _logger = logger;
    private readonly IOnlineUserTrackService _onlineUserTrackService = onlineUserTrackService;
    private readonly JwtSecurityTokenHandler _jwtTokenHandler = new();
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await ProcessUserInfo(context);
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user info");
            throw;
        }
    }

    private async Task ProcessUserInfo(HttpContext context)
    {
        try
        {
            var authorizationHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith(BearPrefix))
            {
                var token = authorizationHeader[BearPrefix.Length..].Trim();
                var principal = ValidateToken(token);
                if (principal != null)
                {
                    var userId = principal.Identity?.Name;

                    _logger.LogDebug("Processing user info for {}", userId);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _onlineUserTrackService.UpdateUserActivityAsync(userId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user info");
        }
    }

    private ClaimsPrincipal? ValidateToken(string token)
    {
        var key = Encoding.UTF8.GetBytes(_jwtConfig.Key);

        try
        {
            var principal = _jwtTokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true // Ensure token has not expired
            }, out _);

            return principal;
        }
        catch
        {
            return null; // Invalid token
        }
    }
}
