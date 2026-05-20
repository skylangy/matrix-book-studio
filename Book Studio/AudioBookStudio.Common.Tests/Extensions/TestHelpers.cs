namespace AudioBookStudio.Common.Tests.Extensions;

public static class TestHelpers
{
    public static string GetResourceFile(this string name)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", name);
    }

    public static string GetOutputFile(this string name)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", name);
    }
}

