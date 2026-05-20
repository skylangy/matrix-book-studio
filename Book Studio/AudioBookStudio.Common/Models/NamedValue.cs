namespace AudioBookStudio.Models;

public class NamedValue<T>
{
    public string Name { get; set; }

    public string Icon { get; set; }

    public T Value { get; set; }
}