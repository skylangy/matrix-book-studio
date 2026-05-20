namespace AudioBookStudio.Common.Models;
public class StatusTemplate
{
    public static string Has(string task, string name)
    {
        return $"Has_{task}_{name}";
    }
}
