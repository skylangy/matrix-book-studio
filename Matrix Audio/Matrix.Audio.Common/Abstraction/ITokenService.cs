using Matrix.Audio.Models;
using System.Security.Claims;

namespace Matrix.Audio.Common.Abstraction;

public interface ITokenService
{
    string GenerateToken(User user, string role);

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
