namespace Matrix.Audio.Models;
public class JwtConfig
{
    public const string JwtConfigKey = "Jwt";
    public const string IssuerKey = "Issuer";
    public const string AudienceKey = "Audience";
    public const string SecretKey = "Key";

    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string TowerToken { get; set; }
    public int ExpireHours { get; set; }
}
