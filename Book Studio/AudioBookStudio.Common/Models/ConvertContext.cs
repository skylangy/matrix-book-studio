using AudioBookStudio.Models.Extensions;

namespace AudioBookStudio.Common.Models;

public class ConvertContext
{
    public IEnumerable<string> Sources { get; set; }

    public string Destination { get; set; }

    public string Book { get; set; }

    public string Chapter { get; set; }

    public int BitRate { get; set; } = 192000;

    public Action<Exception> ExceptionHandler { get; set; }

    public bool IsValid()
    {
        return Sources != null
            && Sources.Any()
            && Destination.IsNotNullOrEmpty();
    }
}