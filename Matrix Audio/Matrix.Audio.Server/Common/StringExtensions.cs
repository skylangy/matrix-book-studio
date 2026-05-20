using System.Text;
using System.Text.RegularExpressions;

namespace Matrix.Audio.Server.Common;

public static class StringExtensions
{
    private static Random random = new Random();
    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateRandomString(this string value, int length = 6)
    {
        StringBuilder sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[random.Next(chars.Length)]);
        }
        return sb.ToString();
    }

    public static string ToSlug(this string value, string? surfix = null)
    {
        var normalizedString = value.RemoveSpecialCharacters();
        var slug = normalizedString.ToLower().Replace(" ", "-").ToLower();

        slug = Regex.Replace(slug, @"-+", "-");

        if (surfix != null)
        {
            slug = $"{slug}-{surfix}";
        }
        return slug;
    }

    public static string RemoveSpecialCharacters(this string str)
    {
        // Replace special characters with empty string
        StringBuilder sb = new StringBuilder();
        foreach (char c in str)
        {
            if (char.IsLetterOrDigit(c) || c == ' ' || c == '-')
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }


}
